using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PatternLab.Core.Helpers
{
    /// <summary>
    /// Pattern Lab extension methods for IDictionary
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// Converts an IDictionary into a dynamic object
        /// </summary>
        /// <param name="source">The dictionary</param>
        /// <returns>The dynamic object</returns>
        public static dynamic ToDynamic(this IDictionary<string, object> source)
        {
            IDictionary<string, object> result = new DynamicDictionary();

            foreach (var keyValuePair in source)
            {
                var processed = false;

                var value = keyValuePair.Value as IDictionary<string, object>;
                if (value != null)
                {
                    result.Add(keyValuePair.Key, ToDynamic(value));
                    processed = true;
                }
                else
                {
                    var collection = keyValuePair.Value as ICollection;
                    if (collection != null)
                    {
                        var values = (from object item in collection
                            let dictionary = item as IDictionary<string, object>
                            select
                                dictionary != null
                                    ? ToDynamic(dictionary)
                                    : ToDynamic(new Dictionary<string, object> {{string.Empty, item}})).ToList();

                        if (values.Count > 0)
                        {
                            result.Add(keyValuePair.Key, values);
                            processed = true;
                        }
                    }
                }

                if (!processed)
                {
                    result.Add(keyValuePair);
                }
            }

            return result;
        }
    }
}