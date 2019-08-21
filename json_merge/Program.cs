using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;

namespace json_merge
{
    class Program
    {
        // Set to true if we should use JSON rather than SJSON.
        static bool _use_json = false;

        // Set to true if results should be displayed in window mode.
        static bool _window = false;

        // Shows an error message.
        static void Error(string s)
        {
            Show(s, "Error");
            Environment.Exit(-1);
        }

        // Shows the message, either as a dialog in windowed mode, or on
        // the command line in command line mode.
        static void Show(string s, string title)
        {
            if (_window)
            {
                string s2 = s.Replace("\n", "\r\n");
                TextForm f = new TextForm();
                f.Text = title;
                f.DisplayText = s2;
                f.ShowDialog();
            }
            else
                Console.WriteLine(s);
        }

        // Loads a JSON or SJSON file.
        static Hashtable Load(string s)
        {
            if (!File.Exists(s))
                Error(String.Format("File not found: {0}", s));
            if (_use_json)
                return JSON.Load(s);
            else
                return SJSON.Load(s);
        }

        // Saves a JSON or SJSON file.
        static void Save(Hashtable h, string s)
        {
            if (_use_json)
                JSON.Save(h, s);
            else
                SJSON.Save(h, s);
        }

        static void Main(string[] args)
        {
            // We need to set this to prevent parse errors in JSON/SJSON parsing.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            // Print usage information if there where no arguments.
            if (args.Length == 0)
                PrintUsage();

            // -hold can be used as a debugging tool, to give a debugger the chance
            // to attach to the process when it is just starting up.
            bool escape_hold = false;

            int i = 0;
            while (i < args.Length)
            {
                if (args[i] == "-json")
                {
                    _use_json = true;
                    ++i;
                }
                else if (args[i] == "-window")
                {
                    _window = true;
                    ++i;
                }
                else if (args[i] == "-diff")
                {
                    if (i + 2 >= args.Length)
                        Error("Not enough arguments for -diff");
                    Hashtable a = Load(args[i + 1]);
                    Hashtable b = Load(args[i + 2]);
                    HashDiff diff = JsonDiff.Diff(a, b);
                    if (_window) {
                        DiffVisualForm form = new DiffVisualForm(a, b, diff, _use_json);
                        form.ShowDialog();
                    } else {
                        StringBuilder sb = new StringBuilder();
                        diff.Write(sb);
                        Show(sb.ToString(), "Diff");
                    }
                    i += 3;
                }
                else if (args[i] == "-merge")
                {
                    if (i + 4 >= args.Length)
                        Error("Not enough arguments for -merge");
                    Hashtable parent = Load(args[i + 1]);
                    Hashtable theirs = Load(args[i + 2]);
                    Hashtable mine = Load(args[i + 3]);
                    if (_window)
                    {
                        HashDiff theirs_diff, mine_diff, merged_diff;
                        Hashtable result = JsonDiff.MergeDetailed(parent, theirs, mine,
                            out theirs_diff, out mine_diff, out merged_diff);
                        MergeVisualForm form = new MergeVisualForm(parent, theirs, theirs_diff,
                            mine, mine_diff, result, merged_diff, _use_json);
                        form.ShowDialog();
                        Save(result, args[i + 4]);
                    }
                    else
                    {
                        Hashtable result = JsonDiff.Merge(parent, theirs, mine);
                        Save(result, args[i + 4]);
                    }

                    // Remove source files (should their be an option for this?)
                    File.Delete(args[i + 1]);
                    File.Delete(args[i + 2]);
                    File.Delete(args[i + 3]);
                    i += 5;
                }
                else if (args[i] == "-help")
                {
                    PrintUsage();
                    ++i;
                }
                else if (args[i] == "-hold")
                {
                    while (!escape_hold)
                        ;
                    ++i;
                }
                else if (args[i] == "-test")
                {
                    Test.TestAll();
                    ++i;
                }
            }
        }

        // Prints usage information.
        static void PrintUsage()
        {
            string usage =
@"NAME
    json_merge - compare and merge SJSON & JSON files

USAGE
    -diff FILE_A FILE_B
        prints the difference between the two SJSON files

    -merge BASE THEIRS MINE RESULT
        performs a 3-way merge between BASE, THEIRS and MINE and
        stores the result in result

    -help
        print this help text

OPTIONS
    -json
        Use JSON as input and output format (if not specified, SJSON is used).

    -window
        Display output in a window rather than in the console.";
            Show(usage, "Usage");
        }
    }
}
