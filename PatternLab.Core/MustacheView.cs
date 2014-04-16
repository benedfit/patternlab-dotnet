using System.IO;
using System.Web.Caching;
using System.Web.Mvc;
using Nustache.Core;

namespace PatternLab.Core
{
    public class MustacheView : IView
    {
        private readonly MustacheViewEngine _engine;
        private readonly ControllerContext _controllerContext;
        private readonly string _physicalPath;

        public MustacheView(MustacheViewEngine engine, ControllerContext controllerContext, string physicalPath)
        {
            _engine = engine;
            _controllerContext = controllerContext;
            _physicalPath = physicalPath;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            GetTemplate().Render(Controllers.PatternsController.Provider.Data(), writer, FindPartial);
        }

        private Template GetTemplate()
        {
            return LoadTemplate(_physicalPath);
        }

        private Template LoadTemplate(string path)
        {
            var key = path;
            if (_controllerContext.HttpContext.Cache[key] != null)
            {
                return (Template) _controllerContext.HttpContext.Cache[key];
            }

            var templateSource = File.ReadAllText(path);
            var template = new Template();
            template.Load(new StringReader(templateSource));

            _controllerContext.HttpContext.Cache.Insert(key, template, new CacheDependency(path));

            return template;
        }

        private Template FindPartial(string name)
        {
            var viewResult = _engine.FindPartialView(_controllerContext, name, false);
            if (viewResult == null || viewResult.View == null) return null;

            var mustacheView = viewResult.View as MustacheView;
            return mustacheView != null ? mustacheView.GetTemplate() : null;
        }
    }
}