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
        public string Include(string partial, string styleModifier, object parameters)
        {
            var data = Model;

            // Add styleModifier
            data.styleModifier = !string.IsNullOrEmpty(styleModifier) ? styleModifier : string.Empty;

            // TODO: Handle pattern parameters
            if (parameters != null)
            {
                data.patternParameters = parameters;
            }

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