using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Castle.MonoRail.Routing
{
    public class QueryStringConstraint : IRouteConstraint
    {
        public QueryStringConstraint(IDictionary<string, object> requiredParams)
        {
            RequiredParams = new Dictionary<string, object>(requiredParams, StringComparer.InvariantCultureIgnoreCase);
        }

        public IDictionary<string, object> RequiredParams { get; private set; }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName,
            RouteValueDictionary values, RouteDirection routeDirection)
        {
            IDictionary<string, object> relevantValues = values;
            if (routeDirection == RouteDirection.IncomingRequest)
                relevantValues = new Hash(httpContext.Request.QueryString, StringComparer.InvariantCultureIgnoreCase);

            return Matches(relevantValues);
        }

        private bool Matches(IDictionary<string, object> values)
        {
            return AllRequiredParametersPresent(values) && AllParameterValuesMatch(values);
        }

        private bool AllParameterValuesMatch(IDictionary<string, object> values)
        {
            var valid = true;
            foreach (var param in RequiredParams)
            {
                valid &= string.Equals(values[param.Key].ToString(), param.Value.ToString(),
                    StringComparison.InvariantCultureIgnoreCase);
            }
            return valid;
        }

        private bool AllRequiredParametersPresent(IDictionary<string, object> values)
        {
            var requiredKeys = new List<string>(RequiredParams.Keys);
            return requiredKeys.All(key => values.ContainsKey(key));
        }
    }
}