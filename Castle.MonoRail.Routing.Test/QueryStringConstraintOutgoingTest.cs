using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Routing;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class QueryStringConstraintOutgoingTest
    {
        [Test]
        public void DoesNotMatchIfParameterMissing()
        {
            Assert.IsFalse(ConstraintMatches(new { action = "put" }, new { }));
        }

        [Test]
        public void MatchesIfParameterPresent()
        {
            Assert.IsTrue(ConstraintMatches(new { action = "put" }, new { action = "put" }));
        }

        [Test]
        public void MatchesCaseInsensitive()
        {
            Assert.IsTrue(ConstraintMatches(new { action = "put" }, new { Action = "PUT" }));
        }

        [Test]
        public void DoesNotMatchIfParameterValueDifferent()
        {
            Assert.IsFalse(ConstraintMatches(new { action = "put" }, new { action = "delete" }));
        }

        [Test]
        public void DoesNotMatchUnlessAllParametersPresent()
        {
            Assert.IsFalse(ConstraintMatches(new { action = "put", foo = "bar" }, new { action = "delete" }));
        }

        [Test]
        public void MatchesAllParameters()
        {
            Assert.IsTrue(ConstraintMatches(new { action = "put", foo = "bar" }, new { action = "put", foo = "bar" }));
        }

        private bool ConstraintMatches(object constraintQueryString, object routeValues)
        {
            var mockContext = MockRepository.GenerateStub<HttpContextBase>();
            var constraint = new QueryStringConstraint(new Hash(constraintQueryString));
            var route = GetRoute(constraint);
            var values = new RouteValueDictionary(routeValues);
            return constraint.Match(mockContext, route, "", values, RouteDirection.UrlGeneration);
        }

        private Route GetRoute(QueryStringConstraint constraint)
        {
            return new Route("Patients/{id}",
                new RouteValueDictionary(new { action = "Update" }),
                new RouteValueDictionary(new { queryString = constraint }),
                new MonorailRouteHandler());
        }
    }
}