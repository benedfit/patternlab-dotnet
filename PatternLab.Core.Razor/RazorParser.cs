using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
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

        public static string Parse(string razorTemplate, object model, string cacheName)
        {
            return TemplateService.Parse(razorTemplate, model, null, cacheName);
        }
    }
}