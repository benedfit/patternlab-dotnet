using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;

namespace PatternLab.Core.Providers
{
    public class EmbeddedResourceProvider : VirtualPathProvider
    {
        public static bool EmbeddedResourceFileExists(string virtualPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcename = EmbeddedResource.GetResourceName(virtualPath);
            var result = resourcename != null &&
                          assembly.GetManifestResourceNames()
                              .Contains(resourcename, StringComparer.InvariantCultureIgnoreCase);
            return result;
        }

        public override bool FileExists(string virtualPath)
        {
            return base.FileExists(virtualPath) || EmbeddedResourceFileExists(virtualPath);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies,
            DateTime utcStart)
        {
            var resource = new EmbeddedResource(virtualPath);
            return resource.GetCacheDependency(utcStart);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return !base.FileExists(virtualPath) ? new EmbeddedResource(virtualPath) : base.GetFile(virtualPath);
        }
    }
}