using System.Collections;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Routing;
using NUnit.Framework;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class RoutingBasedUrlBuilderTest
    {
        private UrlInfo GetUrlInfo()
        {
            return new UrlInfo("test.com", "www", "", "http", 80, "http://www.test.com/Default",
                "", "Default", "Default", "");
        }

        [Test]
        public void BuildUrlWithExplicitControllerAndAction()
        {
            var urlBuilder = TestFactory.CreateUrlBuilder(routes => RouteCollectionExtensions.Map(routes, "{controller}/{action}"));
            Assert.AreEqual("/Patients/Index", urlBuilder.BuildUrl(GetUrlInfo(), "Patients", "Index"));
        }

        [Test]
        public void BuildUrlWithImplicitAction()
        {
            var urlBuilder = TestFactory.CreateUrlBuilder(
                routes => routes.Map("{controller}", new { action = "Index" }));
            Assert.AreEqual("/Patients", urlBuilder.BuildUrl(GetUrlInfo(), "Patients", "Index"));
        }

        [Test]
        public void BuildUrlWithParameters()
        {
            var urlBuilder = TestFactory.CreateUrlBuilder(
                routes => routes.Map("{controller}/{id}", new { action = "Show" }));
            var parameters = new Hashtable { { "id", 123 } };
            Assert.AreEqual("/Patients/123", urlBuilder.BuildUrl(GetUrlInfo(), "Patients", "Show", parameters));
        }

        [Test]
        public void BuildUrlWithHttpVerbConstraint()
        {
            var urlBuilder = TestFactory.CreateUrlBuilder(
                routes => routes.Map("{controller}", new { action = "Create" }, "POST"));
            Assert.AreEqual("/Patients", urlBuilder.BuildUrl(GetUrlInfo(), "Patients", "Create"));
        }

        [Test]
        public void BuildNestedResourceUrl()
        {
            var urlBuilder = TestFactory.CreateUrlBuilder(
                routes => routes.Map("Patients/{mrn}/Labs/{id}", new { action = "Show", controller = "Labs" }));
            var parameters = new Hashtable { { "mrn", "123" }, { "id", "abc" } };
            Assert.AreEqual("/Patients/123/Labs/abc", urlBuilder.BuildUrl(GetUrlInfo(), "Labs", "Show", parameters));
        }

        [Test]
        public void BuildUrlByRouteName()
        {
            var urlBuilder = TestFactory.CreateUrlBuilder(
                routes => routes.Map("PatientsShow", "Patients/{id}",
                              new { action = "Show", controller = "Patients" }, new { }));
            var parameters = new Hash { { "id", "123" } };
            Assert.AreEqual("/Patients/123", urlBuilder.BuildUrlByName("PatientsShow", parameters));
        }
    }
}