using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace json_merge
{
    /// <summary>
    /// Some simple tests of the functionality.
    /// </summary>
    class Test
    {
        static Hashtable Sjson(string s)
        {
            return SJSON.Decode(Encoding.UTF8.GetBytes(s));
        }

        static void TestValidDiff(Hashtable a, Hashtable b, HashDiff diff)
        {
            diff.Apply(a);
            HashDiff newdiff = JsonDiff.Diff(a, b);
            System.Diagnostics.Debug.Assert(newdiff.Empty());
        }

        static void PrintDiff(HashDiff diff)
        {
            StringBuilder sb = new StringBuilder();
            diff.Write(sb);
            System.Diagnostics.Debug.WriteLine(sb.ToString());
        }

        public static void TestAll()
        {
            TestPlain();
            TestObject();
            TestIdArray();
            TestArray();
            TestMerge();
        }

        static void TestDiff(string s_a, string s_b)
        {
            Hashtable a = Sjson(s_a);
            Hashtable b = Sjson(s_b);

            HashDiff diff = JsonDiff.Diff(a, b);
            PrintDiff(diff);
            TestValidDiff(a, b, diff);
        }

        static void TestMerge(string s_parent, string s_left, string s_right)
        {
            Hashtable parent = Sjson(s_parent);
            Hashtable left = Sjson(s_left);
            Hashtable right = Sjson(s_right);

            Hashtable res = JsonDiff.Merge(parent, left, right);
            System.Diagnostics.Debug.WriteLine(SJSON.Encode(res));
        }

        static void TestPlain()
        {
            TestDiff(
                @"a = 2 b = 3 c = 4 sa = ""kaka"" sb = ""saka"" ",
                @"a = 2 b = 4 d = 5 sa = ""kaka"" sb = ""maka"" ");
        }

        static void TestObject()
        {
            TestDiff(
                @"a = 2 b = 3 c = 4 oa = {a = 1 b = 3} ob = {a = ""kaka""} ",
                @"a = 2 b = 4 d = 5 oa = {a = 1 c = 5} oc = {a = ""maka""} ");
        }

        static void TestIdArray()
        {
            TestDiff(
                @"a = [{id=""1""}, {id=""2""}, {id=""3""}] b = [{a=1, id=1} {a=2,id=2}] c = []",
                @"a = [{id=""1""}, {id=""3""}] b = [{a=3,id=1} {a=2, id=2}] d = [{id=""1""} {id=""2""}]"
            );
        }

        static void TestArray()
        {
            TestDiff(
                @"a = [{a=1 b=2} {a=1 b=3}] b = [[1 2] [3 4 5]]",
                @"a = [{a=1 b=2} {a=1 b=3}] b = [[1 2 3] [4 5]]"
            );

            TestDiff(
                @"a = [1, 2, 3] b = [{a=1} {a=2}] c = [] e = [3 7 3 7]",
                @"a = [1, 3] b = [{a=3} {a=2}] d = [1 2] e = [7 3 1 7 3]"
            );
        }

        static void TestMerge()
        {
            TestMerge(
                @" c = {a = 1 b = 2 c = 3} name = ""niklas"" ",
                @" c = {a = 1 b = 2 d = 4} name = ""niklas"" ",
                @" c = {a = 1 b = 2 c = 3 e = 5} name = ""marian"" ");

            TestMerge(
                @" a = [1 2 3] b = ""niklas"" c = {a = 1 b = 2 c = 3} o = [ {id=1} {id=2} {id=3} ]",
                @" a = [1 3] b = ""niklas"" c = {a = 1 b = 2 d = 4} o = [ {id=1} {id=3} {id=4} ]",
                @" a = [1 2 3 4] b = ""marian"" c = {a = 1 b = 2 c = 3 e = 5} o = [ {id=1} {id=2} {id=6} ] ");
        }
    }
}
