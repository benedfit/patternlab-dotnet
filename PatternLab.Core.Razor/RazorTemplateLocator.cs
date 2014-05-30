using PatternLab.Core.Providers;
using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// Pattern Lab razor template locator
    /// </summary>
    public class RazorTemplateLocator : ITemplateResolver
    {
        /// <summary>
        ///  Finds a template based on it's url, slash delimited path, or partial path
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>The template</returns>
        public string Resolve(string searchTerm)
        {
            var pattern = PatternProvider.FindPattern(searchTerm);
            return pattern != null ? pattern.Html : string.Empty;
        }
    }
}