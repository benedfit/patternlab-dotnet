using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nustache.Core;

namespace PatternLab.Core
{
    public class MustacheViewEngine : VirtualPathProviderViewEngine
    {
        public MustacheViewEngine()
        {
            Encoders.HtmlEncode = HttpUtility.HtmlEncode;

            ViewLocationFormats = new[] {"~/Views/{1}/{0}.mustache", "~/Views/Shared/{0}.mustache"};
            PartialViewLocationFormats = new[] {"~/Views/{1}/{0}.mustache", "~/Views/Shared/{0}.mustache"};
            AreaViewLocationFormats = new[] { "~/Areas/{2}/Views/{1}/{0}.mustache", "~/Areas/{2}/Views/Shared/{0}.mustache" };
            AreaPartialViewLocationFormats = new[] { "~/Areas/{2}/Views/{1}/{0}.mustache", "~/Areas/{2}/Views/Shared/{0}.mustache" };

            RootContext = MustacheViewEngineRootContext.ViewData;
        }

        public MustacheViewEngineRootContext RootContext { get; set; }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return GetView(controllerContext, partialPath);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return GetView(controllerContext, viewPath);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentNullException("partialViewName");
            }

            var pattern =
                Controllers.PatternsController.Provider.Patterns()
                    .FirstOrDefault(
                        p =>
                            p.Url.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase) ||
                            p.Partial.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase));

            return pattern == null
                ? new ViewEngineResult(new[] {partialViewName})
                : new ViewEngineResult(CreatePartialView(controllerContext, pattern.Url), this);
        }

        private IView GetView(ControllerContext controllerContext, string path)
        {
            var physicalPath = controllerContext.HttpContext.Server.MapPath(path);
            return new MustacheView(this, controllerContext, physicalPath);
        }
    }

    public enum MustacheViewEngineRootContext
    {
        ViewData,
        Model
    }
}