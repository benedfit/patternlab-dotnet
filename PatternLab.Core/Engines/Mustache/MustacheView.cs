using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PatternLab.Core.Engines.Mustache
{
    /// <summary>
    /// The Pattern Lab specific view for handling Mustache
    /// </summary>
    public class MustacheView : IView
    {
        private readonly MustacheViewEngine _engine;
        private readonly ControllerContext _controllerContext;
        private readonly string _masterPath;
        private readonly Dictionary<string, object> _parameters;
        private readonly string _viewPath;
        private readonly VirtualPathProvider _virtualPathProvider;

        /// <summary>
        /// Initialise a new Pattern Lab view
        /// </summary>
        /// <param name="engine">The current view engine</param>
        /// <param name="controllerContext">The current controller context</param>
        /// <param name="virtualPathProvider">The current virtual path provider</param>
        /// <param name="viewPath">The path to the view</param>
        /// <param name="masterPath">The optional path to the master view</param>
        /// <param name="parameters">Any pattern parameters</param>
        public MustacheView(MustacheViewEngine engine, ControllerContext controllerContext,
            VirtualPathProvider virtualPathProvider, string viewPath, string masterPath, Dictionary<string, object> parameters)
        {
            _engine = engine;
            _controllerContext = controllerContext;
            _masterPath = masterPath;
            _parameters = parameters;
            _viewPath = viewPath;
            _virtualPathProvider = virtualPathProvider;
        }

        /// <summary>
        /// Replaces the Mustache variables in a template with @Model and referenced patterns
        /// </summary>
        /// <param name="viewContext">The current view context</param>
        /// <param name="writer">The text writer</param>
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            // Pass data to write
            Render(viewContext.ViewData.Model, writer);
        }

        /// <summary>
        /// Replaces the Mustache variables in a template with data and referenced patterns
        /// </summary>
        /// <param name="data">The data to replace Mustache variables with</param>
        /// <param name="writer">The text write</param>
        public void Render(object data, TextWriter writer)
        {
            // Get the current template
            var viewTemplate = GetTemplate();

            if (!string.IsNullOrEmpty(_masterPath))
            {
                // If using a master page parse that template first
                var masterTemplate = LoadTemplate(_masterPath);
                masterTemplate.Render(
                    data,
                    writer,
                    name =>
                    {
                        if (name == "Body")
                        {
                            // Replace {{>Body}} with the context of the view
                            return GetTemplate();
                        }

                        var template = viewTemplate.GetTemplateDefinition(name);

                        if (template != null)
                        {
                            return template;
                        }

                        // If another pattern in referenced, find it
                        return FindPartial(name);
                    });
            }
            else
            {
                // Render the template and reference patterns
                GetTemplate().Render(data, writer, FindPartial);
            }
        }

        /// <summary>
        /// Get the Mustache template from it's path and pattern parameters
        /// </summary>
        /// <returns>The Mustache template</returns>
        private MustacheTemplate GetTemplate()
        {
            return LoadTemplate(_viewPath, _parameters);
        }

        private MustacheTemplate LoadTemplate(string virtualPath, Dictionary<string, object> parameters = null)
        {
            var physicalPath = HostingEnvironment.MapPath(virtualPath) ?? string.Empty;
            var serializer = new JavaScriptSerializer();
            var key = string.Format("{0}-{1}", virtualPath, serializer.Serialize(parameters));

            // Check cache for template
            if (_controllerContext.HttpContext.Cache[key] != null)
            {
                return (MustacheTemplate)_controllerContext.HttpContext.Cache[key];
            }

            // Load from disk or assembly embedded resources if not cached
            var embeddedResource = _virtualPathProvider.GetFile(virtualPath) as EmbeddedResource;
            var templateSource = embeddedResource != null ? embeddedResource.ReadAllText() : File.ReadAllText(physicalPath);

            // Pass contents of file into template along with any pattern parameters
            var template = new MustacheTemplate(_parameters);
            template.Load(new StringReader(templateSource));

            // Cache the found template
            _controllerContext.HttpContext.Cache.Insert(key, template,
                embeddedResource != null
                    ? embeddedResource.CacheDependency(DateTime.UtcNow)
                    : new CacheDependency(physicalPath));

            return template;
        }

        /// <summary>
        /// Search the view engine for a partial view
        /// </summary>
        /// <param name="name">The name of the partial view</param>
        /// <returns>The matching partial view</returns>
        private MustacheTemplate FindPartial(string name)
        {
            // Find the partial view
            var viewResult = _engine.FindPartialView(_controllerContext, name, false);
            if (viewResult == null || viewResult.View == null) return null;

            // Return its template if found
            var mustacheView = viewResult.View as MustacheView;
            return mustacheView != null ? mustacheView.GetTemplate() : null;
        }
    }
}