using System;
using System.Collections;
using System.Web.Routing;
using Castle.MonoRail.Routing;
using NUnit.Framework;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class ResourceHelperTest
    {
        private ResourceHelper GetHelper(Action<RouteCollection> routeCommand)
        {
            var urlBuilder = TestFactory.CreateUrlBuilder(routeCommand);
            return new ResourceHelper(urlBuilder);
        }

        [Test]
        public void BuildsIndexByName()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients", helper.Url("PatientsIndex"));
        }

        [Test]
        public void BuildsIndexByControllerAndAction()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients", helper.Url(
                new Hashtable { { "controller", "Patients" }, { "action", "Index" } }));
        }

        [Test]
        public void BuildsNestedIndex()
        {
            var helper = GetHelper(routes => routes.MapResource("Labs", "Patients/{mrn}"));
            Assert.AreEqual("/Patients/123/Labs", helper.Url("LabsIndex", new Hashtable { { "mrn", 123 } }));
        }

        [Test]
        public void BuildsCreate()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients", helper.Url("PatientsCreate"));
        }

        [Test]
        public void BuildsShow()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients/123", helper.Url("PatientsShow", new Hashtable { { "id", 123 } }));
        }

        [Test]
        public void BuildsNew()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients/New", helper.Url("PatientsNew"));
        }

        [Test]
        public void BuildsUpdate()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients/123?method=put",
                helper.Url("PatientsUpdate", new Hashtable { { "id", 123 } }));
        }

        [Test]
        public void BuildsTrueUpdate()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients/123", helper.Url("PatientsTrueUpdate", new Hashtable { { "id", 123 } }));
        }

        [Test]
        public void BuildsDestroy()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients/123?method=delete",
                helper.Url("PatientsDestroy", new Hashtable { { "id", 123 } }));
        }

        [Test]
        public void BuildsTrueDestroy()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients/123",
                helper.Url("PatientsTrueDestroy", new Hashtable { { "id", 123 } }));
        }

        [Test]
        public void BuildsEdit()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients/123/Edit",
                helper.Url("PatientsEdit", new Hashtable { { "id", 123 } }));
        }

        [Test]
        [ExpectedException(typeof(RouteException))]
        public void InvalidParametersThrowsRoutingException()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            helper.Url(new Hashtable { { "controller", "Invalid" }, { "action", "Invalid" } });
        }

        [Test]
        public void BuildsWithFormat()
        {
            var helper = GetHelper(routes => routes.MapResource("Patients"));
            Assert.AreEqual("/Patients.xml", helper.Url("PatientsIndex", new Hashtable { { "format", "xml" } }));
        }
    }
}