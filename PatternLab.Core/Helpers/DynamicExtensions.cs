using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
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
        /// <param name="dictionary">The dictionary</param>
        /// <returns>The dynamic object</returns>
        public static dynamic ToDynamic(this IDictionary<string, object> dictionary)
        {
            var result = new ExpandoObject();
            IDictionary<string, object> objects = result;

            foreach (var keyValuePair in dictionary)
            {
                var processed = false;

                var value = keyValuePair.Value as IDictionary<string, object>;
                if (value != null)
                {
                    objects.Add(keyValuePair.Key, ToDynamic(value));
                    processed = true;
                }
                else
                {
                    var collection = keyValuePair.Value as ICollection;
                    if (collection != null)
                    {
                        var itemList = (from object item in collection
                            let item1 = item as IDictionary<string, object>
                            select
                                item1 != null
                                    ? ToDynamic(item1)
                                    : ToDynamic(new Dictionary<string, object> {{"Unknown", item}})).Cast<object>()
                            .ToList();

                        if (itemList.Count > 0)
                        {
                            objects.Add(keyValuePair.Key, itemList);
                            processed = true;
                        }
                    }
                }

                if (!processed)
                {
                    objects.Add(keyValuePair);
                }
            }

            return result;
        }
    }
}