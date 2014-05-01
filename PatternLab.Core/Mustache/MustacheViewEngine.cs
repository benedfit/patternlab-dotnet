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

            MasterLocationFormats = new[] {string.Concat("~/Views/Shared/{0}", PatternProvider.FileExtensionMustache)};

            ViewLocationFormats = new[] {string.Concat("~/templates/{0}", PatternProvider.FileExtensionMustache)};

            PartialViewLocationFormats = new[]
            {
                string.Concat("~/templates/partials/{0}", PatternProvider.FileExtensionMustache),
                string.Concat("~/templates/pattern-header-footer/{0}", PatternProvider.FileExtensionHtml)
            };

            AreaMasterLocationFormats = new[]
            {string.Concat("~/Areas/{2}/Views/Shared/{0}", PatternProvider.FileExtensionMustache)};

            AreaViewLocationFormats = new[]
            {string.Concat("~/Areas/{2}/templates/{0}", PatternProvider.FileExtensionMustache)};

            AreaPartialViewLocationFormats = new[]
            {
                string.Concat("~/Areas/{2}/templates/partials/{0}", PatternProvider.FileExtensionMustache),
                string.Concat("~/Areas/{2}/templates/pattern-header-footer/{0}", PatternProvider.FileExtensionHtml)
            };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return GetView(controllerContext, partialPath, null);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return GetView(controllerContext, viewPath, masterPath);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName,
            bool useCache)
        {
            var nameFragments = partialViewName.Split(new[] {PatternProvider.NameIdentifierParameters},
                StringSplitOptions.RemoveEmptyEntries);
            if (nameFragments.Length > 1)
            {
                partialViewName = nameFragments[0];
            }

            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            var pattern = provider.Patterns()
                .FirstOrDefault(
                    p =>
                        p.ViewUrl.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase) ||
                        p.PathSlash.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase) ||
                        p.Partial.Equals(partialViewName, StringComparison.InvariantCultureIgnoreCase)) ??
                          provider.Patterns()
                              .FirstOrDefault(
                                  p =>
                                      p.Partial.StartsWith(partialViewName, StringComparison.InvariantCultureIgnoreCase));

            return pattern != null
                ? new ViewEngineResult(CreatePartialView(controllerContext, pattern.ViewUrl), this)
                : base.FindPartialView(controllerContext, partialViewName, useCache);
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            var pattern = provider.Patterns()
                .FirstOrDefault(
                    p =>
                        p.ViewUrl.Equals(viewName, StringComparison.InvariantCultureIgnoreCase) ||
                        p.Partial.Equals(viewName, StringComparison.InvariantCultureIgnoreCase));

            return pattern != null
                ? new ViewEngineResult(CreateView(controllerContext, pattern.ViewUrl, string.Format(MasterLocationFormats[0], masterName)), this)
                : base.FindView(controllerContext, viewName, masterName, useCache);
        }

        private IView GetView(ControllerContext controllerContext, string path, string masterPath)
        {
            return new MustacheView(this, controllerContext, VirtualPathProvider, path, masterPath);
        }
    }
}