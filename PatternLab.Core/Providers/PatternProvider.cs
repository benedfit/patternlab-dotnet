using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using IniParser;
using IniParser.Model;
using PatternLab.Core.Helpers;
using PatternLab.Core.Models;

namespace PatternLab.Core.Providers
{
    public interface IPatternProvider
    {
        string CacheBuster();
        void Clear();
        IniData Config();
        ViewDataDictionary Data();
        List<Pattern> Patterns();
        string Setting(string settingName);
    }

    public class PatternProvider : IPatternProvider
    {
        public static string ConfigPath = "~/config/config.ini";
        public static string DataExtension = ".json";
        public static string DataFolder = "~/_data";
        public static char IdentifierHidden = '_';
        public static char IdentifierParameter = ':';
        public static char IdentifierPsuedo = '~';
        public static char IdentifierState = '@';
        public static string PatternsExtension = ".mustache";
        public static string PatternsFolder = "~/_patterns";

        private string _cacheBuster;
        private IniData _config;
        private ViewDataDictionary _data;
        private List<Pattern> _patterns;

        public string CacheBuster()
        {
            if (!string.IsNullOrEmpty(_cacheBuster)) return _cacheBuster;

            bool enabled;
            if (!Boolean.TryParse(Setting("cacheBusterOn"), out enabled))
            {
                enabled = false;
            }

            _cacheBuster = enabled ? DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) : "0";
            return _cacheBuster;
        }

        public void Clear()
        {
            _config = null;
            _data = null;
        }

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

            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddresses = host.AddressList;
            var ipAddress = ipAddresses[ipAddresses.Length - 1].ToString();

            var ishSettings = Setting("ishControlsHide").Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var hiddenIshControls = ishSettings.ToDictionary(s => s.Trim(), s => true);

            if (Setting("pageFollowNav").Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                hiddenIshControls.Add("tools-follow", true);
            }

            if (Setting("autoReloadNav").Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                hiddenIshControls.Add("tools-reload", true);
            }

            var patternPaths = new Dictionary<string, object>();
            var viewAllPaths = new Dictionary<string, object>();
            var patternTypes = new List<object>();

            var patterns =
                Patterns()
                    .Where(p => !p.Hidden)
                    .ToList();

            if (patterns.Any())
            {
                var types = Patterns().Select(p => p.Type).Distinct().ToList();
                foreach (var type in types)
                {
                    var typeName = type.StripOrdinals();
                    var typeDisplayName = typeName.ToDisplayCase();

                    var typeDetails =
                        new
                        {
                            patternTypeLC = typeName,
                            patternTypeUC = typeDisplayName,
                            patternTypeItems = new List<object>(),
                            patternItems = new List<object>()
                        };

                    var typedPatterns =
                        patterns.Where(p => p.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    var subTypes =
                        typedPatterns.Select(p => p.SubType).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

                    var typedPatternPaths = new Dictionary<string, string>();
                    var subTypePaths = new Dictionary<string, string>();

                    if (subTypes.Any())
                    {
                        foreach (var subType in subTypes)
                        {
                            var subTypeName = subType.StripOrdinals();
                            var subTypeDisplayName = subTypeName.ToDisplayCase();
                            var subTypePath = string.Format("{0}-{1}", type, subType);

                            var subTypeDetails = new
                            {
                                patternSubtypeLC = subTypeName,
                                patternSubtypeUC = subTypeDisplayName,
                                patternSubtypeItems = new List<object>()
                            };

                            var subTypedPatterns =
                                patterns.Where(
                                    p =>
                                        p.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase) &&
                                        p.SubType.Equals(subType, StringComparison.InvariantCultureIgnoreCase)).ToList();

                            foreach (var pattern in subTypedPatterns)
                            {
                                subTypeDetails.patternSubtypeItems.Add(
                                    new
                                    {
                                        patternPath = pattern.HtmlUrl,
                                        patternState = GetState(pattern),
                                        patternPartial = pattern.Partial,
                                        patternName = pattern.Name.StripOrdinals().ToDisplayCase()
                                    });
                            }

                            subTypeDetails.patternSubtypeItems.Add(
                                new
                                {
                                    patternPath = string.Format("{0}/index.html", subTypePath),
                                    patternPartial = string.Format("viewall-{0}-{1}", typeName, subTypeName),
                                    patternName = "View All"
                                });

                            typeDetails.patternTypeItems.Add(subTypeDetails);

                            if (!subTypePaths.ContainsKey(subTypeName))
                            {
                                subTypePaths.Add(subTypeName, subTypePath);
                            }
                        }
                    }

                    foreach (var pattern in typedPatterns)
                    {
                        var patternName = pattern.Name.StripOrdinals();

                        if (!typedPatternPaths.ContainsKey(patternName))
                        {
                            typedPatternPaths.Add(patternName, pattern.PathDash);
                        }

                        if (!subTypes.Any())
                        {
                            typeDetails.patternItems.Add(
                                new
                                {
                                    patternPath = pattern.HtmlUrl,
                                    patternState = GetState(pattern),
                                    patternPartial = pattern.Partial,
                                    patternName = pattern.Name.StripOrdinals().ToDisplayCase()
                                });
                        }
                    }

                    patternPaths.Add(typeName, typedPatternPaths);
                    viewAllPaths.Add(typeName, subTypePaths);
                    patternTypes.Add(typeDetails);
                }
            }

            var mediaQueries = GetMediaQueries();

            var serializer = new JavaScriptSerializer();

            _data = new ViewDataDictionary
            {
                {"cacheBuster", CacheBuster()},
                {"ishminimum", Setting("ishMinimum")},
                {"ishmaximum", Setting("ishMaximum")},
                {"qrcodegeneratoron", Setting("qrCodeGeneratorOn")},
                {"ipaddress", ipAddress},
                {"xiphostname", Setting("xipHostname")},
                {"autoreloadnav", Setting("autoReloadNav")},
                {"autoreloadport", Setting("autoReloadPort")},
                {"pagefollownav", Setting("pageFollowNav")},
                {"pagefollowport", Setting("pageFollowPort")},
                {"ishControlsHide", hiddenIshControls},
                {"cssEnabled", Setting("cssEnabled")},
                {"patternpaths", serializer.Serialize(patternPaths)},
                {"viewallpaths", serializer.Serialize(viewAllPaths)},
                {"mqs", mediaQueries},
                {"patternTypes", patternTypes}
            };

            var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(DataFolder));

            var dataFiles = root.GetFiles(string.Concat("*", DataExtension), SearchOption.AllDirectories);

            _data = AppendData(_data, dataFiles);
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

            var parentPatterns = _patterns.Where(p => p.PseudoPatterns.Any()).ToList();
            foreach (var pattern in parentPatterns)
            {
                _patterns.AddRange(pattern.PseudoPatterns.Select(p => new Pattern(pattern.FilePath, p)));
            }

            _patterns = _patterns.OrderBy(p => p.PathDash).ToList();

            return _patterns;
        }

        public string Setting(string settingName)
        {
            var value = Controllers.PatternLabController.Provider.Config().Global[settingName];

            if (settingName.Equals("cssEnabled", StringComparison.InvariantCultureIgnoreCase))
            {
                value = "false";
            }
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("\"", string.Empty);
            }

            return value;
        }

        public static ViewDataDictionary AppendData(ViewDataDictionary original, Dictionary<string, object> additional)
        {
            foreach (var item in additional)
            {
                if (original.ContainsKey(item.Key))
                {
                    original[item.Key] = item.Value;
                }
                else
                {
                    original.Add(item.Key, item.Value);
                }
            }

            return original;
        }

        public static ViewDataDictionary AppendData(ViewDataDictionary original, FileInfo dataFile)
        {
            return dataFile != null ? AppendData(original, new[] {dataFile}) : original;
        }

        public static ViewDataDictionary AppendData(ViewDataDictionary original, IEnumerable<FileInfo> dataFiles)
        {
            var serializer = new JavaScriptSerializer();

            foreach (var dataFile in dataFiles)
            {
                AppendData(original,
                    serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(dataFile.FullName)));
            }

            return original;
        }

        public static List<string> GetMediaQueries()
        {
            var mediaQueries = new List<string>();

            foreach (var filePath in Directory.GetFiles(HttpContext.Current.Server.MapPath("~/css"), "*.css").ToList())
            {
                var css = File.ReadAllText(filePath);
                var queries = mediaQueries;
                mediaQueries.AddRange(
                    Regex.Matches(css, @"(min|max)-width:([ ]+)?(([0-9]{1,5})(\.[0-9]{1,20}|)(px|em))")
                        .Cast<Match>()
                        .Select(match => match.Groups[3].Value)
                        .Where(mediaQuery => !queries.Contains(mediaQuery)));
            }

            mediaQueries =
                mediaQueries.OrderBy(m => double.Parse(m.Substring(0, m.LastIndexOfAny("0123456789".ToCharArray()) + 1)))
                    .ToList();

            return mediaQueries;
        }

        public static string GetState(Pattern pattern, string state = null)
        {
            var provider = Controllers.PatternLabController.Provider;
            var states = provider.Setting("patternStates")
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (state == null)
            {
                state = pattern.State;
            }

            if (string.IsNullOrEmpty(pattern.State))
                return
                    pattern.Lineages.Select(
                        partial =>
                            provider.Patterns()
                                .FirstOrDefault(
                                    p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                        .Aggregate(state, (current, lineage) => GetState(lineage, current));
            var currentIndex = states.IndexOf(state);
            var newIndex = states.IndexOf(pattern.State);

            if ((newIndex < currentIndex || currentIndex < 0) && newIndex < states.Count - 1)
            {
                state = pattern.State;
            }

            return
                pattern.Lineages.Select(
                    partial =>
                        provider.Patterns()
                            .FirstOrDefault(p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                    .Aggregate(state, (current, lineage) => GetState(lineage, current));
        }
    }
}