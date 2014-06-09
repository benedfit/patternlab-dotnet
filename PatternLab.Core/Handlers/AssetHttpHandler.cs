using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Handlers
{
    /// <summary>
    /// The HTTP handler for requests to assets
    /// </summary>
    public class AssetHttpHandler : IHttpHandler
    {
        private readonly RouteData _routeData;

        /// <summary>
        /// Initialises a new HTTP handler for requests to assets
        /// </summary>
        /// <param name="routeData">The route data</param>
        public AssetHttpHandler(RouteData routeData)
        {
            _routeData = routeData;
        }

        /// <summary>
        /// HTTP handler is not resusable
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Processes the request for an asset
        /// </summary>
        /// <param name="context">The current HTTP context</param>
        public void ProcessRequest(HttpContext context)
        {
            var routeDataValues = _routeData.Values;

            // Get the folder path from the route data
            var folder = routeDataValues["root"].ToString();
            if (PatternProvider.FolderNameAnnotations.EndsWith(folder, StringComparison.InvariantCultureIgnoreCase) ||
                PatternProvider.FolderNameData.EndsWith(folder, StringComparison.InvariantCultureIgnoreCase))
            {
                // Prepend underscore to handle _data and _annotations folders
                folder = string.Concat(PatternProvider.IdentifierHidden, folder);
            }

            // Get the file name from the route data
            var filePath = routeDataValues["path"] != null ? routeDataValues["path"].ToString() : string.Empty;
            var fileName = Path.GetFileName(filePath);
            var virtualPath = string.Format("/{0}/{1}", folder, filePath);

            // Open and read the file
            using (var stream = VirtualPathProvider.OpenFile(virtualPath))
            {
                if (stream.Length <= 0) return;

                context.Response.Clear();
                // Get the mime type from the file name
                context.Response.ContentType = MimeMapping.GetMimeMapping(fileName);

                stream.CopyTo(context.Response.OutputStream);
            }
        }
    }
}