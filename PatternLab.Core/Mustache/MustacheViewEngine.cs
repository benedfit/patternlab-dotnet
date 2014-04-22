using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nustache.Core;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Mustache
{
    public class MustacheViewEngine : VirtualPathProviderViewEngine
    {
        public MustacheViewEngine()
        {
            Encoders.HtmlEncode = HttpUtility.HtmlEncode;

            MasterLocationFormats = new[] {"~/Views/Shared/{0}.mustache"};
            ViewLocationFormats = new[] {"~/templates/{0}.mustache"};
            PartialViewLocationFormats = new[] {"~/templates/partials/{0}.mustache", "~/templates/pattern-header-footer/{0}.html"};
            AreaMasterLocationFormats = new[] { "~/Areas/{2}/Views/Shared/{0}.mustache" };
            AreaViewLocationFormats = new[] { "~/Areas/{2}/templates/{0}.mustache" };
            AreaPartialViewLocationFormats = new[] { "~/Areas/{2}/templates/partials/{0}.mustache", "~/Areas/{2}/templates/pattern-header-footer/{0}.html" };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return GetView(controllerContext, partialPath, null);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return GetView(controllerContext, viewPath, masterPath);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            var nameFragments = partialViewName.Split(new[] { PatternProvider.IdentifierParameter }, StringSplitOptions.RemoveEmptyEntries);
            if (nameFragments.Length > 1)
            {
                partialViewName = nameFragments[0];
            }

            var pattern =
                Controllers.PatternLabController.Provider.Patterns()
                    .FirstOrDefault(
                        p =>
                            p.Url.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase) ||
                            p.PathSlash.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase) ||
                            p.Partial.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase)) ??
                Controllers.PatternLabController.Provider.Patterns()
                    .FirstOrDefault(
                        p => p.Partial.StartsWith(partialViewName, StringComparison.InvariantCultureIgnoreCase));

            return pattern != null
                ? new ViewEngineResult(CreatePartialView(controllerContext, pattern.Url), this)
                : base.FindPartialView(controllerContext, partialViewName, useCache);
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var pattern =
                Controllers.PatternLabController.Provider.Patterns()
                    .FirstOrDefault(
                        p =>
                            p.Url.Equals(viewName, StringComparison.InvariantCultureIgnoreCase) ||
                            p.Partial.Equals(viewName, StringComparison.InvariantCultureIgnoreCase));

            return pattern != null
                ? new ViewEngineResult(CreateView(controllerContext, pattern.Url, string.Format(MasterLocationFormats[0], masterName)), this)
                : base.FindView(controllerContext, viewName, masterName, useCache);
        }

        private IView GetView(ControllerContext controllerContext, string path, string masterPath)
        {
            return new MustacheView(this, controllerContext, VirtualPathProvider, path, masterPath);
        }
    }
}