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
            var resourcename = string.Empty;

            var folders = new[] {"styleguide", "templates", "Views"};
            foreach (
                var folder in
                    folders.Where(folder => virtualPath.ToLower().Contains(string.Format("/{0}/", folder.ToLower()))))
            {
                var folderPath = string.Format("{0}/", folder);
                var index = virtualPath.IndexOf(folderPath, StringComparison.InvariantCultureIgnoreCase);
                if (index < 0) continue;

                var fileName = Path.GetFileName(virtualPath);
                var folderName = Regex.Replace(virtualPath.Replace(fileName, string.Empty).Substring(index),
                    folderPath, string.Format("PatternLab.Core.{0}.", folder), RegexOptions.IgnoreCase);

                resourcename = string.Concat(folderName.Replace('/', '.').Replace('-', '_'), fileName);
            }

            return resourcename;
        }

        public override Stream Open()
        {
            var resourcename = GetResourceName(VirtualPath);
            if (string.IsNullOrEmpty(resourcename)) return Stream.Null;

            var assembly = Assembly.GetExecutingAssembly();
            
            resourcename =
                assembly.GetManifestResourceNames()
                    .FirstOrDefault(r => r.EndsWith(resourcename, StringComparison.InvariantCultureIgnoreCase));

            var stream = assembly.GetManifestResourceStream(resourcename);
            return stream ?? Stream.Null;
        }

        public string ReadAllText()
        {
            using (var stream = Open())
            {
                return stream.Length <= 0 ? string.Empty : new StreamReader(stream).ReadToEnd();
            }
        }
    }
}
