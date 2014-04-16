using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json.Linq;
using PatternLab.Core.Models;

namespace PatternLab.Core.Providers
{
    public interface IPatternProvider
    {
        IniData Config();
        ViewDataDictionary Data();
        List<Pattern> Patterns();
    }

    public class PatternProvider : IPatternProvider
    {
        public static string ConfigPath = "~/config/config.ini";
        public static string DataExtension = ".json";
        public static string DataFolder = "~/_data";
        public static char IdentifierHidden = '_';
        public static char IdentifierPsuedo = '~';
        public static char IdenfifierState = '@';
        public static string PatternsExtension = ".mustache";
        public static string PatternsFolder = "~/_patterns";

        private IniData _config;
        private ViewDataDictionary _data;
        private List<Pattern> _patterns;

        public IniData Config()
        {
            if (_config != null) return _config;

            var parser = new FileIniDataParser();
            parser.Parser.Configuration.AllowKeysWithoutSection = true;
            parser.Parser.Configuration.SkipInvalidLines = true;

            _config = parser.ReadFile(HttpContext.Current.Server.MapPath(ConfigPath));
            return _config;
        }

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
                    .Where(v => v.Directory != null && v.Directory.FullName != root.FullName);

            _patterns = views.Select(v => new Pattern(v.FullName)).ToList();

            var pseudoViews = root.GetFiles(string.Concat("*", DataExtension), SearchOption.AllDirectories)
                .Where(
                    v =>
                        v.Directory != null && v.Directory.FullName != root.FullName &&
                        v.Name.Contains(IdentifierPsuedo));

            _patterns.AddRange(pseudoViews.Select(v => new Pattern(v.FullName)));
            _patterns = _patterns.OrderBy(p => p.PathDash).ToList();

            return _patterns;
        }
    }
}