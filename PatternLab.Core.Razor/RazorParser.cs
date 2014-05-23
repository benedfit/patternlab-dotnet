using System;
using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// The Pattern Lab razor parser
    /// </summary>
    public static class RazorParser
    {
        private static readonly ITemplateService Service = new RazorTemplateService();
        private static readonly object Sync = new object();

        private static ITemplateService TemplateService
        {
            get
            {
                lock (Sync)
                    return Service;
            }
        }

        /// <summary>
        /// Parses a template against a data model
        /// </summary>
        /// <param name="razorTemplate">The template</param>
        /// <param name="model">The data model</param>
        /// <param name="cacheName">The pattern partial (used for caching)</param>
        /// <returns></returns>
        public static string Parse(string razorTemplate, object model, string cacheName)
        {
            try
            {
                return TemplateService.Parse(razorTemplate, model, null, cacheName);
            }
            catch (Exception e)
            {
                // TODO: Remove this once all the issues are handled
                return e.Message;
            }
        }
    }
}