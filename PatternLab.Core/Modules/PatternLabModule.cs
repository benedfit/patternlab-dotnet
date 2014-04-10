﻿using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PatternLab.Core.Handlers;
using PatternLab.Core.Modules;
using PatternLab.Core.Providers;

[assembly: PreApplicationStartMethod(typeof(PatternLabModule), "LoadModule")]

namespace PatternLab.Core.Modules
{
    public class PatternLabModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourceProvider());

            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
        }

        public static void LoadModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(PatternLabModule));
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.Clear();

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.Add("PatternLabAsset", new Route("{root}/{*path}", new RouteValueDictionary(new { }),
                new RouteValueDictionary(new { root = "Styleguide", path = @"^(?!html).+" }),
                new EmbeddedResourceRouteHandler()));

            routes.MapRoute("PatternLabStyleguide", "styleguide/html/styleguide.html",
                new { controller = "Patterns", action = "Styleguide" },
                new[] { "PatternLab.Core.Controllers" });

            routes.MapRoute("PatternLabViewAll", "patterns/{id}/index.html",
                new { controller = "Patterns", action = "ViewAll" },
                new[] { "PatternLab.Core.Controllers" });

            routes.MapRoute("PatternLabViewSingle", "patterns/{id}/{path}.html",
                new { controller = "Patterns", action = "ViewSingle" },
                new[] { "PatternLab.Core.Controllers" });

            routes.MapRoute("PatternLabDefault", "{controller}/{action}/{id}",
                new { controller = "Patterns", action = "Index", id = UrlParameter.Optional },
                new[] { "PatternLab.Core.Controllers" });
        }
    }
}