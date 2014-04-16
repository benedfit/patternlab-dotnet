using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PatternLab.Core.Handlers;
using PatternLab.Core.Modules;
using PatternLab.Core.Providers;

[assembly: PreApplicationStartMethod(typeof (PatternLabModule), "LoadModule")]

namespace PatternLab.Core.Modules
{
    public class PatternLabModule : IHttpModule
    {
        public void Dispose() { }

        public void Init(HttpApplication context)
        {
            HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourceProvider());

            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Add(new MustacheViewEngine());
        }

        public static void LoadModule()
        {
            DynamicModuleUtility.RegisterModule(typeof (PatternLabModule));

            RegisterHttpHandler("PatternLabData", "data/*", "*", "System.Web.StaticFileHandler");
            RegisterHttpHandler("PatternLabPatterns", "patterns/*/*.html", "*", "System.Web.StaticFileHandler");
            RegisterHttpHandler("PatternLabStyleguide", "styleguide/*", "*", "System.Web.StaticFileHandler");
        }

        private static void RegisterHttpHandler(string name, string path, string verb, string type)
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var section = (IgnoreSection) configuration.GetSection("system.webServer");
            var xml = section.SectionInformation.GetRawXml();
            var save = false;
            
            if (xml == null)
            {
                xml =
                    string.Format(
                        "<system.webServer><handlers><add name=\"{0}\" path=\"{1}\" verb=\"{2}\" type=\"{3}\" /></handlers></system.webServer>",
                        name, path, verb, type);
                save = true;
            }
            else if (xml.Contains(name))
            {
                return;
            }
            else
            {
                xml =
                    Regex.Replace(
                        Regex.Replace(
                            Regex.Replace(
                                Regex.Replace(
                                    Regex.Replace(Regex.Replace(xml, @"<handlers\s*/>", "<handlers/>"),
                                        @"</handlers\s*>", "</handlers>"), @"<handlers\s*>", "<handlers>"),
                                @"<system.webServer\s*>", "<system.webServer>"), @"<system.webServer\s*/>",
                            "<system.webServer/>"), @"</system.webServer\s*>", "</system.webServer>");
                if (xml.Contains("<handlers/>"))
                {
                    xml = xml.Replace("<handlers/>",
                        string.Format(
                            "<handlers><add name=\"{0}\" path=\"{1}\" verb=\"{2}\" type=\"{3}\" /></handlers>", name,
                            path, verb, type));
                    save = true;
                }
                else if (xml.Contains("<handlers"))
                {
                    var add = string.Format("<add name=\"{0}\" path=\"{1}\" verb=\"{2}\" type=\"{3}\"", name, path,
                        verb, type);
                    if (!xml.Contains(add))
                    {
                        add = add + "/>";
                        var index = xml.IndexOf("</handlers>", System.StringComparison.OrdinalIgnoreCase);

                        var builder = new StringBuilder(xml);
                        builder.Insert(index, add, 1);
                        xml = builder.ToString();

                        save = true;
                    }
                }
                else if (xml.Contains("<system.webServer/>"))
                {
                    var add =
                        string.Format(
                            "<system.webServer><handlers><add name=\"{0}\" path=\"{1}\" verb=\"{2}\" type=\"{3}\" /></handlers></system.webServer>",
                            name, path, verb, type);
                    xml = xml.Replace("<system.webServer/>", add);
                    save = true;
                }
                else if (xml.Contains("<system.webServer"))
                {
                    var add =
                        string.Format(
                            "<handlers><add name=\"{0}\" path=\"{1}\" verb=\"{2}\" type=\"{3}\" /></handlers>", name,
                            path, verb, type);
                    var index = xml.IndexOf("</system.webServer>", System.StringComparison.OrdinalIgnoreCase);

                    var builder = new StringBuilder(xml);
                    builder.Insert(index, add, 1);
                    xml = builder.ToString();

                    save = true;
                }
            }

            if (!save) return;

            section.SectionInformation.SetRawXml(xml);
            configuration.Save();
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.Clear();

            routes.Add("PatternLabAsset", new Route("{root}/{*path}", new RouteValueDictionary(new {}),
                new RouteValueDictionary(new {root = "styleguide|data", path = @"^(?!html).+"}),
                new AssetRouteHandler()));

            routes.MapRoute("PatternLabStyleguide", "styleguide/html/styleguide.html",
                new {controller = "Patterns", action = "ViewAll", id = string.Empty},
                new[] {"PatternLab.Core.Controllers"});

            routes.MapRoute("PatternLabViewAll", "patterns/{id}/index.html",
                new {controller = "Patterns", action = "ViewAll"},
                new[] {"PatternLab.Core.Controllers"});

            routes.MapRoute("PatternLabViewSingle", "patterns/{id}/{path}.html",
                new {controller = "Patterns", action = "ViewSingle"},
                new[] {"PatternLab.Core.Controllers"});

            routes.MapRoute("PatternLabDefault", "{controller}/{action}/{id}",
                new {controller = "Patterns", action = "Index", id = UrlParameter.Optional},
                new[] {"PatternLab.Core.Controllers"});
        }
    }
}