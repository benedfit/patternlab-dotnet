using System.Web;
using System.Web.Routing;

namespace PatternLab.Core.Handlers
{
    public class EmbeddedResourceRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new EmbeddedResourceHttpHandler(requestContext.RouteData);
        }
    }
}