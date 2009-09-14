using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Routing;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class QueryStringConstraintIncomingTest
    {
        [Test]
        public void MatchesWhenQueryStringPresent()
        {
            Assert.IsTrue(ConstraintMatches(new { action = "put" }, new { action = "put" }));
        }

        [Test]
        public void DoesNotMatchWhenQueryString()
        {
            Assert.IsFalse(ConstraintMatches(new { action = "put" }, new { }));
        }

        [Test]
        public void MatchesCaseInsensitive()
        {
            Assert.IsTrue(ConstraintMatches(new { action = "put" }, new { Action = "PUT" }));
        }

        [Test]
        public void DoesNotMatchIfParameterHasWrongValue()
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

        private bool ConstraintMatches(object constraintQueryString, object actualQueryString)
        {
            var mockContext = GetContext(new Hash(actualQueryString).ToNameValueCollection());
            var constraint = new QueryStringConstraint(new Hash(constraintQueryString));
            var route = GetRoute(constraint);
            return constraint.Match(mockContext, route, "", new RouteValueDictionary(), RouteDirection.IncomingRequest);
        }

        private HttpContextBase GetContext(NameValueCollection queryString)
        {
            var mockRequest = MockRepository.GenerateStub<HttpRequestBase>();
            mockRequest.Stub(r => r.QueryString).Return(queryString).Repeat.Any();

            var mockContext = MockRepository.GenerateStub<HttpContextBase>();
            mockContext.Stub(c => c.Request).Return(mockRequest).Repeat.Any();
            return mockContext;
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