using System.Web.Mvc;

namespace PatternLab.Core.Views
{
    public class MustaceViewEngine : VirtualPathProviderViewEngine
    {
        public MustaceViewEngine()
        {
            ViewLocationFormats = new[] {"~/Views/{1}/{0}.mustache", "~/Views/Shared/{0}.mustache"};

            PartialViewLocationFormats = new[] {"~/Views/{1}/{0}.mustache", "~/Views/Shared/{0}.mustache"};
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            var physicalPath = controllerContext.HttpContext.Server.MapPath(partialPath);
            return new MustacheView(physicalPath);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            var physicalPath = controllerContext.HttpContext.Server.MapPath(viewPath);
            return new MustacheView(physicalPath);
        }
    }
}