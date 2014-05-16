using System.Collections.Generic;
using Nustache.Core;
using PatternLab.Core.Engines.Mustache;

namespace PatternLab.Core.Engines
{
    /// <summary>
    /// The Mustache (.mustache) pattern engine
    /// </summary>
    public class MustachePatternEngine : IPatternEngine
    {
        /// <summary>
        /// The file extension of pattern templates read by pattern engine
        /// </summary>
        /// <returns>.cshtml</returns>
        public string Extension()
        {
            return ".mustache";
        }

        /// <summary>
        /// The name of the pattern engine
        /// </summary>
        /// <returns>Razor</returns>
        public string Name()
        {
            return "Mustache";
        }

        /// <summary>
        /// Parses a string against a data collection using Mustache
        /// </summary>
        /// <param name="template">The string template</param>
        /// <param name="data">The data collection</param>
        /// <returns>The parsed string</returns>
        public string Parse(string template, Dictionary<string, object> data)
        {
            return Render.StringToString(template, data, new MustacheTemplateLocator().GetTemplate);
        }
    }
}