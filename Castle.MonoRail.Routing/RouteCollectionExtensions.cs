using System.Web.Routing;

namespace Castle.MonoRail.Routing
{
    public static class RouteCollectionExtensions
    {
        private static RouteData root;

        public static void MapRoot(this RouteCollection routes, string controller, string action)
        {
            root = new RouteData();
            root.Values["controller"] = controller;
            root.Values["action"] = action;
        }

        public static RouteData Root(this RouteCollection routes)
        {
            return root;
        }

        public static void Map(this RouteCollection routes, string url)
        {
            routes.Map(url, new { });
        }

        public static void Map(this RouteCollection routes, string url, object defaults)
        {
            routes.Map(url, defaults, new { });
        }

        public static void Map(this RouteCollection routes, string url, object defaults, string httpVerb)
        {
            routes.Map(url, defaults, new { httpMethod = new HttpMethodConstraint(httpVerb) });
        }

        public static void Map(this RouteCollection routes, string url, object defaults, object constraints)
        {
            routes.Map(null, url, defaults, constraints);
        }

        public static void Map(this RouteCollection routes, string name, string url, object defaults, object constraints)
        {
            routes.Add(name, new Route(url, new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints), new MonorailRouteHandler()));
        }

        public static void MapResource(this RouteCollection routes, string resourceName)
        {
            routes.MapResource(resourceName, "");
        }

        /// <summary>
        /// Maps the standard RESTful routes for the following resources.  For
        /// example, for a resource named "patients", the following resources
        /// are created:
        /// * A list of patients (/patients)
        /// * A patient (/patients/123)
        /// * A patient edit form (/patients/123/edit)
        /// * A patients creation form (/patients/123/new)
        /// <para>
        /// HTTP verbs are leveraged to map those resources to the following actions
        /// on the PatientsController:
        /// * Index (GET /patients)
        /// * Create (POST /patients)
        /// * Show (GET /patients/123)
        /// * Update (PUT /patients/123, or POST /patients/123?method=put)
        /// * Destroy (DELETE /patients/123, or POST /patients/123?method=delete)
        /// * Edit (GET /patients/123/edit)
        /// * New (GET /patients/123/new)
        /// </para>
        /// <para>
        /// For nested resources (for example, labs which are tied to a patient), set
        /// the resourcePrefix to the parent resource(s).  For example, a resourcePrefix
        /// of "patients/{mrn}" and a resourceName of "labs" will create the following
        /// URL for a list of labs: /patients/123/labs (with the mrn parameter set to 123)
        /// </para>
        /// </summary>
        /// <example>
        /// var routes = new RouteCollection();
        /// routes.MapResource("patients");
        /// routes.MapResources("labs", "patients/{mrn}");
        /// </example>
        /// <param name="routes">The RouteCollection to add the routes to.</param>
        /// <param name="resourceName">The pluralized name of the resource, which will map
        /// to the controller.  For example, "patients" will map to PatientsController</param>
        /// <param name="resourcePrefix">The parent resource URL route string.  For example,
        /// "patients/{mrn}" will make patients the parent resource, with the mrn parameter
        /// automatically set</param>
        public static void MapResource(this RouteCollection routes, string resourceName, string resourcePrefix)
        {
            if (string.IsNullOrEmpty(resourcePrefix))
                resourcePrefix = "";
            else if (!resourcePrefix.EndsWith("/"))
                resourcePrefix += "/";

            new ResourceMapper(routes, resourceName, resourcePrefix).AddRoutes();
        }

        private class ResourceMapper
        {
            private readonly RouteCollection routes;
            private readonly string resourceName;
            private readonly string resourcePrefix;

            public ResourceMapper(RouteCollection routes, string resourceName, string resourcePrefix)
            {
                this.routes = routes;
                this.resourceName = resourceName;
                this.resourcePrefix = resourcePrefix;
            }

            public void AddRoutes()
            {
                Map("Index", "", "GET");
                Map("Create", "", "POST");
                Map("New", "/New", "GET");
                Map("Show", "/{id}", "GET");
                Map("Update", "/{id}", "POST", "Update", new { method = "put" });
                Map("TrueUpdate", "/{id}", "PUT", "Update");
                Map("Destroy", "/{id}", "POST", "Destroy", new { method = "delete" });
                Map("TrueDestroy", "/{id}", "DELETE", "Destroy");
                Map("Edit", "/{id}/Edit", "GET");
            }

            private void Map(string name, string url, string httpVerb)
            {
                Map(name, url, httpVerb, name);
            }

            private void Map(string name, string urlAfterControllerName, string httpVerb, string actionName)
            {
                Map(name, urlAfterControllerName, httpVerb, actionName, null);
            }

            private void Map(string name, string urlAfterControllerName, string httpVerb, string actionName,
                object requiredQueryStringParams)
            {
                var baseUrl = resourcePrefix + resourceName + urlAfterControllerName;

                // System.Web.Routing doesn't currently support doing this in one route
                routes.Add(resourceName + name, CreateResourceRoute(baseUrl, actionName, httpVerb, requiredQueryStringParams));
                routes.Add(resourceName + name + "Extended",
                    CreateResourceRoute(baseUrl + ".{format}", actionName, httpVerb, requiredQueryStringParams));
            }

            private Route CreateResourceRoute(string url, string actionName, string httpVerb, object requiredQueryStringParams)
            {
                var route = new Route(url,
                    new RouteValueDictionary(new { action = actionName, controller = resourceName }),
                    new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint(httpVerb) }),
                    new MonorailRouteHandler());

                if (requiredQueryStringParams != null)
                    route.Constraints.Add("queryString", new QueryStringConstraint(new Hash(requiredQueryStringParams)));

                return route;
            }
        }
    }
}