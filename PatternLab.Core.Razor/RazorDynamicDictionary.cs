using System.Collections.Generic;
using System.Dynamic;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// A dictionary for handling dynamic values
    /// </summary>
    public class RazorDynamicDictionary : DynamicObject
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Initialises a blank dynamic dictionary
        /// </summary>
        public RazorDynamicDictionary() : this(new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initialises a dynamic dictionary from an existing object
        /// </summary>
        /// <param name="source">The existing object</param>
        public RazorDynamicDictionary(object source)
        {
            _dictionary = source as IDictionary<string, object>;
        }

        /// <summary>
        /// Gets a value from the dynamic dictionary
        /// </summary>
        /// <param name="binder">The member binder</param>
        /// <param name="result">The result</param>
        /// <returns>true</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name.ToLower();

            if (!_dictionary.TryGetValue(name, out result))
            {
                // If the value is not in the dictionary return an empty string
                result = string.Empty;
            }

            return true;
        }

        /// <summary>
        /// Sets a value in the dynamic dictionary
        /// </summary>
        /// <param name="binder">The member binder</param>
        /// <param name="value">The value</param>
        /// <returns>true</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary[binder.Name.ToLower()] = value;
            return true;
        }
    }
}