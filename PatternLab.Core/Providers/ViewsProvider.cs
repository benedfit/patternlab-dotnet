using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PatternLab.Core.Models;

namespace PatternLab.Core.Providers
{
    public interface IViewsProvider
    {
        List<View> Views();
    }

    public class ViewsProvider : IViewsProvider
    {
        public static string ExtensionSearchPattern = "*.cshtml";
        public static string FolderPath = "~/Views/";

        private List<View> _views;

        public List<View> Views()
        {
            if (_views != null) return _views;

            var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(FolderPath));

            var views =
                root.GetFiles(ExtensionSearchPattern, SearchOption.AllDirectories)
                    .Where(v => v.Directory != null && v.Directory.FullName != root.FullName && !v.Name.StartsWith("_"));

            _views = views.Select(view => new View
            {
                FilePath = view.FullName
            }).ToList();

            return _views;
        }
    }
}