﻿namespace Ocelot.UnitTests.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Cache;
    using Ocelot.Cache.Middleware;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Logging;
    using Ocelot.Responses;
    using TestStack.BDDfy;
    using Xunit;

    public class OutputCacheMiddlewareTests : ServerHostedMiddlewareTest
    {
        private readonly Mock<IOcelotCache<HttpResponseMessage>> _cacheManager;
        private readonly Mock<IRequestScopedDataRepository> _scopedRepo;
        private HttpResponseMessage _response;

        public OutputCacheMiddlewareTests()
        {
            _cacheManager = new Mock<IOcelotCache<HttpResponseMessage>>();
            _scopedRepo = new Mock<IRequestScopedDataRepository>();

            _scopedRepo
                .Setup(sr => sr.Get<HttpRequestMessage>("DownstreamRequest"))
                .Returns(new OkResponse<HttpRequestMessage>(new HttpRequestMessage(HttpMethod.Get, "https://some.url/blah?abcd=123")));

            GivenTheTestServerIsConfigured();
        }

        [Fact]
        public void should_returned_cached_item_when_it_is_in_cache()
        {
            this.Given(x => x.GivenThereIsACachedResponse(new HttpResponseMessage()))
                .And(x => x.GivenTheDownstreamRouteIs())
                .And(x => x.GivenThereIsADownstreamUrl())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheCacheGetIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_continue_with_pipeline_and_cache_response()
        {
            this.Given(x => x.GivenResponseIsNotCached())
                .And(x => x.GivenTheDownstreamRouteIs())
                .And(x => x.GivenThereAreNoErrors())
                .And(x => x.GivenThereIsADownstreamUrl())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheCacheAddIsCalledCorrectly())
                .BDDfy();
        }

        protected override void GivenTheTestServerServicesAreConfigured(IServiceCollection services)
        {
            services.AddSingleton<IOcelotLoggerFactory, AspDotNetLoggerFactory>();
            services.AddLogging();
            services.AddSingleton(_cacheManager.Object);
            services.AddSingleton(_scopedRepo.Object);
            services.AddSingleton<IRegionCreator, RegionCreator>();
        }

        protected override void GivenTheTestServerPipelineIsConfigured(IApplicationBuilder app)
        {
            app.UseOutputCacheMiddleware();
        }

        private void GivenThereIsACachedResponse(HttpResponseMessage response)
        {
            _response = response;
            _cacheManager
              .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(_response);
        }

        private void GivenResponseIsNotCached()
        {
            _scopedRepo
                .Setup(x => x.Get<HttpResponseMessage>("HttpResponseMessage"))
                .Returns(new OkResponse<HttpResponseMessage>(new HttpResponseMessage()));
        }

        private void GivenTheDownstreamRouteIs()
        {
            var reRoute = new ReRouteBuilder()
                .WithIsCached(true)
                .WithCacheOptions(new CacheOptions(100, "kanken"))
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();
                
            var downstreamRoute = new DownstreamRoute(new List<UrlPathPlaceholderNameAndValue>(), reRoute);

            _scopedRepo
                .Setup(x => x.Get<DownstreamRoute>(It.IsAny<string>()))
                .Returns(new OkResponse<DownstreamRoute>(downstreamRoute));
        }

        private void GivenThereAreNoErrors()
        {
            _scopedRepo
                .Setup(x => x.Get<bool>("OcelotMiddlewareError"))
                .Returns(new OkResponse<bool>(false));
        }

        private void GivenThereIsADownstreamUrl()
        {
            _scopedRepo
                .Setup(x => x.Get<string>("DownstreamUrl"))
                .Returns(new OkResponse<string>("anything"));
        }

        private void ThenTheCacheGetIsCalledCorrectly()
        {
            _cacheManager
                .Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void ThenTheCacheAddIsCalledCorrectly()
        {
            _cacheManager
                .Verify(x => x.Add(It.IsAny<string>(), It.IsAny<HttpResponseMessage>(), It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.Once);
        }
    }
}
