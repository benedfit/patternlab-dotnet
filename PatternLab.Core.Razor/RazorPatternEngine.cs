using System.Web.Script.Serialization;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// The Razor (.cshtml) pattern engine
    /// </summary>
    public class RazorPatternEngine : IPatternEngine
    {
        private ITemplateServiceConfiguration _config;
        private ITemplateService _service;

        private ITemplateServiceConfiguration Config
        {
            get
            {
                // Create new configuration object is one doesn't exist
                return _config ??
                       (_config =
                           new TemplateServiceConfiguration
                           {
                               BaseTemplateType = typeof (RazorTemplate<>),
                               Resolver = new RazorTemplateLocator()
                           });
            }
        }

        private ITemplateService Service
        {
            get
            {
                // Create new service object if one doesn't exist
                return _service ?? (_service = new TemplateService(Config));
            }
        }

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
            return @"@Include\(""(.*?)""";
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
        public string Parse(Pattern pattern, object data)
        {
            var serializer = new JavaScriptSerializer();

            // Create a key to cache the result against
            var key = string.Format("{0}-{1}", pattern.Partial, serializer.Serialize(data));

            return Service.Parse(pattern.Html, data, null, key);
        }
    }
}