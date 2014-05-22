using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// Converts an IDictionary to a DynamicObject
    /// </summary>
    public class RazorDynamicDictionary : DynamicObject
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Creates a new dynamic dictionary
        /// </summary>
        public RazorDynamicDictionary()
        {
            _dictionary = new Dictionary<string, object>();
        }

        /// <summary>
        /// Converts an IDictionary to a dynamic dictionary
        /// </summary>
        /// <param name="dictionary">The dictionary</param>
        public RazorDynamicDictionary(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public void Add(string key, object value)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = value;
            }
            else
            {
                _dictionary.Add(key, value);
            }
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

        /// <summary>
        /// Convert dictionary members to DynamicObject
        /// </summary>
        /// <param name="binder">The member bind</param>
        /// <param name="result">The converted object</param>
        /// <returns>The converted object</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var key = binder.Name;

            try
            {
                result = _dictionary[key];
            }
            catch
            {
                // Return missing values as empty string
                result = string.Empty;
                return true;
            }

            if (result is IDictionary<string, object>)
            {
                // Convert to DynamicObject
                result = new RazorDynamicDictionary(result as IDictionary<string, object>);
            }
            else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
            {
                // Recursively convert all to DynamicObject
                result =
                    new List<RazorDynamicDictionary>(
                        (result as ArrayList).ToArray()
                            .Select(x => new RazorDynamicDictionary(x as IDictionary<string, object>)));
            }
            else if (result is ArrayList)
            {
                result = new List<object>((result as ArrayList).ToArray());
            }

            return _dictionary.ContainsKey(key);
        }

        public static implicit operator bool(RazorDynamicDictionary r)
        {
            return r != null && r._dictionary != null;
        }
    }
}