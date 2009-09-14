using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Routing;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class RouteCollectionExtensionsTest
    {
        [Test]
        public void MapResourceAddsIndexRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients", "GET",
                new { controller = "Patients", action = "Index" });
        }

        [Test]
        public void MapResourceAddsCreateRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients", "POST",
                new { controller = "Patients", action = "Create" });
        }

        [Test]
        public void MapResourceAddsNewRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients/New", "GET",
                new { controller = "Patients", action = "New" });
        }

        [Test]
        public void MapResourceAddsShowRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients/123", "GET",
                new { controller = "Patients", action = "Show", id = "123" });
        }

        [Test]
        public void MapResourceAddsTrueUpdateRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients/123", "PUT",
                new { controller = "Patients", action = "Update", id = "123" });
        }

        [Test]
        public void MapResourceAddsUpdateRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients/123?method=put", "POST",
                new { controller = "Patients", action = "Update", id = "123" });
        }

        [Test]
        public void MapResourceAddsDestroyRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients/123", "DELETE",
                new { controller = "Patients", action = "Destroy", id = "123" });
        }

        [Test]
        public void MapResourceAddsLegacyDestroyRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients/123?method=Delete", "POST",
                new { controller = "Patients", action = "Destroy", id = "123" });
        }

        [Test]
        public void MapResourceAddsEditRoute()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients/123/Edit", "GET",
                new { controller = "Patients", action = "Edit", id = "123" });
        }

        [Test]
        public void MapResourceAddsNestedIndex()
        {
            var routes = new RouteCollection();
            routes.MapResource("labs", "patients/{mrn}");
            AssertRoute(routes, "~/Patients/123/Labs", "GET",
                new { controller = "Labs", action = "Index", mrn = "123" });
        }

        [Test]
        public void MapRouteWithExplicitControllerAndAction()
        {
            var routes = new RouteCollection();
            routes.Map("{controller}/{action}");
            AssertRoute(routes, "~/Patients/Index", "GET",
                new { controller = "Patients", action = "Index" });
        }

        [Test]
        public void MapRouteWithImplicitAction()
        {
            var routes = new RouteCollection();
            routes.Map("{controller}", new { action = "Index" });
            AssertRoute(routes, "~/Patients", "GET",
                new { controller = "Patients", action = "Index" });
        }

        [Test]
        public void MapRouteWithExtraParameters()
        {
            var routes = new RouteCollection();
            routes.Map("Patients/{mrn}/Labs/{id}", new { controller = "Labs", action = "Show" });
            AssertRoute(routes, "~/Patients/123/Labs/abc", "GET",
                new { controller = "Labs", action = "Show", mrn = "123", id = "abc" });
        }

        [Test]
        public void MapRouteWithHttpVerb()
        {
            var routes = new RouteCollection();
            routes.Map("{controller}/{action}", new { }, new { httpMethod = new HttpMethodConstraint("GET") });
            AssertRoute(routes, "~/Patients/Index", "GET",
                new { controller = "Patients", action = "Index" });
        }

        [Test]
        public void MapRouteWithExplicitHttpVerb_DoesntMatchWrongVerb()
        {
            var routes = new RouteCollection();
            routes.Map("{controller}/{action}", new { }, new { httpMethod = new HttpMethodConstraint("GET") });
            Assert.IsNull(GetRoute(routes, "~/Patients/Index", "POST"));
        }

        [Test]
        public void MapRouteWithHttpVerbAsString()
        {
            var routes = new RouteCollection();
            routes.Map("{controller}/{action}", new { }, "GET");
            AssertRoute(routes, "~/Patients/Index", "GET",
                new { controller = "Patients", action = "Index" });
        }

        [Test]
        public void MapRouteWithHttpVerbAsString_DoesntMatchWrongVerb()
        {
            var routes = new RouteCollection();
            routes.Map("{controller}/{action}", new { }, "GET");
            Assert.IsNull(GetRoute(routes, "~/Patients/Index", "POST"));
        }

        [Test]
        public void MappingRoot()
        {
            var routes = new RouteCollection();
            routes.MapRoot("Patients", "Index");
            Assert.AreEqual("Patients", routes.Root().Values["controller"]);
            Assert.AreEqual("Index", routes.Root().Values["action"]);
        }

        [Test]
        public void ResourcesAllowOptionalExtension()
        {
            var routes = new RouteCollection();
            routes.MapResource("patients");
            AssertRoute(routes, "~/Patients.xml", "GET",
                new { controller = "Patients", action = "Index", format = "xml" });
        }

        /// <summary>
        /// Adapted from http://haacked.com/archive/2007/12/17/testing-routes-in-asp.net-mvc.aspx
        /// </summary>
        public void AssertRoute(RouteCollection routes, string url, string httpMethod, object expectations)
        {
            var routeData = GetRoute(routes, url, httpMethod);
            Assert.IsNotNull(routeData, "Did not find the route");

            var hash = new Hash(expectations);
            foreach (var pair in hash)
            {
                Assert.IsTrue(routeData.Values.ContainsKey(pair.Key),
                    string.Format("Missing route value: {0}", pair.Key));
                Assert.IsTrue(string.Equals(pair.Value.ToString(),
                    routeData.Values[pair.Key].ToString(),
                    StringComparison.OrdinalIgnoreCase),
                    string.Format("Expected '{0}', not '{1}' for '{2}'.",
                        pair.Value, routeData.Values[pair.Key], pair.Key));
            }
        }

        private RouteData GetRoute(RouteCollection routes, string url, string httpMethod)
        {
            var matches = Regex.Match(url, @"([^\?]+)(\?(.*))?");
            var stem = matches.Groups[1].Value;
            var queryString = GetQueryString(matches.Groups[3].Value);

            var mockRequest = MockRepository.GenerateStub<HttpRequestBase>();
            mockRequest.Stub(r => r.AppRelativeCurrentExecutionFilePath).Return(stem).Repeat.Any();
            mockRequest.Stub(r => r.QueryString).Return(queryString).Repeat.Any();
            mockRequest.Stub(r => r.HttpMethod).Return(httpMethod).Repeat.Any();

            var mockContext = MockRepository.GenerateStub<HttpContextBase>();
            mockContext.Stub(c => c.Request).Return(mockRequest).Repeat.Any();

            return routes.GetRouteData(mockContext);
        }

        private NameValueCollection GetQueryString(string queryStringText)
        {
            var queryString = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            if (string.IsNullOrEmpty(queryStringText))
                return queryString;

            foreach (var keyValuePair in queryStringText.Split('&'))
            {
                var keyValue = keyValuePair.Split('=');
                queryString.Add(keyValue[0], keyValue[1]);
            }
            return queryString;
        }
    }
}