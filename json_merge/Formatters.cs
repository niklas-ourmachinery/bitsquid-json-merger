using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace json_merge
{
    /// <summary>
    /// A generic interface to a formatter for displaying parts of a SJSON or JSON document.
    /// The interface is implemented by a concrete SjsonFormatter and JsonFormatter that uses
    /// the appropriate syntax for the object type.
    /// </summary>
    interface IFormatter
    {
        /// <summary>
        /// Formats an object field entry:   key = ...
        /// </summary>
        string ObjectField(Hashtable a, string key, int indent);

        /// <summary>
        /// Formats the start of an object stored under the specified key:   key = {
        /// </summary>
        string ObjectStart(string key, int indent);

        /// <summary>
        /// Formats the start of an object without any key:   {
        /// </summary>
        string ObjectStart(int indent);

        /// <summary>
        /// Formats the end of an object:   }
        /// </summary>
        string ObjectEnd(int indent);

        /// <summary>
        /// Formats an item in an array:   ...
        /// </summary>
        string ArrayItem(object o, int indent);

        /// <summary>
        /// Formats the start of an array stored under the specified key:   key = [
        /// </summary>
        string ArrayStart(string key, int indent);

        /// <summary>
        /// Formats the start of an array without any key:   [
        /// </summary>
        string ArrayStart(int indent);

        /// <summary>
        /// Formats the end of an array:   ]
        /// </summary>
        string ArrayEnd(int indent);
    }

    class SjsonFormatter : IFormatter
    {
        public string ObjectField(Hashtable a, string key, int indent)
        {
            if (!a.ContainsKey(key))
                return "";
            StringBuilder sb = new StringBuilder();
            SJSON.WriteObjectField(a, key, sb, indent);
            return sb.ToString();
        }

        public string ObjectStart(int indent)
        {
            StringBuilder sb = new StringBuilder();
            SJSON.WriteNewLine(sb, indent);
            sb.Append("{");
            return sb.ToString();
        }

        public string ObjectStart(string key, int indent)
        {
            StringBuilder sb = new StringBuilder();
            SJSON.WriteNewLine(sb, indent);
            sb.Append(key);
            sb.Append(" = {");
            return sb.ToString();
        }

        public string ObjectEnd(int indent)
        {
            StringBuilder sb = new StringBuilder();
            SJSON.WriteNewLine(sb, indent);
            sb.Append("}");
            return sb.ToString();
        }

        public string ArrayItem(object o, int indent)
        {
            if (o == null)
                return "";
            StringBuilder sb = new StringBuilder();
            SJSON.WriteNewLine(sb, indent);
            SJSON.Write(o, sb, indent);
            return sb.ToString();
        }

        public string ArrayStart(string key, int indent)
        {
            StringBuilder sb = new StringBuilder();
            SJSON.WriteNewLine(sb, indent);
            sb.Append(key);
            sb.Append(" = [");
            return sb.ToString();
        }

        public string ArrayStart(int indent)
        {
            StringBuilder sb = new StringBuilder();
            SJSON.WriteNewLine(sb, indent);
            sb.Append("[");
            return sb.ToString();
        }

        public string ArrayEnd(int indent)
        {
            StringBuilder sb = new StringBuilder();
            SJSON.WriteNewLine(sb, indent);
            sb.Append("]");
            return sb.ToString();
        }
    }

    class JsonFormatter : IFormatter
    {
        // Stack to keep track of whether we need to insert a comma or not.
        // When we open a new object or array, we push false to the stack.
        // After an item has been written, we change the top value to true to
        // indicate that the next item needs a comma.
        // When we close an object or an array we pop the top item.
        List<bool> _comma = new List<bool>();

        // Should a comma be printed for the current array/hash item?
        // This property will return false for the first item in each array/hash scope,
        // then it will return true.
        private bool Comma
        {
            get { bool v = _comma[_comma.Count - 1]; _comma[_comma.Count - 1] = true; return v; }
        }

        public JsonFormatter()
        {
            _comma.Add(false);
        }

        public string ObjectField(Hashtable a, string key, int indent)
        {
            if (!a.ContainsKey(key))
                return "";
            StringBuilder sb = new StringBuilder();
            JSON.WriteObjectField(a, key, Comma, sb, indent);
            return sb.ToString();
        }

        public string ObjectStart(int indent)
        {
            StringBuilder sb = new StringBuilder();
            if (Comma) sb.Append(",");
            JSON.WriteNewLine(sb, indent);
            sb.Append("{");
            _comma.Add(false);
            return sb.ToString();
        }

        public string ObjectStart(string key, int indent)
        {
            StringBuilder sb = new StringBuilder();
            if (Comma) sb.Append(",");
            JSON.WriteNewLine(sb, indent);
            sb.Append(key);
            sb.Append(" : {");
            _comma.Add(false);
            return sb.ToString();
        }

        public string ObjectEnd(int indent)
        {
            StringBuilder sb = new StringBuilder();
            JSON.WriteNewLine(sb, indent);
            sb.Append("}");
            _comma.RemoveAt(_comma.Count - 1);
            return sb.ToString();
        }

        public string ArrayItem(object o, int indent)
        {
            if (o == null)
                return "";
            StringBuilder sb = new StringBuilder();
            if (Comma) sb.Append(",");
            JSON.WriteNewLine(sb, indent);
            JSON.Write(o, sb, indent);
            return sb.ToString();
        }

        public string ArrayStart(string key, int indent)
        {
            StringBuilder sb = new StringBuilder();
            if (Comma) sb.Append(",");
            JSON.WriteNewLine(sb, indent);
            sb.Append(key);
            sb.Append(" = [");
            _comma.Add(false);
            return sb.ToString();
        }

        public string ArrayStart(int indent)
        {
            StringBuilder sb = new StringBuilder();
            if (Comma) sb.Append(",");
            JSON.WriteNewLine(sb, indent);
            sb.Append("[");
            _comma.Add(false);
            return sb.ToString();
        }

        public string ArrayEnd(int indent)
        {
            StringBuilder sb = new StringBuilder();
            JSON.WriteNewLine(sb, indent);
            sb.Append("]");
            _comma.RemoveAt(_comma.Count - 1);
            return sb.ToString();
        }
    }
}
