using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PatternLab.Core;
using PatternLab.Core.Handlers;
using PatternLab.Core.Helpers;
using PatternLab.Core.Mustache;
using PatternLab.Core.Providers;

// Module auto registers itself without the need for web.config
[assembly: PreApplicationStartMethod(typeof(HttpModule), "LoadModule")]

namespace PatternLab.Core
{
    /// <summary>
    /// Pattern Lab specific HTTP module
    /// </summary>
    public class HttpModule : IHttpModule
    {
        private const string ControllerName = "PatternLab";            

        /// <summary>
        /// Disposes of the Pattern Lab HTTP module
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initialises the Pattern Lab HTTP module
        /// </summary>
        /// <param name="context">The current context</param>
        public void Init(HttpApplication context)
        {
            // Register embedded resource virtual path provider
            HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourceProvider());

            // Register any configured Areas
            AreaRegistration.RegisterAllAreas();

            // Register Pattern Lab specific routes
            RegisterRoutes(RouteTable.Routes);

            // Add Pattern Lab view engine
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new MustacheViewEngine());

            // Create directory watcher for clearing provider
            const string watcherName = "PatternLabWatcher";
            context.Application.Add(watcherName, new FileSystemWatcher(HttpRuntime.AppDomainAppPath));

            var watcher = (FileSystemWatcher)context.Application[watcherName];
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += WatchFiles;
            watcher.Created += WatchFiles;
            watcher.Deleted += WatchFiles;
            watcher.Renamed += WatchFiles;
        }

        /// <summary>
        /// Fires when the HTTP module dynamically loads
        /// </summary>
        public static void LoadModule()
        {
            // Register the module
            DynamicModuleUtility.RegisterModule(typeof (HttpModule));

            // Create a static file handler for reserved Pattern Lab paths to force request through the .NET pipeline
            var paths = new[]
            {
                PatternProvider.FolderNameAnnotations.StripOrdinals(), PatternProvider.FolderNameConfig,
                PatternProvider.FolderNameData.StripOrdinals(), PatternProvider.FolderNamePattern.StripOrdinals(),
                PatternProvider.FolderNameSnapshots, PatternProvider.FolderNameAssets,
                PatternProvider.FolderNameTemplates
            };

            foreach (var path in paths)
            {
                RegisterHttpHandler(string.Format("{0}{1}", ControllerName, path.ToDisplayCase()),
                    string.Format("{0}/*", path.ToLower()), "*", "System.Web.StaticFileHandler");
            }
        }
        /// <summary>
        /// Registers a HTTP handler in web.config
        /// </summary>
        /// <param name="name">The handler name</param>
        /// <param name="path">The handler path</param>
        /// <param name="verb">The verbs the handler supports</param>
        /// <param name="type">The handler type</param>
        private static void RegisterHttpHandler(string name, string path, string verb, string type)
        {
            // Load web.config
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var section = (IgnoreSection) configuration.GetSection("system.webServer");
            var xml = section.SectionInformation.GetRawXml();
            var save = false;
            
            if (xml == null)
            {
                // Add handler in system.webServer is missing
                xml =
                    string.Format(
                        "<system.webServer><handlers><add name=\"{0}\" path=\"{1}\" verb=\"{2}\" type=\"{3}\" /></handlers></system.webServer>",
                        name, path, verb, type);
                save = true;
            }
            else if (xml.Contains(name))
            {
                // Do nothing if handler is already registered
                return;
            }
            else
            {
                // If handler is missing parse the config and determine where to add it to
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
                        var index = xml.IndexOf("</handlers>", StringComparison.OrdinalIgnoreCase);

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
                    var index = xml.IndexOf("</system.webServer>", StringComparison.OrdinalIgnoreCase);

                    var builder = new StringBuilder(xml);
                    builder.Insert(index, add, 1);
                    xml = builder.ToString();

                    save = true;
                }
            }

            if (!save) return;

            // Save the config if modified
            section.SectionInformation.SetRawXml(xml);
            configuration.Save();
        }

        /// <summary>
        /// Register Pattern Lab specific routes
        /// </summary>
        /// <param name="routes">The existing route collection</param>
        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.Clear();

            var controllerNamespaces = new[] {typeof (Controllers.PatternLabController).Namespace};

            // Routes for assets contained as embedded resources
            routes.Add(PatternProvider.RouteNameAsset, new Route("{root}/{*path}", new RouteValueDictionary(new {}),
                new RouteValueDictionary(
                    new {root = "annotations|config|data|styleguide|templates", path = @"^(?!html).+"}),
                new AssetRouteHandler()));

            // Route snapshots/index.html
            routes.MapRoute(PatternProvider.RouteNameSnapshots,
                string.Concat(PatternProvider.FolderNameSnapshots, "/", PatternProvider.FileNameViewer),
                new {controller = ControllerName, action = PatternProvider.ViewNameSnapshots}, controllerNamespaces);

            // Route styleguide.html
            routes.MapRoute(PatternProvider.RouteNameStyleguide, PatternProvider.FilePathStyleguide,
                new {controller = ControllerName, action = PatternProvider.ViewNameViewAllPage, id = string.Empty},
                controllerNamespaces);

            // Route for 'view all' HTML pages
            routes.MapRoute(PatternProvider.RouteNameViewAll,
                string.Concat(PatternProvider.FolderNamePattern.StripOrdinals(), "/{id}/",
                    PatternProvider.FileNameViewer),
                new {controller = ControllerName, action = PatternProvider.ViewNameViewAllPage}, controllerNamespaces);

            // Route for /patterns/pattern.escaped.html pages
            routes.MapRoute(PatternProvider.RouteNameViewSingleEncoded,
                string.Concat(PatternProvider.FolderNamePattern.StripOrdinals(), "/{id}/{path}",
                    PatternProvider.FileExtensionEscapedHtml),
                new {controller = ControllerName, action = PatternProvider.ViewNameViewSingle, parse = true},
                controllerNamespaces);

            // Route for /patterns/pattern.html pages
            routes.MapRoute(PatternProvider.RouteNameViewSingle,
                string.Concat(PatternProvider.FolderNamePattern.StripOrdinals(), "/{id}/{path}",
                    PatternProvider.FileExtensionHtml),
                new
                {
                    controller = ControllerName,
                    action = PatternProvider.ViewNameViewSingle,
                    masterName = PatternProvider.ViewNameViewSingle
                },
                controllerNamespaces);

            // Route for /patterns/pattern.{pattern engine extension} pages
            routes.MapRoute(PatternProvider.RouteNameViewSingleTemplate,
                string.Concat(PatternProvider.FolderNamePattern.StripOrdinals(), "/{id}/{path}.{extension}"),
                new {controller = ControllerName, action = PatternProvider.ViewNameViewSingle},
                new {extension = @"^(?!html).+"}, controllerNamespaces);

            // Route for viewer page
            routes.MapRoute(PatternProvider.RouteNameDefault, "{action}/{id}",
                new
                {
                    controller = ControllerName,
                    action = PatternProvider.ViewNameViewerPage,
                    id = UrlParameter.Optional
                }, controllerNamespaces);
        }

        /// <summary>
        /// Pattern Lab file system watch
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="e">The file system event argument</param>
        private static void WatchFiles(object source, FileSystemEventArgs e)
        {
            var filePath = e.FullPath;
            var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(directory))
            {
                // Remove application root from string
                directory = directory.Replace(HttpRuntime.AppDomainAppPath, string.Empty);
            }

            // Changes in the following directories always need to force a provider clear
            var includedDirectories = new List<string>
            {
                PatternProvider.FolderNameAnnotations,
                PatternProvider.FolderNameData,
                PatternProvider.FolderNamePattern
            };

            // Ignore hidden directories starting with an underscore
            if (directory.StartsWith(PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture)) &&
                !includedDirectories.Where(directory.StartsWith).Any())
            {
                return;
            }

            var extension = Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1, extension.Length - 1);
            }

            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();

            // Clear the provider if the directory isn't ignored, and the file extension isn't ignored
            if (!provider.IgnoredDirectories().Where(directory.StartsWith).Any() &&
                !provider.IgnoredExtensions().Contains(extension))
            {
                provider.Clear();
            }
        }
    }
}