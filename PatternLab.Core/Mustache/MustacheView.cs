using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PatternLab.Core.Mustache
{
    public class MustacheView : IView
    {
        private readonly MustacheViewEngine _engine;
        private readonly ControllerContext _controllerContext;
        private readonly string _masterPath;
        private readonly Dictionary<string, object> _parameters;
        private readonly string _viewPath;
        private readonly VirtualPathProvider _virtualPathProvider;

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

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            Render(viewContext.ViewData.Model, writer);
        }

        public void Render(object data, TextWriter writer)
        {
            var viewTemplate = GetTemplate();

            if (!string.IsNullOrEmpty(_masterPath))
            {
                var masterTemplate = LoadTemplate(_masterPath);
                masterTemplate.Render(
                    data,
                    writer,
                    name =>
                    {
                        if (name == "Body")
                        {
                            return GetTemplate();
                        }

                        var template = viewTemplate.GetTemplateDefinition(name);

                        if (template != null)
                        {
                            return template;
                        }

                        return FindPartial(name);
                    });
            }
            else
            {
                GetTemplate().Render(data, writer, FindPartial);
            }
        }

        private MustacheTemplate GetTemplate()
        {
            return LoadTemplate(_viewPath, _parameters);
        }

        private MustacheTemplate LoadTemplate(string virtualPath, Dictionary<string, object> parameters = null)
        {
            var physicalPath = HostingEnvironment.MapPath(virtualPath) ?? string.Empty;
            var serializer = new JavaScriptSerializer();
            var key = string.Format("{0}-{1}", virtualPath, serializer.Serialize(parameters));

            if (_controllerContext.HttpContext.Cache[key] != null)
            {
                return (MustacheTemplate)_controllerContext.HttpContext.Cache[key];
            }

            var embeddedResource = _virtualPathProvider.GetFile(virtualPath) as EmbeddedResource;
            var templateSource = embeddedResource != null ? embeddedResource.ReadAllText() : File.ReadAllText(physicalPath);

            var template = new MustacheTemplate(_parameters);
            template.Load(new StringReader(templateSource));

            _controllerContext.HttpContext.Cache.Insert(key, template,
                embeddedResource != null
                    ? embeddedResource.CacheDependency(DateTime.UtcNow)
                    : new CacheDependency(physicalPath));

            return template;
        }

        private MustacheTemplate FindPartial(string name)
        {
            var viewResult = _engine.FindPartialView(_controllerContext, name, false);
            if (viewResult == null || viewResult.View == null) return null;

            var mustacheView = viewResult.View as MustacheView;
            return mustacheView != null ? mustacheView.GetTemplate() : null;
        }
    }
}