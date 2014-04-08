using System.Web;
using System.Web.Routing;

namespace PatternLab.Core.Handlers
{
    class EmbeddedResourceRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new EmbeddedResourceHttpHandler(requestContext.RouteData);
        }
    }
}