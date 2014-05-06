using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;

namespace PatternLab.Core.Providers
{
    /// <summary>
    /// A VirtualPathProvider for handling embedded resources
    /// </summary>
    public class EmbeddedResourceProvider : VirtualPathProvider
    {
        /// <summary>
        /// Checks if the embedded resource exists
        /// </summary>
        /// <param name="virtualPath">The virtual path to the embedded resource</param>
        /// <returns>Wheter it exists or not</returns>
        public static bool EmbeddedResourceFileExists(string virtualPath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Find the assembly level name of the embedded resource from its virtual path
            var resourcename = EmbeddedResource.GetResourceName(virtualPath);

            // Return whether the assembly contains an embedded resource of that name
            return resourcename != null &&
                   assembly.GetManifestResourceNames()
                       .Contains(resourcename, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Check if the file exists
        /// </summary>
        /// <param name="virtualPath">The virtual path to the file</param>
        /// <returns>Whether it exists or not</returns>
        public override bool FileExists(string virtualPath)
        {
            // Check if the file exists on disk, or as an embedded resource
            return base.FileExists(virtualPath) || EmbeddedResourceFileExists(virtualPath);
        }

        /// <summary>
        /// Get the cache dependency for an embedded resource
        /// </summary>
        /// <param name="virtualPath">The virtual path to the file</param>
        /// <param name="virtualPathDependencies">The virtual path dependencies associated with the cache dependency</param>
        /// <param name="utcStart">The start date</param>
        /// <returns>The cache dependency</returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies,
            DateTime utcStart)
        {
            // Find the resource and return its cache dependency
            var resource = new EmbeddedResource(virtualPath);
            return resource.CacheDependency(utcStart);
        }

        /// <summary>
        /// Gets a file
        /// </summary>
        /// <param name="virtualPath">The virtual path to the file</param>
        /// <returns>The virtual file</returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            // Return the file from disk, or from embedded resources
            return !base.FileExists(virtualPath) ? new EmbeddedResource(virtualPath) : base.GetFile(virtualPath);
        }
    }
}