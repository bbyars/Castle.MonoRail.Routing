using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Framework.Services;

namespace Castle.MonoRail.Routing
{
    public class RoutingBasedUrlBuilder : DefaultUrlBuilder
    {
        private readonly RouteCollection routes;
        private HttpContextBase httpContext;

        public RoutingBasedUrlBuilder() : this(RouteTable.Routes)
        {
        }

        public RoutingBasedUrlBuilder(RouteCollection routes)
        {
            this.routes = routes;
        }

        public virtual HttpContextBase Context
        {
            get { return httpContext ?? new HttpContextWrapper(HttpContext.Current); }
            set { httpContext = value; }
        }

        public virtual string BuildUrlByName(string routeName, IDictionary<string, object> parameters)
        {
            var routeValues = new RouteValueDictionary(parameters);
            return GetVirtualPath(routeName, routeValues);
        }

        protected override string InternalBuildUrl(string area, string controller, string action, string protocol,
            string port, string domain, string subdomain, string appVirtualDir, string extension,
            bool absolutePath, bool applySubdomain, string suffix, string basePath)
        {
            var routeValues = GetRouteValues(controller, action, suffix);
            return GetVirtualPath(null, routeValues);
        }

        private RouteValueDictionary GetRouteValues(string controller, string action, string querystring)
        {
            var routeValues = new RouteValueDictionary { { "controller", controller }, { "action", action } };

            if (!string.IsNullOrEmpty(querystring))
            {
                foreach (var pair in querystring.Trim('&').Split('&'))
                {
                    var keyValue = pair.Split('=');
                    routeValues.Add(ServerUtil.UrlDecode(keyValue[0]), ServerUtil.UrlDecode(keyValue[1]));
                }
            }
            return routeValues;
        }

        private string GetVirtualPath(string routeName, RouteValueDictionary routeValues)
        {
            var requestContext = new RequestContext(Context, new RouteData());
            var virtualPathData = routes.GetVirtualPath(requestContext, routeName, routeValues);

            if (virtualPathData == null)
                throw new RouteException(string.Format("No route could be found matching the given parameters.{0}Route Name: {1}{0}Url: {2}",
                    Environment.NewLine, routeName, Context.Request.Url));

            return virtualPathData.VirtualPath;
        }
    }
}