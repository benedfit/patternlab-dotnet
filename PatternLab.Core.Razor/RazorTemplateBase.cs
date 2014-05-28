using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// The Pattern Lab razor template base
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    public class RazorTemplateBase<T> : TemplateBase<T>
    {
        /// <summary>
        /// Include a pattern within another
        /// </summary>
        /// <param name="cacheName">The pattern partial (used for caching)</param>
        /// <param name="model">The optional data model</param>
        /// <returns>The pattern to include</returns>
        public override TemplateWriter Include(string cacheName, object model = null)
        {
            if (model is string)
            {
                // styleModifier supplied
                return Include(cacheName, model as string, null);
            }

            return Include(cacheName, string.Empty, model);
        }

        /// <summary>
        /// Include a pettern within another, including a styleModifier or pattern parameters
        /// </summary>
        /// <param name="cacheName">The pattern partial (used for caching)</param>
        /// <param name="styleModifier">The styleModifier</param>
        /// <param name="parameters">The pattern parameters</param>
        /// <returns>The pattern to include</returns>
        public TemplateWriter Include(string cacheName, string styleModifier, object parameters)
        {
            dynamic model = Model;
            
            // Add styleModifier
            model.styleModifier = !string.IsNullOrEmpty(styleModifier) ? styleModifier : string.Empty;

            // TODO: Handle pattern parameters
            return base.Include(cacheName, (object)model);
        }

        /// <summary>
        /// Renders a link to a pattern - http://patternlab.io/docs/data-link-variable.html
        /// </summary>
        /// <param name="partial">The pattern partial</param>
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
        /// Determines if a section should be rendered
        /// </summary>
        /// <param name="model">The data model</param>
        /// <returns>Whether or not the section should be rendered</returns>
        public bool Section(object model)
        {
            if (model is bool)
            {
                // If a boolean, return its value
                return (bool) model;
            }

            // If not a boolean, check if it's null
            return model != null;
        }
    }
}