using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Caching;
using System.Web.Hosting;

namespace PatternLab.Core.Models
{
    public class EmbeddedResource : VirtualFile
    {
        public EmbeddedResource(string virtualPath) : base(virtualPath)
        {
            GetCacheDependency = utcStart => new CacheDependency(Assembly.GetExecutingAssembly().Location);
        }

        public Func<DateTime, CacheDependency> GetCacheDependency { get; private set; }

        internal static string GetResourceName(string virtualPath)
        {
            string resourcename = string.Empty;

            var folders = new[] {"styleguide", "Views"};
            foreach (
                var folder in
                    folders.Where(folder => virtualPath.ToLower().Contains(string.Format("/{0}/", folder.ToLower()))))
            {
                var folderPath = string.Format("{0}/", folder);
                var index = virtualPath.IndexOf(folderPath, StringComparison.InvariantCultureIgnoreCase);
                if (index >= 0)
                {
                    resourcename = Regex.Replace(virtualPath.Substring(index), folderPath,
                        string.Format("PatternLab.Core.{0}.", folder),
                        RegexOptions.IgnoreCase).Replace('/', '.');
                }
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