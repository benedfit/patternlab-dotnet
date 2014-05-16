using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nustache.Core;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Engines.Mustache
{
    /// <summary>
    /// The Pattern Lab specific view engine for handling Mustache
    /// </summary>
    public class MustacheViewEngine : VirtualPathProviderViewEngine
    {
        /// <summary>
        /// Initialises a new Pattern Lab view engine
        /// </summary>
        public MustacheViewEngine()
        {
            Encoders.HtmlEncode = HttpUtility.HtmlEncode;

            var masterLocationFormats = new List<string>();
            var areaMasterLocationFormats = new List<string>();
            var viewLocationFormats = new List<string>();
            var areaViewLocationFormats = new List<string>();
            var partialViewLocationFormats = new List<string>()
            {
                string.Concat("~/templates/pattern-header-footer/{0}", PatternProvider.FileExtensionHtml)
            };
            var areaPartialViewLocationFormats = new List<string>()
            {
                string.Concat("~/Areas/{2}/templates/pattern-header-footer/{0}", PatternProvider.FileExtensionHtml)
            };

            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            var extension = new MustachePatternEngine().Extension;

            // Set search locations for master pages, views and partial views
            masterLocationFormats.Add(string.Concat("~/Views/Shared/{0}", extension));
            areaMasterLocationFormats.Add(string.Concat("~/Areas/{2}/Views/Shared/{0}", extension));
            viewLocationFormats.Add(string.Concat("~/templates/{0}", extension));
            areaViewLocationFormats.Add(string.Concat("~/Areas/{2}/templates/{0}", extension));
            partialViewLocationFormats.Add(string.Concat("~/templates/partials/{0}", extension));
            areaPartialViewLocationFormats.Add(string.Concat("~/Areas/{2}/templates/partials/{0}", extension));

            MasterLocationFormats = masterLocationFormats.ToArray();
            AreaMasterLocationFormats = areaMasterLocationFormats.ToArray();
            ViewLocationFormats = viewLocationFormats.ToArray();
            AreaViewLocationFormats = areaViewLocationFormats.ToArray();
            PartialViewLocationFormats = partialViewLocationFormats.ToArray();
            AreaPartialViewLocationFormats = areaPartialViewLocationFormats.ToArray();
        }

        /// <summary>
        /// Creates a new partial view
        /// </summary>
        /// <param name="controllerContext">The current controller context</param>
        /// <param name="partialPath">The path to the partial</param>
        /// <returns>The partial view</returns>
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return GetView(controllerContext, partialPath, null);
        }

        /// <summary>
        /// Creates a new view
        /// </summary>
        /// <param name="controllerContext">The current controller context</param>
        /// <param name="viewPath">The path to the view</param>
        /// <param name="masterPath">The optional path to the master view</param>
        /// <returns>The view</returns>
        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return GetView(controllerContext, viewPath, masterPath);
        }

        /// <summary>
        /// Finds a partial view based on it's url, slash delimited path, or partial path
        /// </summary>
        /// <param name="controllerContext">The current controller context</param>
        /// <param name="partialViewName">The name of the partial view</param>
        /// <param name="useCache">Whether or not to use the cache</param>
        /// <returns>The partial view</returns>
        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName,
            bool useCache)
        {
            var pattern = FindPattern(partialViewName);

            // If a matching pattern is found return the pattern's template
            return pattern != null
                ? new ViewEngineResult(CreatePartialView(controllerContext, partialViewName), this)
                : base.FindPartialView(controllerContext, partialViewName, useCache);
        }

        /// <summary>
        /// Find a pattern based on it's url, slash delimited path, or partial path
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>The pattern</returns>
        private static Pattern FindPattern(string searchTerm)
        {
            // Remove pattern parameters
            searchTerm = searchTerm.StripPatternParameters();

            // Find a pattern based on it's url, slash delimited path, or partial path - http://patternlab.io/docs/pattern-including.html (see 'examples')
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

        /// <summary>
        /// Finds a view based on it's url, slash delimited path, or partial path
        /// </summary>
        /// <param name="controllerContext">The current controller context</param>
        /// <param name="viewName">The name of the view</param>
        /// <param name="masterName">The optional name of the master view</param>
        /// <param name="useCache">Whether or not to use the cache</param>
        /// <returns>The view</returns>
        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var pattern = FindPattern(viewName);

            // If a matching pattern is found return the pattern's template
            return pattern != null
                ? new ViewEngineResult(CreateView(controllerContext, viewName, string.Format(MasterLocationFormats[0], masterName)), this)
                : base.FindView(controllerContext, viewName, masterName, useCache);
        }

        /// <summary>
        /// Gets a Pattern Lab view
        /// </summary>
        /// <param name="controllerContext">The current controller context</param>
        /// <param name="name">The name of the view</param>
        /// <param name="masterPath">The optional path to the master view</param>
        /// <returns>The view</returns>
        private IView GetView(ControllerContext controllerContext, string name, string masterPath)
        {
            var pattern = FindPattern(name);

            // If a matching pattern is found return a view, and pass it any pattern parameters
            return new MustacheView(this, controllerContext, VirtualPathProvider,
                pattern != null ? pattern.ViewUrl : name, masterPath,
                name.ToPatternParameters());
        }
    }
}