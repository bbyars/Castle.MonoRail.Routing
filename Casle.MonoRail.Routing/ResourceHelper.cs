using System;
using System.Collections;
using System.Collections.Generic;
using Castle.MonoRail.Framework.Helpers;

namespace Castle.MonoRail.Routing
{
    /// <summary>
    /// Builds URLs based on route names and/or routing parameters.
    /// </summary>
    public class ResourceHelper : AbstractHelper
    {
        private readonly RoutingBasedUrlBuilder urlBuilder;

        public ResourceHelper() : this(new RoutingBasedUrlBuilder())
        {
        }

        public ResourceHelper(RoutingBasedUrlBuilder urlBuilder)
        {
            this.urlBuilder = urlBuilder;
        }

        public virtual string Url(IDictionary parameters)
        {
            return Url(null, parameters);
        }

        public virtual string Url(string routeName)
        {
            return Url(routeName, new Hashtable());
        }

        public virtual string Url(string routeName, IDictionary parameters)
        {
            var routeParameters = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var key in parameters.Keys)
            {
                routeParameters.Add(key.ToString(), parameters[key]);
            }

            // Hacks because apparently the QueryStringConstraint isn't used for building urls
            if (IsUpdate(routeName))
                routeParameters.Add("method", "put");
            if (IsDestroy(routeName))
                routeParameters.Add("method", "delete");

            // Hack because the current version of System.Web.Routing doesn't support an
            // optional extension with just one route
            if (RequiresExtendedName(routeParameters, routeName))
                routeName += "Extended";

            return urlBuilder.BuildUrlByName(routeName, routeParameters);
        }

        private bool RequiresExtendedName(IDictionary<string, object> routeParameters, string routeName)
        {
            return routeParameters.ContainsKey("format")
                && routeName != null
                && !routeName.EndsWith("Extended", StringComparison.CurrentCultureIgnoreCase);
        }

        private bool IsUpdate(string routeName)
        {
            return IsTunnelledThroughQueryString(routeName, "Update");
        }

        private bool IsDestroy(string routeName)
        {
            return IsTunnelledThroughQueryString(routeName, "Destroy");
        }

        private bool IsTunnelledThroughQueryString(string routeName, string method)
        {
            return !string.IsNullOrEmpty(routeName)
                && routeName.EndsWith(method)
                    && !routeName.EndsWith("True" + method);
        }
    }
}