using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core
{
    public class Builder
    {
        private readonly ControllerContext _controllerContext;
        private List<string> _ignoredDirectories;
        private List<string> _ignoredExtensions;
        private readonly PatternProvider _provider;

        public Builder(PatternProvider provider, ControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
            _provider = provider;
        }

        public void CleanAll(DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists) return;

            foreach (
                var file in directory.GetFiles().Where(file => !string.IsNullOrEmpty(Path.GetExtension(file.FullName))))
            {
                file.Delete();
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
            }
        }

        public void CopyAll(DirectoryInfo source, DirectoryInfo destination)
        {
            CreateDirectory(destination.FullName);

            foreach (var file in source.GetFiles())
            {
                var extension = Path.GetExtension(file.FullName);
                if (!string.IsNullOrEmpty(extension))
                {
                    extension = extension.Substring(1, extension.Length - 1);
                }

                if (!IgnoredExtensions().Contains(extension) &&
                    !file.Name.StartsWith(PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture)))
                {
                    file.CopyTo(Path.Combine(destination.ToString(), file.Name), true);
                }
            }

            foreach (var directory in source.GetDirectories())
            {
                if (IgnoredDirectories().Any(d => d.Equals(directory.Name))) continue;

                var targetDirectory =
                    destination.CreateSubdirectory(
                        directory.Name.Replace(
                            PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture), string.Empty));
                CopyAll(directory, targetDirectory);
            }
        }

        public void CreateFile(string virtualPath, Stream stream, DirectoryInfo source, DirectoryInfo destination)
        {
            var filePath = HostingEnvironment.MapPath(virtualPath) ?? string.Empty;
            filePath = filePath.Replace(source.FullName, destination.FullName);

            CreateDirectory(filePath);

            using (var file = File.Create(filePath))
            {
                stream.CopyTo(file);
            }
        }

        public void CreateFile(string virtualPath, string contents, DirectoryInfo source, DirectoryInfo destination)
        {
            var filePath = HostingEnvironment.MapPath(virtualPath) ?? string.Empty;
            filePath = filePath.Replace(source.FullName, destination.FullName);

            CreateDirectory(filePath);

            File.WriteAllText(filePath, contents);
        }

        public void CreateDirectory(string path)
        {
            var name = Path.GetDirectoryName(path);
            if (name == null) return;
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
            }
        }

        public string Generate(string destination, bool? enableCss = null, bool? patternsOnly = null, bool? noCache = null)
        {
            var start = DateTime.Now;
            var content = new StringBuilder("configuring pattern lab...<br/>");
            var controller = new Controllers.PatternLabController { ControllerContext = _controllerContext };
            var url = new UrlHelper(_controllerContext.RequestContext);
            var sourceDirectory = new DirectoryInfo(HttpRuntime.AppDomainAppPath);
            var destinationDirectory = new DirectoryInfo(string.Format("{0}{1}\\", HttpRuntime.AppDomainAppPath, destination));
            var cacheBuster = noCache.HasValue && noCache.Value ? "0" : _provider.CacheBuster();

            if ((patternsOnly.HasValue && !patternsOnly.Value) || !patternsOnly.HasValue &&
                _provider.Setting("cleanPublic").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase))
            {
                // Clean all files 
                CleanAll(destinationDirectory);

                // Copy all files and folders from source to public
                CopyAll(sourceDirectory, destinationDirectory);

                // Create index.html
                var view = controller.Index();

                CreateFile(string.Format("~/{0}", PatternProvider.FileNameIndex), view.Capture(_controllerContext),
                    sourceDirectory, destinationDirectory);

                // Create latest-change.txt
                CreateFile("~/latest-change.txt", cacheBuster, sourceDirectory, destinationDirectory);

                // Create /styleguide/html/styleguide.html
                view = controller.ViewAll(string.Empty, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value);

                CreateFile(url.RouteUrl("PatternLabStyleguide"), view.Capture(_controllerContext), sourceDirectory,
                    destinationDirectory);

                const string assetRootFolder = "styleguide";
                var assembly = Assembly.GetExecutingAssembly();
                var assetFolders = new[] {"css", "fonts", "html", "images", "js", "vendor"};
                var assetNamespace = string.Format("{0}.{1}.", assembly.GetName().Name, assetRootFolder);
                var assetNames = assembly.GetManifestResourceNames().Where(r => r.Contains(assetNamespace));

                // Create assets
                foreach (var assetName in assetNames)
                {
                    var virtualPath = assetName.Replace(assetNamespace, string.Empty);
                    virtualPath = assetFolders.Aggregate(virtualPath,
                        (current, assetFolder) =>
                            current.Replace(string.Format("{0}.", assetFolder), string.Format("{0}/", assetFolder)));

                    var embeddedResource = new EmbeddedResource(assetName);

                    CreateFile(string.Format("~/{0}/{1}", assetRootFolder, virtualPath), embeddedResource.Open(),
                        sourceDirectory, destinationDirectory);
                }
            }

            // Clean all files 
            CleanAll(destinationDirectory.GetDirectories("patterns").FirstOrDefault());

            var patterns = _provider.Patterns().Where(p => !p.Hidden).ToList();
            var typeDashes =
                patterns.Where(p => !string.IsNullOrEmpty(p.SubType))
                    .Select(p => p.TypeDash)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .ToList();

            // Create view-all files
            foreach (var typeDash in typeDashes)
            {
                var view = controller.ViewAll(typeDash, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value);
                CreateFile(url.RouteUrl("PatternLabViewAll", new { id = typeDash }), view.Capture(_controllerContext),
                    sourceDirectory, destinationDirectory);
            }

            // Create pattern files
            foreach (var pattern in patterns)
            {
                var virtualPath =
                    url.RouteUrl("PatternLabViewSingle", new { id = pattern.PathDash, path = pattern.PathDash }) ??
                    string.Empty;

                // Create .html
                var view = controller.ViewSingle(pattern.PathDash, PatternProvider.FileNameLayout, null,
                    enableCss.HasValue && enableCss.Value, noCache.HasValue && noCache.Value);
                CreateFile(virtualPath, view.Capture(_controllerContext), sourceDirectory, destinationDirectory);

                // Create .mustache
                view = controller.ViewSingle(pattern.PathDash, string.Empty, null, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value);
                CreateFile(
                    virtualPath.Replace(PatternProvider.FileExtensionHtml, PatternProvider.FileExtensionMustache),
                    view.Capture(_controllerContext), sourceDirectory, destinationDirectory);

                // Create .escaped.html
                view = controller.ViewSingle(pattern.PathDash, string.Empty, true, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value);
                CreateFile(
                    virtualPath.Replace(PatternProvider.FileExtensionHtml, PatternProvider.FileExtensionEscapedHtml),
                    view.Capture(_controllerContext), sourceDirectory, destinationDirectory);
            }

            var elapsed = DateTime.Now - start;
            var random = new Random().Next(60);
            var sayings = new[]
            {
                "have fun storming the castle",
                "be well, do good work, and keep in touch",
                "may the sun shine, all day long",
                "smile",
                "namaste",
                "walk as if you are kissing the earth with your feet",
                "to be beautiful means to be yourself",
                "i was thinking of the immortal words of socrates, who said \"...i drank what?\"",
                "let me take this moment to compliment you on your fashion sense, particularly your slippers",
                "42",
                "he who controls the spice controls the universe",
                "the greatest thing you'll ever learn is just to love and be loved in return",
                "nice wand",
                "i don't have time for a grudge match with every poseur in a parka"
            };

            content.Append("your site has been generated...<br/>");
            content.AppendFormat("site generation took {0} seconds...<br/>", elapsed.TotalSeconds);

            // Randomly prints a saying after the generate is complete
            if (sayings.Length > random)
            {
                content.AppendFormat("{0}...<br />", sayings[random]);
            }

            return content.ToString();
        }

        public List<string> IgnoredDirectories()
        {
            if (_ignoredDirectories != null) return _ignoredDirectories;

            _ignoredDirectories = _provider.IgnoredDirectories();
            _ignoredDirectories.AddRange(new[] {"_patterns", "bin", "config", "obj", "Properties"});

            return _ignoredDirectories;
        }

        public List<string> IgnoredExtensions()
        {
            if (_ignoredExtensions != null) return _ignoredExtensions;

            _ignoredExtensions = _provider.IgnoredExtensions();
            _ignoredExtensions.AddRange(new[] { "asax", "config", "cs", "csproj", "user" });

            return _ignoredExtensions;
        }
    }
}