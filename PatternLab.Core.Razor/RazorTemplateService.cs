using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// The Pattern Lab razor template service
    /// </summary>
    public class RazorTemplateService : TemplateService
    {
        /// <summary>
        /// Initialises a new Pattern Lab razor template service
        /// </summary>
        public RazorTemplateService()
            : base(new TemplateServiceConfiguration {BaseTemplateType = typeof (RazorTemplateBase<>)})
        {
        }
    }
}