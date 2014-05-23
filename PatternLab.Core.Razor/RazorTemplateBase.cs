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
            // TODO: Handle styleModifier and pattern parameters
            return base.Include(cacheName, Model);
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
    }
}