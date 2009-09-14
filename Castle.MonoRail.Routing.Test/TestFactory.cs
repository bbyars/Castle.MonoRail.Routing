using System;
using System.Web;
using System.Web.Routing;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Routing;
using Rhino.Mocks;

namespace Castle.MonoRail.Routing.Test
{
    public static class TestFactory
    {
        public static RoutingBasedUrlBuilder CreateUrlBuilder(Action<RouteCollection> routeCommand)
        {
            var routes = new RouteCollection();
            routeCommand(routes);

            var urlBuilder = new RoutingBasedUrlBuilder(routes);
            urlBuilder.Context = CreateMockContext("~/");
            urlBuilder.ServerUtil = CreateMockServerUtility();

            return urlBuilder;
        }

        public static IServerUtility CreateMockServerUtility()
        {
            var mockServerUtility = MockRepository.GenerateStub<IServerUtility>();
            mockServerUtility.Stub(s => s.UrlEncode("")).IgnoreArguments()
                .Do((Func<string, string>)(content => HttpUtility.UrlEncode(content)))
                .Repeat.Any();
            mockServerUtility.Stub(s => s.UrlDecode("")).IgnoreArguments()
                .Do((Func<string, string>)(content => HttpUtility.UrlDecode(content)))
                .Repeat.Any();
            return mockServerUtility;
        }

        public static HttpContextBase CreateMockContext(string appPath)
        {
            var mockRequest = MockRepository.GenerateStub<HttpRequestBase>();
            mockRequest.Stub(r => r.AppRelativeCurrentExecutionFilePath).Return(appPath).Repeat.Any();
            mockRequest.Stub(r => r.PathInfo).Return("").Repeat.Any();

            var mockResponse = MockRepository.GenerateStub<HttpResponseBase>();
            mockResponse.Stub(r => r.ApplyAppPathModifier("")).IgnoreArguments()
                .Do((Func<string, string>)(virtualPath => virtualPath));

            var mockContext = MockRepository.GenerateStub<HttpContextBase>();
            mockContext.Stub(c => c.Request).Return(mockRequest).Repeat.Any();
            mockContext.Stub(c => c.Response).Return(mockResponse).Repeat.Any();
            return mockContext;
        }
    }
}