using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace json_merge
{
    /// <summary>
    /// Helper class for dealing with id-arrays. ID-arrays are arrays where the entries
    /// are hash tables that contain an "id" field that uniquely identifies the entry
    /// with a GUID.
    /// 
    /// Id-arrays can be merged better than regular arrays. Since each entry in an
    /// id-array has a unique identity it is easy to tell if an item has been moved,
    /// if an existing item has been changed or if a new item has been created.
    /// 
    /// Arrays that don't have IDs are called position arrays and are harder to diff and merge.
    /// For example consider [1 2 3] -> [1 5 2] we can't tell semantically whether the
    /// 2 has changed to 5 and 3 has changed to 2... or if the 2 has been moved and 3 changed
    /// to 5... or if 2 and 3 have been removed and 5 2 inserted, etc, etc.
    /// 
    /// For best merge results, when you have control over the file format, always use id arrays
    /// when you want to preserve item identity.
    /// </summary>
    public static class Id
    {
        /// <summary>
        /// Returns true if the hashtable contains an id field. Currently id fields can use one
        /// of the keys: id, Id, ID. I suggest you modify this if you want to use other keys for
        /// id fields.
        /// </summary>
        public static bool ContainsId(Hashtable a)
        {
            return a.ContainsKey("id") || a.ContainsKey("Id") || a.ContainsKey("ID");
        }

        /// <summary>
        /// Returns the id of an object or null if it doesn't has one. The id is either the
        /// id field. Or, if the object is a string, the id is the string itself. (So string arrays
        /// will merge as id arrays with the ids being the strings themselves.)
        /// </summary>
        public static object GetId(object a)
        {
            if (a is Hashtable)
            {
                Hashtable h = (a as Hashtable);
                if (h.ContainsKey("id")) return h["id"];
                if (h.ContainsKey("Id")) return h["Id"];
                if (h.ContainsKey("ID")) return h["ID"];
            }
            if (a is string)
                return a;
            return null;
        }

        /// <summary>
        /// Finds the object with the specified id in the array and returns it.
        /// </summary>
        public static object FindObjectWithId(ArrayList a, object id)
        {
            foreach (object o in a)
                if (id.Equals(GetId(o)))
                    return o;
            return null;
        }

        /// <summary>
        /// Finds the index of object with the specified id in the array and returns it.
        /// </summary>
        public static int FindIndexWithId(ArrayList a, object id)
        {
            for (int i=0; i<a.Count; ++i)
                if (id.Equals(GetId(a[i])))
                    return i;
            return -1;
        }
    }

    /// <summary>
    /// Abstract representation of a DiffOperation that is part of a computed diff.
    /// The DiffOperation is applied to a particular key in a Hashtable or an ArrayList.
    /// </summary>
    public abstract class DiffOperation
    {
        /// <summary>
        /// Writes a textual representation of the operation to the string builder.
        /// </summary>
        abstract public void Write(StringBuilder sb, string context);

        /// <summary>
        /// Applies the diff operation to the item at the key in the Hashtable.
        /// </summary>
        abstract public void Apply(Hashtable h, string key);

        /// <summary>
        /// Applies the diff operation to the item at the key in the ArrayList.
        /// </summary>
        abstract public void Apply(ArrayList a, int key);

        /// <summary>
        /// Merges two Diff operation into a single operation.
        /// </summary>
        static public DiffOperation Merge(DiffOperation left, DiffOperation right)
        {
            // Remove always trumps other operations.
            if (right is RemoveOperation)
                return right;
            if (left is RemoveOperation)
                return left;

            // Change operation trumps other operations and right changes take precedence over
            // left changes.
            if (right is ChangeOperation)
                return right;
            if (left is ChangeOperation)
                return left;

            // Recursively merge array and object operations.
            System.Diagnostics.Debug.Assert(left.GetType() == right.GetType());
            if (left is ChangeObjectOperation)
                return ChangeObjectOperation.Merge(left as ChangeObjectOperation, right as ChangeObjectOperation);
            if (left is ChangePositionArrayOperation)
                return ChangePositionArrayOperation.Merge(left as ChangePositionArrayOperation, right as ChangePositionArrayOperation);
            if (left is ChangeIdArrayOperation)
                return ChangeIdArrayOperation.Merge(left as ChangeIdArrayOperation, right as ChangeIdArrayOperation);

            System.Diagnostics.Debug.Assert(false);
            return null;
        }
    }

    /// <summary>
    /// A diff operation that removes a key.
    /// </summary>
    class RemoveOperation : DiffOperation
    {
        public RemoveOperation()
        {
        }

        public override void Write(StringBuilder sb, string key)
        {
            sb.AppendFormat("{0} = null\n", key);
        }

        public override void Apply(Hashtable h, string key)
        {
            h.Remove(key);
        }

        public override void Apply(ArrayList a, int key)
        {
            a[key] = null;
        }
    }

    /// <summary>
    /// A diff operation that changes the value of a key.
    /// (Also used for adding keys.)
    /// </summary>
    class ChangeOperation : DiffOperation
    {
        object _value;
        public ChangeOperation(object value)
        {
            _value = value;
        }

        public override void Write(StringBuilder sb, string key)
        {
            sb.AppendFormat("{0} = {1}\n", key, SJSON.EncodeObject(_value));
        }

        public override void Apply(Hashtable h, string key)
        {
            h[key] = _value;
        }

        public override void Apply(ArrayList a, int key)
        {
            while (key >= a.Count)
                a.Add(null);
            a[key] = _value;
        }
    }

    /// <summary>
    ///  A diff operation that applies a set of changes to an object stored
    ///  in the value of a key.
    /// </summary>
    class ChangeObjectOperation : DiffOperation
    {
        public HashDiff Diff;
        public ChangeObjectOperation(HashDiff diff)
        {
            Diff = diff;
        }

        public override void Write(StringBuilder sb, string key)
        {
            Diff.Write(sb, key);
        }

        public override void Apply(Hashtable h, string key)
        {
            Diff.Apply(h[key] as Hashtable);
        }

        public override void Apply(ArrayList a, int key)
        {
            Diff.Apply(a[key] as Hashtable);
        }

        public static ChangeObjectOperation Merge(ChangeObjectOperation left, ChangeObjectOperation right)
        {
            return new ChangeObjectOperation(HashDiff.Merge(left.Diff, right.Diff));
        }
    }

    /// <summary>
    ///  A diff operation that applies a set of changes to an arraylist with objects identified
    ///  by id.
    /// </summary>
    class ChangeIdArrayOperation : DiffOperation
    {
        public HashDiff Diff;
        public ChangeIdArrayOperation(HashDiff diff)
        {
            Diff = diff;
        }

        public override void Write(StringBuilder sb, string key)
        {
            Diff.Write(sb, key);
        }

        public override void Apply(Hashtable h, string key)
        {
            Diff.ApplyId(h[key] as ArrayList);
            FilterNull(h[key] as ArrayList);
        }

        public override void Apply(ArrayList a, int key)
        {
            Diff.ApplyId(a[key] as ArrayList);
            FilterNull(a[key] as ArrayList);
        }

        private void FilterNull(ArrayList a)
        {
            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] == null)
                {
                    a.RemoveAt(i);
                    --i;
                }
            }
        }

        public static ChangeIdArrayOperation Merge(ChangeIdArrayOperation left, ChangeIdArrayOperation right)
        {
            return new ChangeIdArrayOperation(HashDiff.Merge(left.Diff, right.Diff));
        }
    }

    // <summary>
    ///  A diff operation that applies a set of changes to an array stored
    ///  in the value of a key.
    /// </summary>
    class ChangePositionArrayOperation : DiffOperation
    {
        /// <summary>
        /// Stores the differences between the array items once the mapping has
        /// been applied.
        /// </summary>
        public PositionArrayDiff Diff;

        public ChangePositionArrayOperation(PositionArrayDiff diff)
        {
            Diff = diff;
        }

        public override void Write(StringBuilder sb, string key)
        {
            Diff.Write(sb, key);
        }

        public override void Apply(Hashtable h, string key)
        {
            Diff.Apply(h[key] as ArrayList);
            FilterNull(h[key] as ArrayList);
        }

        public override void Apply(ArrayList a, int key)
        {
            Diff.Apply(a[key] as ArrayList);
            FilterNull(a[key] as ArrayList);
        }

        private void FilterNull(ArrayList a)
        {
            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] == null)
                {
                    a.RemoveAt(i);
                    --i;
                }
            }
        }

        public static ChangePositionArrayOperation Merge(ChangePositionArrayOperation left, ChangePositionArrayOperation right)
        {
           PositionArrayDiff diff = PositionArrayDiff.Merge(left.Diff, right.Diff);
            return new ChangePositionArrayOperation(diff);
        }
    }

    /// <summary>
    /// Class that represents the difference between two hash tables.
    /// </summary>
    public class HashDiff
    {
        /// <summary>
        /// We store the difference as a DiffOperation for each changed key in the hashtable.
        /// </summary>
        public Dictionary<object, DiffOperation> Operations = new Dictionary<object, DiffOperation>();

        public void Write(StringBuilder sb, params string[] context)
        {
            string c = context.Length > 0 ? context[0] + "." : "";
            foreach (var kvp in Operations)
                kvp.Value.Write(sb, c + kvp.Key);
        }

        public void Apply(Hashtable h)
        {
            foreach (var kvp in Operations)
                kvp.Value.Apply(h, kvp.Key as string);
        }

        public void ApplyId(ArrayList a)
        {
            foreach (var kvp in Operations)
            {
                int index = Id.FindIndexWithId(a, kvp.Key);
                if (index == -1)
                {
                    a.Add(null);
                    index = a.Count - 1;
                }
                kvp.Value.Apply(a, index);
            }
        }

        public bool Empty()
        {
            return Operations.Count == 0;
        }

        /// <summary>
        /// Merges two HashDifferences to a unified difference.
        /// </summary>
        public static HashDiff Merge(HashDiff left, HashDiff right)
        {
            // Find the combined key list.
            List<object> keys = left.Operations.Keys.ToList<object>();
            foreach (var kvp in right.Operations)
                if (!left.Operations.ContainsKey(kvp.Key))
                    keys.Add(kvp.Key);

            // Merge the operations for each key.
            HashDiff h = new HashDiff();
            foreach (object key in keys)
            {
                if (left.Operations.ContainsKey(key) && right.Operations.ContainsKey(key))
                    h.Operations[key] = DiffOperation.Merge(left.Operations[key], right.Operations[key]);
                else if (left.Operations.ContainsKey(key))
                    h.Operations[key] = left.Operations[key];
                else if (right.Operations.ContainsKey(key))
                    h.Operations[key] = right.Operations[key];
            }
            return h;
        }
    }

     /// <summary>
    /// Class that represents the difference between two arrays where elements are identified by
    /// their position in the array.
    /// </summary>
    public class PositionArrayDiff
    {
        /// <summary>
        /// The difference is stored as a difference operation for each item in the array.
        /// </summary>
        public Dictionary<int, DiffOperation> Operations = new Dictionary<int, DiffOperation>();

        public void Write(StringBuilder sb, params string[] context)
        {
            string c = context.Length > 0 ? context[0] : "";
            foreach (var kvp in Operations)
                kvp.Value.Write(sb, c + "[" + kvp.Key + "]");
        }

        public void Apply(ArrayList a)
        {
            foreach (var kvp in Operations)
                kvp.Value.Apply(a, kvp.Key);
        }

        public bool Empty()
        {
            return Operations.Count == 0;
        }

        /// <summary>
        /// Merges two array differences to a unified difference.
        /// </summary>
        public static PositionArrayDiff Merge(PositionArrayDiff left, PositionArrayDiff right)
        {
            List<int> keys = left.Operations.Keys.ToList<int>();
            foreach (var kvp in right.Operations)
                if (!left.Operations.ContainsKey(kvp.Key))
                    keys.Add(kvp.Key);

            PositionArrayDiff h = new PositionArrayDiff();
            foreach (int key in keys)
            {
                if (left.Operations.ContainsKey(key) && right.Operations.ContainsKey(key))
                    h.Operations[key] = DiffOperation.Merge(left.Operations[key], right.Operations[key]);
                else if (left.Operations.ContainsKey(key))
                    h.Operations[key] = left.Operations[key];
                else if (right.Operations.ContainsKey(key))
                    h.Operations[key] = right.Operations[key];
            }
            return h;
        }
    }

    /// <summary>
    /// Class for computing Json and Sjson differences.
    /// </summary>
    class JsonDiff
    {
        /// <summary>
        /// Finds the difference operation between two Json value items.
        /// </summary>
        private static DiffOperation DiffValue(object first, object second)
        {
            if (first == null && second == null)
                return null;
            else if (second == null)
                return new RemoveOperation();
            else if (first == null)
                return new ChangeOperation(second);
            else if (first.GetType() != second.GetType())
                return new ChangeOperation(second);
            else if (first is double)
                return (double)first == (double)second ? null : new ChangeOperation(second);
            else if (first is bool)
                return (bool)first == (bool)second ? null : new ChangeOperation(second);
            else if (first is string)
                return (string)first == (string)second ? null : new ChangeOperation(second);
            else if (first is Hashtable)
            {
                HashDiff subdiff = Diff(first as Hashtable, second as Hashtable);
                return subdiff.Empty() ? null : new ChangeObjectOperation(subdiff);
            }
            else if (first is ArrayList)
            {
                if (AreIdArrays(first as ArrayList, second as ArrayList))
                {
                    HashDiff subdiff = IdDiff(first as ArrayList, second as ArrayList);
                    return subdiff.Empty() ? null : new ChangeIdArrayOperation(subdiff);
                }
                else
                {
                    PositionArrayDiff subdiff = PositionDiff(first as ArrayList, second as ArrayList);
                    return subdiff.Empty() ? null : new ChangePositionArrayOperation(subdiff);
                }
            }
            else
                throw new System.Exception("error");
        }

        /// <summary>
        /// Returns true if a and b are "id"-arrays, where the elements are identified by id rather
        /// than by position.
        /// 
        /// An array is an id array if either all objects in the array have id-fields or if all objects
        /// are strings.
        /// </summary>
        private static bool AreIdArrays(ArrayList a, ArrayList b)
        {
            bool all_contains_id = true;
            bool all_string = true;

            foreach (object o in a)
            {
                if (!(o is string))
                    all_string = false;
                if (!(o is Hashtable) || !Id.ContainsId(o as Hashtable))
                    all_contains_id = false;
            }

            foreach (object o in b)
            {
                if (!(o is string))
                    all_string = false;
                if (!(o is Hashtable) || !Id.ContainsId(o as Hashtable))
                    all_contains_id = false;
            }

            return all_contains_id || all_string;
        }

        /// <summary>
        /// Finds the difference between two Json hashtables.
        /// </summary>
        public static HashDiff Diff(Hashtable first, Hashtable second)
        {
            HashDiff diff = new HashDiff();

            foreach (DictionaryEntry de in first)
            {
                string key = (de.Key as string);
                object first_value = de.Value;
                object second_value = second.ContainsKey(key) ? second[key] : null;

                DiffOperation op = DiffValue(first_value, second_value);
                if (op != null)
                    diff.Operations[key] = op;
            }

            foreach (DictionaryEntry de in second)
            {
                string key = de.Key as string;
                if (!first.ContainsKey(key))
                    diff.Operations[key] = new ChangeOperation(de.Value);
            }

            return diff;
        }

        /// <summary>
        /// Finds the difference between two Json arrays based on element IDs.
        /// </summary>
        public static HashDiff IdDiff(ArrayList a, ArrayList b)
        {
            HashDiff diff = new HashDiff();

            for (int i = 0; i < a.Count; ++i)
            {
                object id = Id.GetId(a[i]);
                DiffOperation op = DiffValue(a[i], Id.FindObjectWithId(b, id));
                if (op != null)
                    diff.Operations[id] = op;
            }

            for (int i = 0; i < b.Count; ++i)
            {
                object id = Id.GetId(b[i]);
                object a_object = Id.FindObjectWithId(a, id);
                if (a_object == null)
                    diff.Operations[id] = new ChangeOperation(b[i]);
            }

            return diff;
        }

        /// <summary>
        /// Finds the difference between two Json arrays based on positions.
        /// </summary>
        public static PositionArrayDiff PositionDiff(ArrayList a, ArrayList b)
        {
            PositionArrayDiff diff = new PositionArrayDiff();
            
            int n = Math.Max(a.Count, b.Count);
            for (int i = 0; i < n; ++i)
            {
                object first_value = i >= a.Count ? null : a[i];
                object second_value = i >= b.Count ? null : b[i];
                DiffOperation op = DiffValue(first_value, second_value);
                if (op != null)
                    diff.Operations[i] = op;
            }

            return diff;
        }
       
        /// <summary>
        /// Computes the three-way-merge of Json hashtables.
        /// </summary>
        public static Hashtable Merge(Hashtable parent, Hashtable left, Hashtable right)
        {
            HashDiff left_diff = JsonDiff.Diff(parent, left);
            HashDiff right_diff = JsonDiff.Diff(parent, right);
            HashDiff diff = HashDiff.Merge(left_diff, right_diff);

            Hashtable res = parent.DeepClone();

            diff.Apply(res);
            return res;
        }

        /// <summary>
        /// Computes the three-way-merge of Json hashtables and returns the intermediate diff results.
        /// </summary>
        public static Hashtable MergeDetailed(Hashtable parent, Hashtable left, Hashtable right,
            out HashDiff left_diff, out HashDiff right_diff, out HashDiff merged_diff)
        {
            left_diff = JsonDiff.Diff(parent, left);
            right_diff = JsonDiff.Diff(parent, right);
            merged_diff = HashDiff.Merge(left_diff, right_diff);

            Hashtable res = parent.DeepClone();

            merged_diff.Apply(res);
            return res;
        }
    }
}
