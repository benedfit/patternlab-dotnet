using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core
{
    /// <summary>
    /// The Pattern Lab builder
    /// </summary>
    public class Builder
    {
        private readonly ControllerContext _controllerContext;
        private List<string> _ignoredDirectories;
        private List<string> _ignoredExtensions;
        private readonly PatternProvider _provider;

        /// <summary>
        /// Initialises a new Pattern Lab builder
        /// </summary>
        /// <param name="provider">The pattern provider</param>
        /// <param name="controllerContext">The current controller context</param>
        public Builder(PatternProvider provider, ControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
            _provider = provider;
        }

        /// <summary>
        /// Cleans out all files and sub-directories within a directory
        /// </summary>
        /// <param name="directory">The directory to clean</param>
        public bool CleanAll(DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists) return true;

            var cleaned = false;

            // Delete all files, except those with no extension (e.g. README files)
            foreach (
                var file in directory.GetFiles().Where(file => !string.IsNullOrEmpty(Path.GetExtension(file.FullName))))
            {
                try
                {
                    file.Delete();
                    cleaned = true;
                }
                catch
                {
                    cleaned = false;
                }
            }

            // Delete all sub directories
            foreach (var subDirectory in directory.GetDirectories())
            {
                try
                {
                    subDirectory.Delete(true);
                    cleaned = true;
                }
                catch
                {
                    cleaned = false;
                }
            }

            return cleaned;
        }

        /// <summary>
        /// Copies all files from one directory to another
        /// </summary>
        /// <param name="source">The source directory</param>
        /// <param name="destination">The destination directory</param>
        public void CopyAll(DirectoryInfo source, DirectoryInfo destination)
        {
            // Create destination directory is if doesn't exists
            CreateDirectory(destination.FullName);

            foreach (var file in source.GetFiles())
            {
                // Copy all files, unless their extension is in the ignore list - http://patternlab.io/docs/pattern-managing-assets.html
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

            // Copy all sub-directories, unless they are in the ignore list - http://patternlab.io/docs/pattern-managing-assets.html
            foreach (var directory in source.GetDirectories())
            {
                if (IgnoredDirectories().Any(d => d.Equals(directory.Name))) continue;

                var targetDirectory =
                    destination.CreateSubdirectory(
                        directory.Name.Replace(
                            PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture), string.Empty));

                // Run copy all again on sub-directory
                CopyAll(directory, targetDirectory);
            }
        }

        /// <summary>
        /// Creates a new directory if it doesn't exist
        /// </summary>
        /// <param name="path">The path to the directory</param>
        public static void CreateDirectory(string path)
        {
            var name = Path.GetDirectoryName(path);
            if (name == null) return;
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
            }
        }

        /// <summary>
        /// Creates a copy of a file from one directory into another from a contents stream
        /// </summary>
        /// <param name="virtualPath">The virtual path to the file</param>
        /// <param name="stream">The contents stream</param>
        /// <param name="source">The source directory</param>
        /// <param name="destination">The destination directory</param>
        public static void CreateFile(string virtualPath, Stream stream, DirectoryInfo source, DirectoryInfo destination)
        {
            // Parse virtual path and create directory
            var filePath = HostingEnvironment.MapPath(virtualPath) ?? string.Empty;

            if (source != null)
            {
                filePath = filePath.Replace(source.FullName, destination.FullName);
            }

            CreateDirectory(filePath);

            // Write stream contents to file
            using (var file = File.Create(filePath))
            {
                stream.CopyTo(file);
            }
        }

        /// <summary>
        /// Creates a copy of a file from one directory into another from a string
        /// </summary>
        /// <param name="virtualPath">The virtual path to the file</param>
        /// <param name="contents">The contents as a string</param>
        /// <param name="source">The source directory</param>
        /// <param name="destination">The destination directory</param>
        public static void CreateFile(string virtualPath, string contents, DirectoryInfo source, DirectoryInfo destination)
        {
            // Parse virtual path and create directory
            var filePath = HostingEnvironment.MapPath(virtualPath) ?? string.Empty;

            if (source != null)
            {
                filePath = filePath.Replace(source.FullName, destination.FullName);
            }

            CreateDirectory(filePath);

            // Write string contents to file
            File.WriteAllText(filePath, contents);
        }

        /// <summary>
        /// The static output generator - http://patternlab.io/docs/net-command-line.html
        /// </summary>
        /// <param name="source">The name of the source directory</param>
        /// <param name="destination">The name of the destination directory</param>
        /// <param name="enableCss">Generate CSS for each pattern. Currently unsupported</param>
        /// <param name="patternsOnly">Generate only the patterns. Does NOT clean the destination folder</param>
        /// <param name="noCache">Set the cacheBuster value to 0</param>
        /// <returns>The results of the generator</returns>
        public string Generate(string source, string destination, bool? enableCss = null, bool? patternsOnly = null, bool? noCache = null)
        {
            var start = DateTime.Now;
            var content = new StringBuilder("configuring pattern lab...<br/>");
            var controller = new Controllers.PatternLabController { ControllerContext = _controllerContext };
            var url = new UrlHelper(_controllerContext.RequestContext);

            // Set location to copy from as root of app
            var sourceDirectory = new DirectoryInfo(source);
            var destinationDirectory = new DirectoryInfo(destination);
            
            // Determine value for {{ cacheBuster }} variable
            var cacheBuster = noCache.HasValue && noCache.Value ? "0" : _provider.CacheBuster();

            // If not only generating patterns, and cleanPubnlic config setting set to true clean destination directory
            if ((patternsOnly.HasValue && !patternsOnly.Value) || !patternsOnly.HasValue &&
                _provider.Setting("cleanPublic").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase))
            {
                // Clean all files 
                CleanAll(destinationDirectory);

                // Copy all files and folders from source to public
                CopyAll(sourceDirectory, destinationDirectory);

                // Create 'Viewer' page
                var view = controller.Index();

                // Capture the view and write its contents to the file
                CreateFile(string.Format("~/{0}", PatternProvider.FileNameViewer), view.Capture(_controllerContext),
                    sourceDirectory, destinationDirectory);

                // Create latest-change.txt
                CreateFile("~/latest-change.txt", cacheBuster, sourceDirectory, destinationDirectory);

                // Create /styleguide/html/styleguide.html
                view = controller.ViewAll(string.Empty, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value);

                // Capture the view and write its contents to the file
                CreateFile(url.RouteUrl("PatternLabStyleguide"), view.Capture(_controllerContext), sourceDirectory,
                    destinationDirectory);

                // Parse embedded resources for required assets
                const string assetRootFolder = "styleguide";
                var assembly = Assembly.GetExecutingAssembly();
                var assetFolders = new[] {"css", "fonts", "html", "images", "js", "vendor"};
                var assetNamespace = string.Format("{0}.EmbeddedResources.{1}.", assembly.GetName().Name, assetRootFolder);
                var assetNames = assembly.GetManifestResourceNames().Where(r => r.Contains(assetNamespace));

                // Create assets from embedded resources
                foreach (var assetName in assetNames)
                {
                    var virtualPath = assetName.Replace(assetNamespace, string.Empty);
                    virtualPath = assetFolders.Aggregate(virtualPath,
                        (current, assetFolder) =>
                            current.Replace(string.Format("{0}.", assetFolder), string.Format("{0}/", assetFolder)));

                    var embeddedResource = new EmbeddedResource(assetName);

                    // Get the contents of the embedded resource and write it to the file
                    CreateFile(string.Format("~/{0}/{1}", assetRootFolder, virtualPath), embeddedResource.Open(),
                        sourceDirectory, destinationDirectory);
                }
            }

            // Clean all files in /patterns
            CleanAll(destinationDirectory.GetDirectories("patterns").FirstOrDefault());

            // Find all patterns that aren't hidden from navigation
            var patterns = _provider.Patterns().Where(p => !p.Hidden).ToList();
            var typeDashes =
                patterns.Where(p => !string.IsNullOrEmpty(p.SubType))
                    .Select(p => p.TypeDash)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .ToList();

            // Create view-all HTML files
            foreach (var typeDash in typeDashes)
            {
                var view = controller.ViewAll(typeDash, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value);

                // Capture the view and write its contents to the file
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
                var view = controller.ViewSingle(pattern.PathDash, PatternProvider.ViewNameViewSingle, null,
                    enableCss.HasValue && enableCss.Value, noCache.HasValue && noCache.Value, string.Empty);

                // Capture the view and write its contents to the file
                CreateFile(virtualPath, view.Capture(_controllerContext), sourceDirectory, destinationDirectory);

                // Create template file
                var extension = _provider.PatternEngine().Extension();

                view = controller.ViewSingle(pattern.PathDash, string.Empty, null, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value, extension);

                // Capture the view and write its contents to the file
                CreateFile(
                    virtualPath.Replace(PatternProvider.FileExtensionHtml, extension),
                    view.Capture(_controllerContext), sourceDirectory, destinationDirectory);

                // Create .escaped.html
                view = controller.ViewSingle(pattern.PathDash, string.Empty, true, enableCss.HasValue && enableCss.Value,
                    noCache.HasValue && noCache.Value, string.Empty);

                // Capture the view and write its contents to the file
                CreateFile(
                    virtualPath.Replace(PatternProvider.FileExtensionHtml, PatternProvider.FileExtensionEscapedHtml),
                    view.Capture(_controllerContext), sourceDirectory, destinationDirectory);
            }

            // Determine the time taken the run the generator
            var elapsed = DateTime.Now - start;

            content.Append("your site has been generated...<br/>");
            content.AppendFormat("site generation took {0} seconds...<br/>", elapsed.TotalSeconds);

            // Randomly prints a saying after the generate is complete
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

            if (sayings.Length > random)
            {
                content.AppendFormat("{0}...<br />", sayings[random]);
            }

            // Display the results of the generator
            return content.ToString();
        }

        /// <summary>
        /// The directories ignored by the generator
        /// </summary>
        /// <returns>A list of directory names</returns>
        public List<string> IgnoredDirectories()
        {
            if (_ignoredDirectories != null) return _ignoredDirectories;

            _ignoredDirectories = _provider.IgnoredDirectories();

            // Add some additional directories that the provider doesn't need to ignore
            _ignoredDirectories.AddRange(new[] {"_patterns", "bin", "config", "obj", "Properties"});

            return _ignoredDirectories;
        }

        /// <summary>
        /// The file extensions ignored by the generator
        /// </summary>
        /// <returns>A list of file extensions</returns>
        public List<string> IgnoredExtensions()
        {
            if (_ignoredExtensions != null) return _ignoredExtensions;

            _ignoredExtensions = _provider.IgnoredExtensions();

            // Add some additional extensions that the provider doesn't need to ignore
            _ignoredExtensions.AddRange(new[] {"asax", "config", "cs", "csproj", "nuspec", "pp", "user"});

            return _ignoredExtensions;
        }
    }
}