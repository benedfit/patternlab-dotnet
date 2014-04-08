using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using PatternLab.Core.Models;
using System.Web.Caching;
using System.Collections;
using System;

namespace PatternLab.Core.Providers
{
    public class EmbeddedResourceProvider : VirtualPathProvider
    {
        private static bool EmbeddedResourceFileExists(string virtualPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcename = EmbeddedResource.GetResourceName(virtualPath);
            var result = resourcename != null && assembly.GetManifestResourceNames().Contains(resourcename, StringComparer.InvariantCultureIgnoreCase);
            return result;
        }

        private static bool EmbeddedResourceDirectoryExists(string virtualPath)
        {
            return true;
        }

        public override bool FileExists(string virtualPath)
        {
            return base.FileExists(virtualPath) || EmbeddedResourceFileExists(virtualPath);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            var resource = new EmbeddedResource(virtualPath);
            if (resource != null)
            {
                return resource.GetCacheDependency(utcStart);
            }

            if (DirectoryExists(virtualPath) || FileExists(virtualPath))
            {
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
            }

            return null;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return !base.FileExists(virtualPath) ? new EmbeddedResource(virtualPath) : base.GetFile(virtualPath);
        }
    }
}
