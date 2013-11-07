using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace PatternLab.Models
{
    public class Pattern
    {
        public const string PatternsPath = "~/Views/Patterns";

        public string Id
        {
            get { return string.Format("{0}-{1}", Level, Name); }
        }

        public string Level
        {
            get { return StripOrdinals(Url.Replace(PatternsPath, string.Empty)).Split('/')[0]; }
        }

        public string Name
        {
            get { return StripOrdinals(Path.GetFileNameWithoutExtension(View)); }
        }

        public string Url
        {
            get { return AbsolutePathToUrl(View); }
        }

        public string View { get; set; }

        public string AbsolutePathToUrl(string path)
        {
            return path.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], "~/").Replace(@"\", "/");
        }

        public string StripOrdinals(string value)
        {
            return Regex.Replace(value, @"^[\/]*[\d]+-", string.Empty);
        }
    }
}