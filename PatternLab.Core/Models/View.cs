using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using PatternLab.Core.Providers;
using System;

namespace PatternLab.Core.Models
{
    public class View
    {
        public string Annotations { get; set; }

        public string DisplayName
        {
            get { return StripOrdinals(Path.GetFileNameWithoutExtension(FilePath)); }
        }

        public string FilePath { get; set; }

        public string Id
        {
            get
            {
                return
                    Path.GetFileNameWithoutExtension(
                        StripOrdinals(string.Join("_", Url.Replace(ViewsProvider.FolderPath, string.Empty).Split('/'))));
            }
        }

        public string TypeName
        {
            get { return IdFragment(0); }
        }

        public string SubTypeName
        {
            get { return IdFragment(1); }
        }

        public string Url
        {
            get { return AbsolutePathToUrl(FilePath); }
        }

        private string IdFragment(int index)
        {
            var fragments = Id.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            fragments.Remove(DisplayName);
            return fragments.Count > index ? fragments[index] : string.Empty;
        }

        private static string AbsolutePathToUrl(string path)
        {
            return path.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], "~/")
                .Replace(@"\", "/");
        }

        private static string StripOrdinals(string value)
        {
            return Regex.Replace(value, @"[\~\/\-\d]+", string.Empty);
        }
    }
}