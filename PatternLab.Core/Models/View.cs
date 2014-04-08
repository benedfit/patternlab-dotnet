using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using PatternLab.Core.Providers;

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

        public string GroupName
        {
            get { return IdFragment(1); }
        }

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

        public string Url
        {
            get { return AbsolutePathToUrl(FilePath); }
        }

        private string IdFragment(int index)
        {
            var fragments = Id.Split('_').ToList();
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