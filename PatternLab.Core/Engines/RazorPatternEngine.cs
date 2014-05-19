using System.Collections.Generic;
using RazorEngine;

namespace PatternLab.Core.Engines
{
    /// <summary>
    /// The Razor (.cshtml) pattern engine
    /// </summary>
    public class RazorPatternEngine : IPatternEngine
    {
        /// <summary>
        /// The file extension of pattern templates read by pattern engine
        /// </summary>
        /// <returns>.cshtml</returns>
        public string Extension()
        {
            return ".cshtml";
        }

        /// <summary>
        /// The Regex pattern for finding lineages in templates read by pattern engine
        /// </summary>
        /// <returns>@Include\(""(.*?)""\)</returns>
        public string LineagePattern()
        {
            return @"@Include\(""(.*?)""\)";
        }

        /// <summary>
        /// The name of the pattern engine
        /// </summary>
        /// <returns>Razor</returns>
        public string Name()
        {
            return "Razor";
        }

        /// <summary>
        /// Parses a string against a data collection using Razor
        /// </summary>
        /// <param name="pattern">The pattern</param>
        /// <param name="data">The data collection</param>
        /// <returns>The parsed string</returns>
        public string Parse(Pattern pattern, Dictionary<string, object> data)
        {
            // Convert data collection to dynamic
            dynamic model = new DynamicDictionary(data);

            return Razor.Parse(pattern.Html, model, pattern.Partial);
        }
    }
}