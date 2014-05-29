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
using RazorTemplates.Core;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// The Pattern Lab razor template base
    /// </summary>
    public abstract class RazorTemplate : TemplateBase
    {
        /// <summary>
        /// Include a pettern within another
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <returns>The pattern to include</returns>
        public string Include(string partial)
        {
            return Include(partial, string.Empty, null);
        }

        /// <summary>
        /// Include a pettern within another, including pattern parameters
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <param name="parameters">The pattern parameters</param>
        /// <returns>The pattern to include</returns>
        public string Include(string partial, object parameters)
        {
            return Include(partial, string.Empty, parameters);
        }

        /// <summary>
        /// Include a pettern within another, including a styleModifier
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <param name="styleModifier">The styleModifier</param>
        /// <returns>The pattern to include</returns>
        public string Include(string partial, string styleModifier)
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
        public string Include(string partial, string styleModifier, params object[] parameters)
        {
            var data = Model;
            // Add styleModifier to data collection
            data.styleModifier = !string.IsNullOrEmpty(styleModifier) ? styleModifier : string.Empty;

            var serializer = new JavaScriptSerializer();
            var key = string.Format("{0}-{1}-{2}", partial, serializer.Serialize(data), serializer.Serialize(parameters));

            // Check cache for template
            if (HttpContext.Current.Cache[key] != null)
            {
                return (string)HttpContext.Current.Cache[key];
            }

            if (parameters != null)
            {
                // Loop through pattern parameters and override the data collection
                foreach (var parameter in parameters)
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

            // Find a pattern via the partial
            var pattern = PatternProvider.FindPattern(partial);
            
            if (pattern == null) return string.Empty;

            var template = new RazorPatternEngine().Parse(pattern, data);

            // Cache the found template
            HttpContext.Current.Cache.Insert(key, template, new CacheDependency(pattern.FilePath));

            return template;
        }

        /// <summary>
        /// Renders a link to a pattern - http://patternlab.io/docs/data-link-variable.html
        /// </summary>
        /// <param name="partial">The pattern's partial path</param>
        /// <returns>The link to the pattern</returns>
        public string Link(string partial)
        {
            var data = Model;

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
        /// <param name="number">The number of list items</param>
        /// <returns>The listItems data objects</returns>
        public List<dynamic> ListItems(string number)
        {
            // Determine how may listItem variables need to be generated based on the number name
            var index = PatternProvider.ListItemVariables.IndexOf(number);
            return ListItems(index + 1);
        }

        /// <summary>
        /// Creates a list using listItems - http://patternlab.io/docs/data-listitems.html
        /// </summary>
        /// <param name="count">The number of list items</param>
        /// <returns>The listItems data objects</returns>
        public List<dynamic> ListItems(int count)
        {
            var data = Model;
            var random = new Random();
            var randomNumbers = new List<int>();
            var listItems = new List<dynamic>();

            // Don't generate more than the listItems keyword allows for
            count = Math.Min(count, PatternProvider.ListItemVariables.Count);

            // For the desired number of listItems randomly select the object from listitems.json
            while (randomNumbers.Count < count)
            {
                // Check that the random number hasn't already been used
                var randomNumber = random.Next(1, PatternProvider.ListItemVariables.Count);
                if (randomNumbers.Contains(randomNumber)) continue;

                listItems.Add(data[randomNumber.ToString(CultureInfo.InvariantCulture)]);

                randomNumbers.Add(randomNumber);
            }

            return listItems;
        }

        /// <summary>
        /// Renders the razor remplate
        /// </summary>
        /// <param name="model">The data collection</param>
        /// <returns>The rendered contents of the template</returns>
        public override string Render(object model)
        {
            try
            {
                // Render the template
                return base.Render(model);
            }
            catch
            {
                // Handle errors during render and display an empty string
                return string.Empty;
            }
        }
    }
}