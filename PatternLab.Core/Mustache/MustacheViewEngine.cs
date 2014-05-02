using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nustache.Core;
using PatternLab.Core.Helpers;
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
            var pattern = FindPattern(partialViewName);

            return pattern != null
                ? new ViewEngineResult(CreatePartialView(controllerContext, partialViewName), this)
                : base.FindPartialView(controllerContext, partialViewName, useCache);
        }

        private static Pattern FindPattern(string searchTerm)
        {
            searchTerm = searchTerm.StripPatternParameters();

            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            return provider.Patterns()
                .FirstOrDefault(
                    p =>
                        p.ViewUrl.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
                        p.PathSlash.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
                        p.Partial.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase)) ??
                   provider.Patterns()
                       .FirstOrDefault(
                           p =>
                               p.Partial.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase));
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var pattern = FindPattern(viewName);

            return pattern != null
                ? new ViewEngineResult(CreateView(controllerContext, viewName, string.Format(MasterLocationFormats[0], masterName)), this)
                : base.FindView(controllerContext, viewName, masterName, useCache);
        }

        private IView GetView(ControllerContext controllerContext, string name, string masterPath)
        {
            var pattern = FindPattern(name);
            return new MustacheView(this, controllerContext, VirtualPathProvider,
                pattern != null ? pattern.ViewUrl : name, masterPath,
                name.ToPatternParameters());
        }
    }
}