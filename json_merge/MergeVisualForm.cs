using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace json_merge
{
    /// <summary>
    /// Form that displays the result of a merge operation visually. Note that currently there
    /// are no ways of affecting the outcome of the merge, the display is just for information.
    /// </summary>
    public partial class MergeVisualForm : Form
    {
        // Formatters for the a, b and c objects.
        IFormatter _af, _bf, _cf;

        // True if json formatting is used.
        bool _json;

        // Dictionary that maps key paths to the line number where that key can be found.
        // This is used to match up line numbers in the different views in a rather inefficient
        // way (see below).
        Dictionary<string, int> _line_number = new Dictionary<string, int>();

        // Set if a change has been made to the _line_number dictionary.
        bool _line_numbers_changed;

        // A list of the lines where differences have been found, used for prev/next jumping.
        List<int> _diff_lines = new List<int>();

        // The current difference that is highlighted.
        int _current_diff = 0;
        
        // Stores the diff_lines during document generation.
        HashSet<int> _diff_lines_set = new HashSet<int>();
        int _last_diff_line = 0;

        public MergeVisualForm(Hashtable parent, Hashtable a, HashDiff adiff, 
            Hashtable b, HashDiff bdiff, Hashtable c, HashDiff cdiff, bool json)
        {
            InitializeComponent();

            _json = json;
            if (_json)
            {
                _af = new JsonFormatter();
                _bf = new JsonFormatter();
                _cf = new JsonFormatter();
            }
            else
            {
                _af = new SjsonFormatter();
                _bf = new SjsonFormatter();
                _cf = new SjsonFormatter();
            }

            // Enforce object creation (you can get a highlighting bug otherwise).
            IntPtr ah = aTextBox.Handle;
            IntPtr bh = bTextBox.Handle;
            IntPtr ch = cTextBox.Handle;

            // This while loop is a rather ugly thing.
            //
            // We use the _line_number Dictionary to store the highest line number where
            // a particular key path (e.g. items.size.x) have been displayed in any of the
            // views. When a view wants to display a particular key, it pads the text with
            // newlines so that the key appears at the maximum line number where it has
            // appeared before. If the current line number is higher than the previous
            // maximum, the _line_number Dictionary is updated with the new maximum and
            // _line_numbers_changed is true.
            //
            // By looping here and reformatting the text until _line_number is no longer
            // changing we ensure that all keys are displayed at the same line in all three
            // views, so that we can scroll them simultaneously.
            //
            // In theory, this could take quite some time for the worst-case scenario and
            // it would be better to rewrite the code so that we are formatting the three views
            // simultaneously, padding with newlines as we go along. However, that is rather
            // hairy to write and this seems to work well for now.
            do
            {
                _diff_lines_set.Clear();

                aTextBox.Text = "";
                bTextBox.Text = "";
                cTextBox.Text = "";
                _line_numbers_changed = false;
                int indent = 0;

                if (_json)
                {
                    SameText(aTextBox, "{");
                    SameText(bTextBox, "{");
                    SameText(cTextBox, "{");
                    indent = 1;
                }

                DisplayDiff(aTextBox, _af, parent, a, adiff, indent, "");
                DisplayDiff(bTextBox, _bf, parent, b, bdiff, indent, "");
                DisplayDiff(cTextBox, _cf, parent, c, cdiff, indent, "");

                if (_json)
                {
                    SameText(aTextBox, "\n}");
                    SameText(bTextBox, "\n}");
                    SameText(cTextBox, "\n}");
                }
            } while (_line_numbers_changed);

            _diff_lines_set.Add(0);
            _diff_lines_set.Add(aTextBox.Lines.Count());
            _diff_lines = _diff_lines_set.OrderBy(i => i).ToList();
            _current_diff = 0;

            aTextBox.SelectionStart = 0;
            aTextBox.SelectionLength = 0;
            bTextBox.SelectionStart = 0;
            bTextBox.SelectionLength = 0;
            cTextBox.SelectionStart = 0;
            cTextBox.SelectionLength = 0;
        }

        // Shows unchanged text in the textbox.
        private void SameText(RichTextBox rtb, string s)
        {
            rtb.AppendText(s);
        }

        // Shows removed text in the text box. I would like to use strikeout for removed
        // text, but I can't find a way to set that for the RichTextBox.
        private void RemovedText(RichTextBox rtb, string s)
        {
            if (aTextBox.Lines.Count() != _last_diff_line + 1)
                _diff_lines_set.Add(aTextBox.Lines.Count());
            _last_diff_line = aTextBox.Lines.Count();

            int start = rtb.Lines.Count();
            rtb.SelectionBackColor = Color.Pink;
            rtb.AppendText(s);
            rtb.SelectionBackColor = Color.White;
        }

        // Shows changed text in the text box.
        private void ChangedText(RichTextBox rtb, string before, string after)
        {
            if (aTextBox.Lines.Count() != _last_diff_line + 1)
                _diff_lines_set.Add(aTextBox.Lines.Count());
            _last_diff_line = aTextBox.Lines.Count();

            int start = rtb.Lines.Count();
            rtb.SelectionBackColor = Color.Pink;
            rtb.AppendText(before);
            rtb.SelectionBackColor = Color.Yellow;
            rtb.AppendText(after);
            rtb.SelectionBackColor = Color.White;
        }

        // Checks the line number for the key path. If it has been displayed before at
        // a higher line number, we pad with newlines to get the key at the same line.
        // If it has been displayed before at a lower line number, we update the dictionary
        // with the current line number and specify that the texts must be regenerated
        // so that line numbers match in all three displays.
        private void CheckLineNumber(RichTextBox rtb, string path)
        {
            int line_no = _line_number.GetValueOrDefault(path, 0);
            if (rtb.Lines.Count() > line_no)
            {
                _line_numbers_changed = true;
                line_no = rtb.Lines.Count();
                _line_number[path] = line_no;
            }
            while (rtb.Lines.Count() < line_no)
                rtb.AppendText("\n");
        }

        // Shows the difference between a and b in the text box.
        private void DisplayDiff(RichTextBox rtb, IFormatter f, Hashtable a, Hashtable b, HashDiff diff, 
            int indent, string path)
        {
            HashSet<string> keys = new HashSet<string>();
            foreach (string key in a.Keys) keys.Add(key);
            foreach (string key in b.Keys) keys.Add(key);

            foreach (string key in keys.OrderBy(i => i))
            {
                string subpath = path + "." + key;
                CheckLineNumber(rtb, subpath);

                if (diff.Operations.ContainsKey(key))
                {
                    DiffOperation dop = diff.Operations[key];
                    if (dop is RemoveOperation)
                        RemovedText(rtb, f.ObjectField(a, key, indent));
                    else if (dop is ChangeOperation)
                        ChangedText(rtb, f.ObjectField(a, key, indent), f.ObjectField(b, key, indent));
                    else if (dop is ChangeObjectOperation)
                    {
                        SameText(rtb, f.ObjectStart(key, indent));
                        DisplayDiff(rtb, f, a[key] as Hashtable, b[key] as Hashtable, (dop as ChangeObjectOperation).Diff, indent + 1, subpath);
                        SameText(rtb, f.ObjectEnd(indent));
                    }
                    else if (dop is ChangePositionArrayOperation)
                    {
                        SameText(rtb, f.ArrayStart(key, indent));
                        DisplayDiff(rtb, f, a[key] as ArrayList, b[key] as ArrayList, (dop as ChangePositionArrayOperation).Diff, indent + 1, subpath);
                        SameText(rtb, f.ArrayEnd(indent));
                    }
                    else if (dop is ChangeIdArrayOperation)
                    {
                        SameText(rtb, f.ArrayStart(key, indent));
                        DisplayDiff(rtb, f, a[key] as ArrayList, b[key] as ArrayList, (dop as ChangeIdArrayOperation).Diff, indent + 1, subpath);
                        SameText(rtb, f.ArrayEnd(indent));
                    }
                }
                else
                    SameText(rtb, f.ObjectField(b, key, indent));
            }
        }

        // Shows the difference between ao and bo in the text box.
        private void DisplayArrayDiff(RichTextBox rtb, IFormatter f, object ao, object bo, DiffOperation dop,
            int indent, string path)
        {
            CheckLineNumber(rtb, path);
            if (dop is RemoveOperation)
                RemovedText(rtb, f.ArrayItem(ao, indent));
            else if (dop is ChangeOperation)
                ChangedText(rtb, f.ArrayItem(ao, indent), f.ArrayItem(bo, indent));
            else if (dop is ChangeObjectOperation)
            {
                SameText(rtb, f.ObjectStart(indent));
                DisplayDiff(rtb, f, ao as Hashtable, bo as Hashtable, (dop as ChangeObjectOperation).Diff, indent + 1, path);
                SameText(rtb, f.ObjectEnd(indent));
            }
            else if (dop is ChangePositionArrayOperation)
            {
                SameText(rtb, f.ArrayStart(indent));
                DisplayDiff(rtb, f, ao as ArrayList, bo as ArrayList, (dop as ChangePositionArrayOperation).Diff, indent + 1, path);
                SameText(rtb, f.ArrayEnd(indent));
            }
            else if (dop is ChangeIdArrayOperation)
            {
                SameText(rtb, f.ArrayStart(indent));
                DisplayDiff(rtb, f, ao as ArrayList, bo as ArrayList, (dop as ChangeIdArrayOperation).Diff, indent + 1, path);
                SameText(rtb, f.ArrayEnd(indent));
            }
            else
                SameText(rtb, f.ArrayItem(ao, indent));
        }

        // Shows the difference between a and b in the text box.
        private void DisplayDiff(RichTextBox rtb, IFormatter f, ArrayList a, ArrayList b, PositionArrayDiff diff,
            int indent, string path)
        {
            int n = Math.Max(a.Count, b.Count);
            for (int i = 0; i < n; ++i)
            {
                object ao = i < a.Count ? a[i] : null;
                object bo = i < b.Count ? b[i] : null;
                string subpath = string.Format("{0}.{1}", path, i);
                DisplayArrayDiff(rtb, f, ao, bo, diff.Operations.GetValueOrDefault(i, null), indent, subpath);
            }
        }

        // Shows the difference between a and b in the text box.
        private void DisplayDiff(RichTextBox rtb, IFormatter f, ArrayList a, ArrayList b, HashDiff diff,
            int indent, string path)
        {
            HashSet<object> keys = new HashSet<object>();
            foreach (object h in a)
                keys.Add(Id.GetId(h));
            foreach (object h in b)
                keys.Add(Id.GetId(h));

            foreach (object key in keys.OrderBy(i => i))
            {
                object ao = Id.FindObjectWithId(a, key);
                object bo = Id.FindObjectWithId(b, key);
                string subpath = string.Format("{0}.{1}", path, key);
                DisplayArrayDiff(rtb, f, ao, bo, diff.Operations.GetValueOrDefault(key, null), indent, subpath);
            }
        }

        // Prevents infinite recursion in scroll events.
        private bool _recursing = false;

        // Implements syncrhonized scrolling.
        private void aTextBox_Scroll(object sender, EventArgs e)
        {
            if (_recursing) return;
            _recursing = true;
            Win32.SetScrollPos(bTextBox.Handle, Win32.GetScrollPos(aTextBox.Handle));
            Win32.SetScrollPos(cTextBox.Handle, Win32.GetScrollPos(aTextBox.Handle));
            _recursing = false;
        }

        // Implements syncrhonized scrolling.
        private void bTextBox_Scroll(object sender, EventArgs e)
        {
            if (_recursing) return;
            _recursing = true;
            Win32.SetScrollPos(aTextBox.Handle, Win32.GetScrollPos(bTextBox.Handle));
            Win32.SetScrollPos(cTextBox.Handle, Win32.GetScrollPos(bTextBox.Handle));
            _recursing = false;
        }

        // Implements syncrhonized scrolling.
        private void cTextBox_Scroll(object sender, EventArgs e)
        {
            if (_recursing) return;
            _recursing = true;
            Win32.SetScrollPos(aTextBox.Handle, Win32.GetScrollPos(cTextBox.Handle));
            Win32.SetScrollPos(bTextBox.Handle, Win32.GetScrollPos(cTextBox.Handle));
            _recursing = false;
        }

        // Scrolls to the current item in the _diff_lines array.
        private void ScrollToCurrentDiff()
        {
            Win32.SendMessage(aTextBox.Handle, Win32.EM_LINESCROLL, 0, -100000);
            int line = _diff_lines[_current_diff] - 10;
            if (line < 0)
                line = 0;
            Win32.SendMessage(aTextBox.Handle, Win32.EM_LINESCROLL, 0, line);
            Win32.SetScrollPos(bTextBox.Handle, Win32.GetScrollPos(aTextBox.Handle));
            Win32.SetScrollPos(cTextBox.Handle, Win32.GetScrollPos(aTextBox.Handle));
        }

        // Scrolls to the next item in the _diff_lines array.
        private void nextDifferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ++_current_diff;
            if (_current_diff >= _diff_lines.Count)
                _current_diff = _diff_lines.Count - 1;
            ScrollToCurrentDiff();
        }

        // Scrolls to the previous item in the _diff_lines array.
        private void previousDifferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            --_current_diff;
            if (_current_diff < 0)
                _current_diff = 0;
            ScrollToCurrentDiff();
        }
    }
}
