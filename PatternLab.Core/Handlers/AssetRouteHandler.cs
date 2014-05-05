using System.Web;
using System.Web.Routing;

namespace PatternLab.Core.Handlers
{
    /// <summary>
    /// The route handler for requests to assets
    /// </summary>
    public class AssetRouteHandler : IRouteHandler
    {
        /// <summary>
        /// Gets the HTTP handler for requests to assets
        /// </summary>
        /// <param name="requestContext">The current RequestContext</param>
        /// <returns>A HTTP handler for requesting an asset</returns>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new AssetHttpHandler(requestContext.RouteData);
        }
    }
}
