using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using PatternLab.Core.Models;

namespace PatternLab.Core.Providers
{
    public interface IPatternProvider
    {
        ViewDataDictionary Data();
        List<Pattern> Patterns();
    }

    public class PatternProvider : IPatternProvider
    {
        public static string DataExtension = ".json";
        public static string DataFolder = "~/_data";
        public static char IdentifierHidden = '_';
        public static char IdentifierPsuedo = '~';
        public static char IdenfifierState = '@';
        public static string PatternsExtension = ".mustache";
        public static string PatternsFolder = "~/_patterns";

        private ViewDataDictionary _data;
        private List<Pattern> _patterns;


        public ViewDataDictionary Data()
        {
            if (_data != null) return _data;
            _data = new ViewDataDictionary();

            var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(DataFolder));

            var dataFiles = root.GetFiles(string.Concat("*", DataExtension), SearchOption.AllDirectories);
            foreach (var dataFile in dataFiles)
            {
                foreach (var item in JObject.Parse(File.ReadAllText(dataFile.FullName)))
                {
                    if (_data.ContainsKey(item.Key))
                    {
                        _data[item.Key] = item.Value;
                    }
                    else
                    {
                        _data.Add(item.Key, item.Value);
                    }
                }
            }

            return _data;
        }

        public List<Pattern> Patterns()
        {
            if (_patterns != null) return _patterns;

            var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(PatternsFolder));

            var views =
                root.GetFiles(string.Concat("*", PatternsExtension), SearchOption.AllDirectories)
                    .Where(
                        v =>
                            v.Directory != null && v.Directory.FullName != root.FullName &&
                            !v.Name.StartsWith(IdentifierHidden.ToString(CultureInfo.InvariantCulture)));

            _patterns = views.Select(v => new Pattern(v.FullName)).ToList();

            var pseudoViews = root.GetFiles(string.Concat("*", DataExtension), SearchOption.AllDirectories)
                .Where(
                    v =>
                        v.Directory != null && v.Directory.FullName != root.FullName &&
                        v.Name.Contains(IdentifierPsuedo) && !v.Name.StartsWith(IdentifierHidden.ToString(CultureInfo.InvariantCulture)));

            _patterns.AddRange(pseudoViews.Select(v => new Pattern(v.FullName)));
            _patterns = _patterns.OrderBy(p => p.PathDash).ToList();

            return _patterns;
        }
    }
}