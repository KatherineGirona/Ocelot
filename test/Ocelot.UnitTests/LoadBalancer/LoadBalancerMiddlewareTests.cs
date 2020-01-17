using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ocelot.Configuration.Builder;
using Ocelot.DownstreamRouteFinder;
using Ocelot.Infrastructure.RequestData;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.LoadBalancer.Middleware;
using Ocelot.Logging;
using Ocelot.Responses;
using Ocelot.Values;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    public class LoadBalancerMiddlewareTests
    {
        private readonly Mock<ILoadBalancerHouse> _loadBalancerHouse;
        private readonly Mock<IRequestScopedDataRepository> _scopedRepository;
        private readonly Mock<ILoadBalancer> _loadBalancer;
        private readonly string _url;
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private HttpResponseMessage _result;
        private OkResponse<Ocelot.Request.Request> _request;
        private OkResponse<string> _downstreamUrl;
        private OkResponse<DownstreamRoute> _downstreamRoute;

        public LoadBalancerMiddlewareTests()
        {
            _url = "http://localhost:51879";
            _loadBalancerHouse = new Mock<ILoadBalancerHouse>();
            _scopedRepository = new Mock<IRequestScopedDataRepository>();
            var builder = new WebHostBuilder()
              .ConfigureServices(x =>
              {
                  x.AddSingleton<IOcelotLoggerFactory, AspDotNetLoggerFactory>();
                  x.AddLogging();
                  x.AddSingleton(_loadBalancerHouse.Object);
                  x.AddSingleton(_scopedRepository.Object);
              })
              .UseUrls(_url)
              .UseKestrel()
              .UseContentRoot(Directory.GetCurrentDirectory())
              .UseIISIntegration()
              .UseUrls(_url)
              .Configure(app =>
              {
                  app.UseLoadBalancingMiddlewareExtensions();
              });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public void should_call_scoped_data_repository_correctly()
        {
            var downstreamRoute = new DownstreamRoute(new List<Ocelot.DownstreamRouteFinder.UrlMatcher.UrlPathPlaceholderNameAndValue>(),
                new ReRouteBuilder()
                    .Build());

            this.Given(x => x.GivenTheDownStreamUrlIs("any old string"))
                .And(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => x.GivenTheLoadBalancerHouseReturns())
                .And(x => x.GivenTheLoadBalancerReturns())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheScopedDataRepositoryIsCalledCorrectly())
                .BDDfy();
        }

        private void GivenTheLoadBalancerReturns()
        {
            _loadBalancer
                .Setup(x => x.Lease())
                .Returns(new OkResponse<HostAndPort>(new HostAndPort("127.0.0.1", 80)));
        }

        private void GivenTheDownStreamRouteIs(DownstreamRoute downstreamRoute)
        {
            _downstreamRoute = new OkResponse<DownstreamRoute>(downstreamRoute);
            _scopedRepository
                .Setup(x => x.Get<DownstreamRoute>(It.IsAny<string>()))
                .Returns(_downstreamRoute);
        }

        private void GivenTheLoadBalancerHouseReturns()
        {
            _loadBalancerHouse
                .Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new OkResponse<ILoadBalancer>(_loadBalancer.Object));
        }

        private void ThenTheScopedDataRepositoryIsCalledCorrectly()
        {
            _scopedRepository
                .Verify(x => x.Add("Request", _request.Data), Times.Once());
        }

        private void WhenICallTheMiddleware()
        {
            _result = _client.GetAsync(_url).Result;
        }

        private void GivenTheDownStreamUrlIs(string downstreamUrl)
        {
            _downstreamUrl = new OkResponse<string>(downstreamUrl);
            _scopedRepository
                .Setup(x => x.Get<string>(It.IsAny<string>()))
                .Returns(_downstreamUrl);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}