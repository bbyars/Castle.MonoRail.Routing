using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Routing
{
    /// <summary>
    /// Overrides Monorail's default tokenizer, since we're using System.Web.Routing
    /// to figure out the controller and action for us.
    /// </summary>
    public class RoutingBasedUrlTokenizer : IUrlTokenizer
    {
        private readonly RouteCollection routes;
        private HttpContextBase httpContext;

        public RoutingBasedUrlTokenizer() : this(RouteTable.Routes)
        {
        }

        public RoutingBasedUrlTokenizer(RouteCollection routes)
        {
            this.routes = routes;
        }

        public virtual HttpContextBase Context
        {
            get { return httpContext ?? new HttpContextWrapper(HttpContext.Current); }
            set { httpContext = value; }
        }

        public virtual UrlInfo TokenizeUrl(string rawUrl, Uri uri, bool isLocal, string appVirtualDir)
        {
            if (rawUrl.TrimEnd('/').ToLower() == appVirtualDir.ToLower())
            {
                var root = routes.Root();
                return new UrlInfo(uri.Host, uri.Host, appVirtualDir, uri.Scheme, uri.Port, rawUrl,
                    "", root.GetRequiredString("controller"), root.GetRequiredString("action"), "");
            }

            var routeData = routes.GetRouteData(Context);

            if (routeData == null)
                throw new HttpException(404, "The resource cannot be found");

            var values = routeData.Values;
            var controller = routeData.GetRequiredString("controller");
            var action = routeData.GetRequiredString("action");

            RewritePath(values, uri.Query);

            return new UrlInfo(uri.Host, uri.Host, appVirtualDir,
                uri.Scheme, uri.Port, rawUrl, "", controller, action, "");
        }

        private void RewritePath(RouteValueDictionary values, string oldQuerystring)
        {
            var newUrl = GetRewrittenUrl(values, oldQuerystring);
            Context.RewritePath(newUrl);
        }

        private string GetRewrittenUrl(RouteValueDictionary values, string oldQuerystring)
        {
            var newUrl = string.Format("{0}/{1}", values["controller"], values["action"]);
            newUrl += string.IsNullOrEmpty(oldQuerystring) ? "?" : oldQuerystring;

            var specialParams = new List<string> { "controller", "action" };
            foreach (var key in values.Keys)
            {
                var name = key;
                if (specialParams.Exists(param => param.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                if (!newUrl.EndsWith("?"))
                    newUrl += "&";
                newUrl += string.Format("{0}={1}", HttpUtility.UrlEncode(name),
                    HttpUtility.UrlEncode(values[name].ToString()));
            }
            return newUrl.TrimEnd('?', '&');
        }
    }
}