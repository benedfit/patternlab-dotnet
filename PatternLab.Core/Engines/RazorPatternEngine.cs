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
        /// <param name="template">The string template</param>
        /// <param name="data">The data collection</param>
        /// <returns>The parsed string</returns>
        public string Parse(string template, Dictionary<string, object> data)
        {
            //return Razor.Parse(template, data);
            return template;
        }
    }
}