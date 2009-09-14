using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Routing
{
    /// <summary>
    /// Delegates to the MonoRail handler, but using System.Web.Routing
    /// </summary>
    public class MonorailRouteHandler : MonoRailHttpHandlerFactory, IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var request = HttpContext.Current.Request;
            return GetHandler(
                HttpContext.Current,
                request.RequestType,
                request.RawUrl,
                request.PhysicalApplicationPath);
        }
    }
}