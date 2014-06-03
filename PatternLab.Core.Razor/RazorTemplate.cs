using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Script.Serialization;
using PatternLab.Core.Providers;
using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// Pattern Lab razor template
    /// </summary>
    public abstract class RazorTemplate<T> : TemplateBase<T>
    {
        /// <summary>
        /// Include a pettern within another, including pattern parameters
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <param name="parameters">The pattern parameters</param>
        /// <returns>The pattern to include</returns>
        public override TemplateWriter Include(string partial, object parameters = null)
        {
            return Include(partial, string.Empty, parameters);
        }

        /// <summary>
        /// Include a pettern within another, including a styleModifier
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <param name="styleModifier">The styleModifier</param>
        /// <returns>The pattern to include</returns>
        public TemplateWriter Include(string partial, string styleModifier)
        {
            return Include(partial, styleModifier, null);
        }

        /// <summary>
        /// Include a pettern within another, including a styleModifier and pattern parameters
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <param name="styleModifier">The styleModifier</param>
        /// <param name="parameters">The pattern parameters</param>
        /// <returns>The pattern to include</returns>
        public TemplateWriter Include(string partial, string styleModifier, params object[] parameters)
        {
            dynamic data = Model;
            // Add styleModifier to data collection
            data.styleModifier = !string.IsNullOrEmpty(styleModifier) ? styleModifier : string.Empty;

            var serializer = new JavaScriptSerializer();
            var key = string.Format("{0}({1})", partial, serializer.Serialize(parameters));

            // Check cache for template
            /*if (HttpContext.Current.Cache[key] != null)
            {
                return (TemplateWriter)HttpContext.Current.Cache[key];
            }*/

            if (parameters.Any())
            {
                // Loop through pattern parameters and override the data collection
                foreach (var parameter in parameters.Where(p => p != null))
                {
                    var dictionary = parameter as IDictionary<string, object>;
                    if (dictionary != null)
                    {
                        foreach (var keyValuePair in dictionary)
                        {
                            data[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }
                    else
                    {
                        var list = parameter as IList;
                        if (list != null)
                        {
                            // If the object is an array, loop through it's children
                            foreach (
                                var keyValuePair in
                                    list.OfType<DynamicDictionary>().SelectMany(d => d))
                            {
                                data[keyValuePair.Key] = keyValuePair.Value;
                            }
                        }
                        else
                        {
                            // If it's an anonymous type, loop through it's properties
                            foreach (
                                var property in
                                    parameter.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                            {
                                data[property.Name] = property.GetValue(parameter, null);
                            }
                        }
                    }
                }
            }

            var template = base.Include(partial, (object)data);
            var pattern = PatternProvider.FindPattern(partial);
            var callback = new CacheItemRemovedCallback(Removed);

            // Cache the found template
            HttpContext.Current.Cache.Insert(key, template, new CacheDependency(pattern.CacheDependencies.ToArray()),
                Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, callback);

            return template;
        }

        /// <summary>
        /// Renders a link to a pattern - http://patternlab.io/docs/data-link-variable.html
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <returns>The link to the pattern</returns>
        public string Link(string partial)
        {
            dynamic data = Model;

            try
            {
                // Find the link from the current model
                return data.link[partial];
            }
            catch
            {
                // Handle partials that don't match a pattern
                return string.Empty;
            }
        }

        /// <summary>
        /// Creates a list using listItems - http://patternlab.io/docs/data-listitems.html
        /// </summary>
        /// <param name="key">The key (one through twelve) representing the number of listItems to render</param>
        /// <param name="template">The razor template to render</param>
        /// <returns>The listItems objects</returns>
        public TemplateWriter ListItems(string key, Func<T, TemplateWriter> template)
        {
            // Determine how may listItem variables need to be generated based on the number name key
            var index = PatternProvider.ListItemVariables.IndexOf(key);

            // If the key does not match a listItem variable, nothing will be rendered
            return ListItems(index + 1, template);
        }

        /// <summary>
        /// Creates a list using listItems - http://patternlab.io/docs/data-listitems.html
        /// </summary>
        /// <param name="count">The number of listItems to render</param>
        /// <param name="template">The razor template to render</param>
        /// <returns>The listItems objects</returns>
        public TemplateWriter ListItems(int count, Func<T, TemplateWriter> template)
        {
            dynamic data = Model;
            var random = new Random();
            var randomNumbers = new List<int>();
            var listItems = new List<dynamic>();

            // Don't generate more than the listItems keyword allows for
            count = Math.Min(count, PatternProvider.ListItemVariables.Count);

            // For the desired number of listItems randomly select the object from listitems.json
            while (randomNumbers.Count < count)
            {
                // Check that the random number hasn't already been used
                var randomNumber = random.Next(1, PatternProvider.ListItemVariables.Count + 1);
                if (randomNumbers.Contains(randomNumber)) continue;

                listItems.Add(data[randomNumber.ToString(CultureInfo.InvariantCulture)]);

                randomNumbers.Add(randomNumber);
            }

            return new TemplateWriter(writer =>
            {
                foreach (var listItem in listItems)
                {
                    Model = listItem;

                    template(listItem).WriteTo(writer);
                }

                Model = data;
            });
        }

        /// <summary>
        /// Removes cached template when file is edited
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <param name="value">The object value</param>
        /// <param name="reason">The reason to remove from cache</param>
        private void Removed(string key, object value, CacheItemRemovedReason reason)
        {
            // Clear the template from cache
            TemplateService.RemoveTemplate(key.Remove(key.IndexOf(PatternProvider.IdentifierParameters)));
        }
    }
}