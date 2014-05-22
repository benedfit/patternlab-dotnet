using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    public class RazorTemplateService : TemplateService
    {
        public RazorTemplateService()
            : base(new TemplateServiceConfiguration {BaseTemplateType = typeof (RazorTemplateBase<>)})
        {
        }
    }
}