using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace PatternLab.Core
{
    /// <summary>
    /// A dynamic data dictionary for use in Pattern Lab
    /// </summary>
    public class DynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        readonly IDictionary<string, object> _dictionary = new Dictionary<string, object>();

        /// <summary>
        /// The boolean operator
        /// </summary>
        /// <param name="d">The dynamic dictionary</param>
        /// <returns>True/false</returns>
        public static implicit operator bool(DynamicDictionary d)
        {
            // Check if the property exists
            return d != null && d._dictionary != null;
        }

        /// <summary>
        /// Adds an item to the dynamic dictionary
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Add(KeyValuePair<string, object> item)
        {
            _dictionary.Add(item);
        }

        /// <summary>
        /// Clears the the dynamic dictionary
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// Check to see if the dynamic dictionary contains an item
        /// </summary>
        /// <param name="item">The item to check if it contains</param>
        /// <returns>True/false</returns>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.Contains(item);
        }

        /// <summary>
        /// Copies the dynamic dictionary to an array
        /// </summary>
        /// <param name="array">The target array</param>
        /// <param name="arrayIndex">The array index</param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes an item from the dynamic dictionary
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>True/false</returns>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dictionary.Remove(item);
        }

        /// <summary>
        /// The number of items in the dynamic dictionary
        /// </summary>
        public int Count
        {
            get { return _dictionary.Count; }
        }

        /// <summary>
        /// Returns whether or not the dynamic dictionary is readonly
        /// </summary>
        public bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        /// <summary>
        /// Tries to get a member from the dynamic dictionary
        /// </summary>
        /// <param name="binder">The member binder</param>
        /// <param name="result">The resulting value</param>
        /// <returns>True/false</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!TryGetValue(binder.Name, out result))
            {
                // Return empty string for missing values
                result = string.Empty;
            }

            return true;
        }

        /// <summary>
        /// Tries to set a member in the dynamic dictionary
        /// </summary>
        /// <param name="binder">The member binder</param>
        /// <param name="value">The value to set</param>
        /// <returns>True/false</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Gets the dynamic member names of the dynamic dictionary
        /// </summary>
        /// <returns>A list of member names</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dictionary.Keys;
        }

        /// <summary>
        /// Gets the enumerator for the dynamic dictionary
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator for the inherited interface
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determines whether the dynamic dictionary contains a specific key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True/false</returns>
        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Adds an item to the dynamic dictionary
        /// </summary>
        /// <param name="key">The item key</param>
        /// <param name="value">The item value</param>
        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        /// <summary>
        /// Removes an item from the dynamic dictionary
        /// </summary>
        /// <param name="key">The item key</param>
        /// <returns>True/false</returns>
        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        /// <summary>
        /// Tries to get a value from the dynamic dictionary
        /// </summary>
        /// <param name="key">The item key</param>
        /// <param name="value">The resulting item</param>
        /// <returns>True/false</returns>
        public bool TryGetValue(string key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Selects a single item from the dynamic dictionary by its key
        /// </summary>
        /// <param name="key">The item key</param>
        /// <returns>The value</returns>
        public object this[string key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        /// <summary>
        /// The list of keys in the dynamic dictionary
        /// </summary>
        public ICollection<string> Keys
        {
            get { return _dictionary.Keys; }
        }

        /// <summary>
        /// The list of values in the dynamic dictionary
        /// </summary>
        public ICollection<object> Values
        {
            get { return _dictionary.Values; }
        }
    }
}