using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Caching;
using System.Web.Hosting;
using PatternLab.Core.Providers;

namespace PatternLab.Core
{
    /// <summary>
    /// Denotes an asset that has its 'Build Action' set to 'Embedded Resource'
    /// </summary>
    public class EmbeddedResource : VirtualFile
    {
        /// <summary>
        /// Initialises a new embedded resource
        /// </summary>
        /// <param name="virtualPath">The virtual path to the asset</param>
        public EmbeddedResource(string virtualPath) : base(virtualPath)
        {
            CacheDependency = utcStart => new CacheDependency(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// The cache dependency of the embedded resource
        /// </summary>
        public Func<DateTime, CacheDependency> CacheDependency { get; private set; }

        /// <summary>
        /// Gets the assembly level name of an embedded resource from its virtual path
        /// </summary>
        /// <param name="virtualPath">The virtual path to the asset</param>
        /// <returns>The name of the asset once it has been compiled</returns>
        internal static string GetResourceName(string virtualPath)
        {
            var resourcename = virtualPath;
            var assembly = Assembly.GetExecutingAssembly();

            // The following folders contain embedded resources
            var folders = new[] {"config", "styleguide", "templates"};
            foreach (
                var folder in
                    folders.Where(folder => virtualPath.ToLower().Contains(string.Format("/{0}/", folder.ToLower()))))
            {
                var folderPath = string.Format("{0}/", folder);
                var index = virtualPath.IndexOf(folderPath, StringComparison.InvariantCultureIgnoreCase);
                if (index < 0) continue;

                var fileName = Path.GetFileName(virtualPath);
                var folderName = Regex.Replace(virtualPath.Replace(fileName, string.Empty).Substring(index),
                    folderPath, string.Format("{0}.EmbeddedResources.{1}.", assembly.GetName().Name, folder),
                    RegexOptions.IgnoreCase);

                // Create the embedded resource's assembly level name
                resourcename =
                    string.Concat(
                        folderName.Replace(Path.AltDirectorySeparatorChar, Type.Delimiter)
                            .Replace(PatternProvider.IdentifierSpace, PatternProvider.IdentifierHidden), fileName);
            }

            return resourcename;
        }

        /// <summary>
        /// Open the embedded resource as a stream
        /// </summary>
        /// <returns>The contents of the embedded resource as a stream</returns>
        public override Stream Open()
        {
            // Get assembly level name of asset
            var resourcename = GetResourceName(VirtualPath);
            if (string.IsNullOrEmpty(resourcename)) return Stream.Null;

            var assembly = Assembly.GetExecutingAssembly();
            
            // Search assembly for asset matching the name
            resourcename =
                assembly.GetManifestResourceNames()
                    .FirstOrDefault(r => r.EndsWith(resourcename, StringComparison.InvariantCultureIgnoreCase));

            // Return contents of asset
            var stream = assembly.GetManifestResourceStream(resourcename);
            return stream ?? Stream.Null;
        }

        /// <summary>
        /// Read the contents of an embedded resource
        /// </summary>
        /// <returns>The content of the embedded resource as a string</returns>
        public string ReadAllText()
        {
            using (var stream = Open())
            {
                return stream.Length <= 0 ? string.Empty : new StreamReader(stream).ReadToEnd();
            }
        }
    }
}