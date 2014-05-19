using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace PatternLab.Core
{
    /// <summary>
    /// Converts an IDictionary to a DynamicObject
    /// </summary>
    public class DynamicDictionary : DynamicObject
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Converts an IDictionary to a DynamicObject
        /// </summary>
        /// <param name="dictionary">The dictionary</param>
        public DynamicDictionary(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
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
                result = new DynamicDictionary(result as IDictionary<string, object>);
            }
            else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
            {
                // Recursively convert all to DynamicObject
                result =
                    new List<DynamicDictionary>(
                        (result as ArrayList).ToArray()
                            .Select(x => new DynamicDictionary(x as IDictionary<string, object>)));
            }
            else if (result is ArrayList)
            {
                result = new List<object>((result as ArrayList).ToArray());
            }

            return _dictionary.ContainsKey(key);
        }
    }
}