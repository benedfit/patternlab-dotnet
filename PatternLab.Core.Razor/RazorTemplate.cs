using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
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
            return pattern != null ? new RazorPatternEngine().Parse(pattern, data) : string.Empty;
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
    }
}