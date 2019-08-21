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
    /// Form that displays the difference between two JSON files.
    /// </summary>
    public partial class DiffVisualForm : Form
    {
        // Used to format the data
        IFormatter _af, _bf;

        // Are we using _json format.
        bool _json;

        // Stores the lines that contain any differences.
        List<int> _diff_lines = new List<int>();

        // Current line in _diff_lines that is displayed.
        int _current_diff = 0;

        // Used when building _diff_lines to coalesc contigous blocks.
        int _last_diff_line = 0;
       
        /// <summary>
        /// Shows the difference between the two JSON objects a and b.
        /// Diff is the computed difference between the objects.
        /// If json is set to true, the output is showed in JSON format,
        /// otherwise SJSON format is used.
        /// </summary>
        public DiffVisualForm(Hashtable a, Hashtable b, HashDiff diff, bool json)
        {
            InitializeComponent();

            _json = json;
            if (_json) {
                _af = new JsonFormatter();
                _bf = new JsonFormatter();
            }
            else {
                _af = new SjsonFormatter();
                _bf = new SjsonFormatter();
            }

            _diff_lines.Add(0);

            // Enforce object creation (you can get a highlighting bug otherwise).
            IntPtr ah = aTextBox.Handle;
            IntPtr bh = bTextBox.Handle;

            if (_json)
            {
                SameText("{", "{");
                DisplayDiff(a, b, diff, 1);
                SameText("\n}", "\n}");
            }
            else
                DisplayDiff(a, b, diff, 0);

            _diff_lines.Add(aTextBox.Lines.Count());

            aTextBox.SelectionStart = 0;
            aTextBox.SelectionLength = 0;
            bTextBox.SelectionStart = 0;
            bTextBox.SelectionLength = 0;
        }

        // Make sure that strings a and b contain the same number of lines, padding
        // them at the end if necessary.
        private void MakeEqualLineLength(ref string a, ref string b)
        {
            int na = 0; int nb = 0;
            for (int i = 0; i < a.Length; ++i)
                if (a[i] == '\n')
                    ++na;
            for (int i = 0; i < b.Length; ++i)
                if (b[i] == '\n')
                    ++nb;
            while (na < nb)
            {
                a = a + "\n";
                ++na;
            }
            while (nb < na)
            {
                b = b + "\n";
                ++nb;
            }
        }

        // Inserts text that has been removed in the new file.
        private void RemovedText(string astr, string bstr)
        {
            if (aTextBox.Lines.Count() != _last_diff_line + 1)
                _diff_lines.Add(aTextBox.Lines.Count());
            _last_diff_line = aTextBox.Lines.Count();

            aTextBox.SelectionBackColor = Color.Pink;
            bTextBox.SelectionBackColor = Color.Pink;

            MakeEqualLineLength(ref astr, ref bstr);
            aTextBox.AppendText(astr);
            bTextBox.AppendText(bstr);

            aTextBox.SelectionBackColor = Color.White;
            bTextBox.SelectionBackColor = Color.White;
        }

        // Inserts text that has been changed in the new file.
        private void ChangedText(string astr, string bstr)
        {
            if (aTextBox.Lines.Count() != _last_diff_line + 1)
                _diff_lines.Add(aTextBox.Lines.Count());
            _last_diff_line = aTextBox.Lines.Count();

            aTextBox.SelectionBackColor = Color.Yellow;
            bTextBox.SelectionBackColor = Color.Yellow;

            MakeEqualLineLength(ref astr, ref bstr);
            aTextBox.AppendText(astr);
            bTextBox.AppendText(bstr);

            aTextBox.SelectionBackColor = Color.White;
            bTextBox.SelectionBackColor = Color.White;
        }

        // Inserts text that is the same in the old and new file.
        private void SameText(string a, string b)
        {
            aTextBox.AppendText(a);
            bTextBox.AppendText(b);
        }

        // Shows the difference between two hash tables. 
        private void DisplayDiff(Hashtable a, Hashtable b, HashDiff diff, int indent)
        {
            HashSet<string> keys = new HashSet<string>();
            foreach (string key in a.Keys) keys.Add(key);
            foreach (string key in b.Keys) keys.Add(key);

            foreach (string key in keys.OrderBy(i => i))
            {
                if (diff.Operations.ContainsKey(key))
                {
                    DiffOperation dop = diff.Operations[key];
                    if (dop is RemoveOperation)
                        RemovedText(_af.ObjectField(a, key, indent), "");
                    else if (dop is ChangeOperation)
                        ChangedText(_af.ObjectField(a, key, indent), _bf.ObjectField(b, key, indent));
                    else if (dop is ChangeObjectOperation)
                    {
                        SameText(_af.ObjectStart(key, indent), _bf.ObjectStart(key, indent));
                        DisplayDiff(a[key] as Hashtable, b[key] as Hashtable, (dop as ChangeObjectOperation).Diff, indent+1);
                        SameText(_af.ObjectEnd(indent), _bf.ObjectEnd(indent));
                    }
                    else if (dop is ChangePositionArrayOperation)
                    {
                        SameText(_af.ArrayStart(key, indent), _bf.ArrayStart(key, indent));
                        DisplayDiff(a[key] as ArrayList, b[key] as ArrayList, (dop as ChangePositionArrayOperation).Diff, indent + 1);
                        SameText(_af.ArrayEnd(indent), _bf.ArrayEnd(indent));
                    }
                    else if (dop is ChangeIdArrayOperation)
                    {
                        SameText(_af.ArrayStart(key, indent), _bf.ArrayStart(key, indent));
                        DisplayDiff(a[key] as ArrayList, b[key] as ArrayList, (dop as ChangeIdArrayOperation).Diff, indent + 1);
                        SameText(_af.ArrayEnd(indent), _bf.ArrayEnd(indent));
                    }
                }
                else
                    SameText(_af.ObjectField(a, key, indent), _bf.ObjectField(a, key, indent));
            }
        }

        // Shows the difference between two array entries.
        private void DisplayArrayDiff(object ao, object bo, DiffOperation dop, int indent)
        {
            if (dop is RemoveOperation)
                RemovedText(_af.ArrayItem(ao, indent), "");
            else if (dop is ChangeOperation)
                ChangedText(_af.ArrayItem(ao, indent), _bf.ArrayItem(bo, indent));
            else if (dop is ChangeObjectOperation)
            {
                SameText(_af.ObjectStart(indent), _bf.ObjectStart(indent));
                DisplayDiff(ao as Hashtable, bo as Hashtable, (dop as ChangeObjectOperation).Diff, indent + 1);
                SameText(_af.ObjectEnd(indent), _bf.ObjectEnd(indent));
            }
            else if (dop is ChangePositionArrayOperation)
            {
                SameText(_af.ArrayStart(indent), _bf.ArrayStart(indent));
                DisplayDiff(ao as ArrayList, bo as ArrayList, (dop as ChangePositionArrayOperation).Diff, indent + 1);
                SameText(_af.ArrayEnd(indent), _bf.ArrayEnd(indent));
            }
            else if (dop is ChangeIdArrayOperation)
            {
                SameText(_af.ArrayStart(indent), _bf.ArrayStart(indent));
                DisplayDiff(ao as ArrayList, bo as ArrayList, (dop as ChangeIdArrayOperation).Diff, indent + 1);
                SameText(_af.ArrayEnd(indent), _bf.ArrayEnd(indent));
            }
            else
                SameText(_af.ArrayItem(ao, indent), _bf.ArrayItem(bo, indent));
        }

        // Shows the difference between two arrays that use the position-merge method.
        private void DisplayDiff(ArrayList a, ArrayList b, PositionArrayDiff diff, int indent)
        {
            int n = Math.Max(a.Count, b.Count);
            for (int i=0; i<n; ++i) {
                object ao = i < a.Count ? a[i] : null;
                object bo = i < b.Count ? b[i] : null;
                DisplayArrayDiff(ao, bo, diff.Operations.GetValueOrDefault(i,null), indent);
            }
        }

        // Shows the difference between two arrays that use the id-merge method.
        private void DisplayDiff(ArrayList a, ArrayList b, HashDiff diff, int indent)
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
                DisplayArrayDiff(ao, bo, diff.Operations.GetValueOrDefault(key, null), indent);
            }
        }

        // Set if we are already processing a scroll event to prevent infinite scrolling.
        private bool _recursing = false;

        // Synchronize textbox scrolling.
        private void aTextBox_Scroll(object sender, EventArgs e)
        {
            if (_recursing) return;
            _recursing = true;
            Win32.SetScrollPos(bTextBox.Handle, Win32.GetScrollPos(aTextBox.Handle));
            _recursing = false;
        }

        // Synchronize textbox scrolling.
        private void bTextBox_Scroll(object sender, EventArgs e)
        {
            if (_recursing) return;
            _recursing = true;
            Win32.SetScrollPos(aTextBox.Handle, Win32.GetScrollPos(bTextBox.Handle));
            _recursing = false;
        }

        // Scroll to the current entry in the _diff_lines array.
        private void ScrollToCurrentDiff()
        {
            Win32.SendMessage(aTextBox.Handle, Win32.EM_LINESCROLL, 0, -100000);
            int line = _diff_lines[_current_diff] - 10;
            if (line < 0)
                line = 0;
            Win32.SendMessage(aTextBox.Handle, Win32.EM_LINESCROLL, 0, line);
            Win32.SetScrollPos(bTextBox.Handle, Win32.GetScrollPos(aTextBox.Handle));
        }

        // Scroll to next entry in the _diff_lines array.
        private void nextDifferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ++_current_diff;
            if (_current_diff >= _diff_lines.Count)
                _current_diff = _diff_lines.Count - 1;
            ScrollToCurrentDiff();
        }

        // Scroll to the previous entry in the _diff_lines array.
        private void previousDifferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            --_current_diff;
            if (_current_diff < 0)
                _current_diff = 0;
            ScrollToCurrentDiff();
        }
    }
}
