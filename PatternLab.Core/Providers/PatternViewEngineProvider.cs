using System.Web.Mvc;

namespace PatternLab.Core.Providers
{
    public class PatternViewEngineProvider : RazorViewEngine
    {
        public PatternViewEngineProvider()
        {
            PartialViewLocationFormats = new[]
            {
                "~/Views/{1}/{0}.mustache",
                "~/Views/Shared/{0}.mustache"
            };
        }
    }
}