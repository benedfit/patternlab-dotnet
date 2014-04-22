using System;
using System.IO;
using System.Text;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Mvc;
using PatternLab.Core.Models;

namespace PatternLab.Core.Mustache
{
    public class MustacheView : IView
    {
        private readonly MustacheViewEngine _engine;
        private readonly ControllerContext _controllerContext;
        private readonly string _masterPath;
        private readonly string _physicalMasterPath;
        private readonly string _physicalViewPath;
        private readonly string _viewPath;
        private readonly VirtualPathProvider _virtualPathProvider;

        public MustacheView(MustacheViewEngine engine, ControllerContext controllerContext, VirtualPathProvider virtualPathProvider, string viewPath, string masterPath)
        {
            _engine = engine;
            _controllerContext = controllerContext;
            _virtualPathProvider = virtualPathProvider;
            _viewPath = viewPath;
            _masterPath = masterPath;
            _physicalViewPath = controllerContext.HttpContext.Server.MapPath(viewPath);
            _physicalMasterPath = controllerContext.HttpContext.Server.MapPath(masterPath);
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var viewTemplate = GetTemplate();
            var data = viewContext.ViewData.Model;

            if (!string.IsNullOrEmpty(_masterPath))
            {
                var masterTemplate = LoadTemplate(_physicalMasterPath, _masterPath);
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
            return LoadTemplate(_physicalViewPath, _viewPath);
        }

        private MustacheTemplate LoadTemplate(string physicalPath, string virtualPath)
        {
            var key = physicalPath;
            if (_controllerContext.HttpContext.Cache[key] != null)
            {
                return (MustacheTemplate)_controllerContext.HttpContext.Cache[key];
            }

            var embeddedResource = _virtualPathProvider.GetFile(virtualPath) as EmbeddedResource;
            var templateSource = embeddedResource != null ? embeddedResource.ReadAllText() : File.ReadAllText(physicalPath);

            var template = new MustacheTemplate();
            template.Load(new StringReader(templateSource));

            _controllerContext.HttpContext.Cache.Insert(key, template,
                embeddedResource != null
                    ? embeddedResource.GetCacheDependency(DateTime.UtcNow)
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