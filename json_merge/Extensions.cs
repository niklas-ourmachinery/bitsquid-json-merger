using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace json_merge
{
    /// <summary>
    /// Extensions to the Dictionary class.
    /// </summary>
    public static class DictionaryDefaultExtension
    {
        /// <summary>
        /// Returns the value for the key if it exists, otherwise the specified default value.
        /// </summary>
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dic, K key, V def)
        {
            V ret;
            bool found = dic.TryGetValue(key, out ret);
            if (found) { return ret; }
            return def;
        }
    }

    /// <summary>
    /// Extensions to clone objects. 
    /// </summary>
    public static class ObjectCloneExtension
    {
        /// <summary>
        /// Clones the object through the binary serializer.
        /// </summary>
        public static T DeepClone<T>(this T a)
        {
            using (Stream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
