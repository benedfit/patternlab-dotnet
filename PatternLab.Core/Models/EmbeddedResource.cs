using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;

namespace PatternLab.Core.Models
{
    public class EmbeddedResource : VirtualFile
    {
        public EmbeddedResource(string virtualPath) : base(virtualPath) 
        {
            GetCacheDependency = (utcStart) => new CacheDependency(Assembly.GetExecutingAssembly().Location);
        }

        public Func<DateTime, CacheDependency> GetCacheDependency { get; private set; }

        internal static string GetResourceName(string virtualPath)
        {
            var resourcename = virtualPath;

            var folders = new[] { "Styleguide", "Views" };
            foreach (var folder in folders.Where(folder => virtualPath.ToLower().Contains(string.Format("/{0}/", folder.ToLower()))))
            {
                resourcename = virtualPath
                    .Substring(virtualPath.IndexOf(string.Format("{0}/", folder),
                        StringComparison.InvariantCultureIgnoreCase))
                    .Replace(string.Format("{0}/", folder), string.Format("PatternLab.Core.{0}.", folder))
                    .Replace("/", ".");
            }

            return resourcename;
        }

        public override Stream Open()
        {
            var resourcename = GetResourceName(VirtualPath);
            if (string.IsNullOrEmpty(resourcename)) return Stream.Null;

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcename);
            return stream ?? Stream.Null;
        }
    }
}
