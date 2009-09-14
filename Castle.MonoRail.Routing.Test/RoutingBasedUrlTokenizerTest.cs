using System;
using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Routing;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class RoutingBasedUrlTokenizerTest
    {
        [Test]
        public void UrlMatchingAppDir_ReturnsRootRoute()
        {
            var routes = new RouteCollection();
            routes.MapRoot("Patients", "Index");

            var tokenizer = new RoutingBasedUrlTokenizer(routes);
            tokenizer.Context = TestFactory.CreateMockContext("~/");
            var urlInfo = tokenizer.TokenizeUrl("/vdir/", new Uri("http://localhost/vdir/"), true, "/vdir");

            Assert.AreEqual("Patients", urlInfo.Controller);
            Assert.AreEqual("Index", urlInfo.Action);
        }

        [Test]
        public void RootRouteMatchIsCaseInsensitive()
        {
            var routes = new RouteCollection();
            routes.MapRoot("Patients", "Index");

            var tokenizer = new RoutingBasedUrlTokenizer(routes);
            tokenizer.Context = TestFactory.CreateMockContext("~/");
            var urlInfo = tokenizer.TokenizeUrl("/VDIR", new Uri("http://localhost/VDIR"), true, "/vdir");

            Assert.AreEqual("Patients", urlInfo.Controller);
            Assert.AreEqual("Index", urlInfo.Action);
        }

        [Test]
        public void InvalidUrlThrows404()
        {
            var routes = new RouteCollection();
            routes.MapRoot("Patients", "Index");

            var tokenizer = new RoutingBasedUrlTokenizer(routes);
            tokenizer.Context = TestFactory.CreateMockContext("~/Login");

            try
            {
                tokenizer.TokenizeUrl("/Login", new Uri("http://localhost/Login"), true, "/");
                Assert.Fail("Should throw 404");
            }
            catch (HttpException ex)
            {
                Assert.AreEqual(404, ex.GetHttpCode());
            }
        }

        [Test]
        public void ReturnsMatchingRouteValues()
        {
            var routes = new RouteCollection();
            routes.Map("Patients/{id}", new { controller = "Patients", action = "Show" });

            var tokenizer = new RoutingBasedUrlTokenizer(routes);
            tokenizer.Context = TestFactory.CreateMockContext("~/Patients/123");
            var urlInfo = tokenizer.TokenizeUrl("/Patients/123", new Uri("http://localhost/Patients/123"), true, "/");

            Assert.AreEqual("Patients", urlInfo.Controller);
            Assert.AreEqual("Show", urlInfo.Action);
        }

        [Test]
        public void SimpleRewrite()
        {
            var routes = new RouteCollection();
            routes.Map("Patients", new { controller = "Patients", action = "Index" });

            var tokenizer = new RoutingBasedUrlTokenizer(routes);
            var mockContext = TestFactory.CreateMockContext("~/Patients");
            tokenizer.Context = mockContext;

            tokenizer.TokenizeUrl("/Patients/", new Uri("http://localhost/Patients/"), true, "/");
            mockContext.AssertWasCalled(context => context.RewritePath("Patients/Index"));
        }

        [Test]
        public void RewriteWithRoutingParameters()
        {
            var routes = new RouteCollection();
            routes.Map("Patients/{id}", new { controller = "Patients", action = "Show" });

            var tokenizer = new RoutingBasedUrlTokenizer(routes);
            var mockContext = TestFactory.CreateMockContext("~/Patients/123");
            tokenizer.Context = mockContext;

            tokenizer.TokenizeUrl("/Patients/123", new Uri("http://localhost/Patients/123"), true, "/");
            mockContext.AssertWasCalled(context => context.RewritePath("Patients/Show?id=123"));
        }

        [Test]
        public void RewriteWithRoutingParametersAndQuerystring()
        {
            var routes = new RouteCollection();
            routes.Map("Patients/{id}", new { controller = "Patients", action = "Show" });

            var tokenizer = new RoutingBasedUrlTokenizer(routes);
            var mockContext = TestFactory.CreateMockContext("~/Patients/123");
            tokenizer.Context = mockContext;

            tokenizer.TokenizeUrl("/Patients/123", new Uri("http://localhost/Patients/123?debug=true"), true, "/");
            Assert.AreEqual("Patients/Show?debug=true&id=123",
                mockContext.GetArgumentsForCallsMadeOn(context => context.RewritePath(null))[0][0]);
        }
    }
}