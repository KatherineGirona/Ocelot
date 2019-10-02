﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Ocelot.Library.RequestBuilder;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    using Library.Builder;
    using Library.Configuration;
    using Library.Configuration.Yaml;
    using Library.Responses;

    public class OcelotConfigurationTests
    {
        private readonly Mock<IOptions<YamlConfiguration>> _yamlConfig;
        private readonly Mock<IConfigurationValidator> _validator;
        private OcelotConfiguration _config;
        private YamlConfiguration _yamlConfiguration;
        private readonly Mock<IConfigurationHeaderExtrator> _configExtractor;
        private readonly Mock<ILogger<OcelotConfiguration>> _logger;

        public OcelotConfigurationTests()
        {
            _logger = new Mock<ILogger<OcelotConfiguration>>();
            _configExtractor = new Mock<IConfigurationHeaderExtrator>();
            _validator = new Mock<IConfigurationValidator>();
            _yamlConfig = new Mock<IOptions<YamlConfiguration>>();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_anything_to_end_of_string()
        {
            this.Given(x => x.GivenTheYamlConfigIs(new YamlConfiguration
            {
                ReRoutes = new List<YamlReRoute>
                {
                    new YamlReRoute
                    {
                        UpstreamTemplate = "/api/products/{productId}",
                        DownstreamTemplate = "/products/{productId}",
                        UpstreamHttpMethod = "Get"
                    }
                }
            }))
                .And(x => x.GivenTheYamlConfigIsValid())
                .When(x => x.WhenIInstanciateTheOcelotConfig())
                .Then(x => x.ThenTheReRoutesAre(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamTemplate("/products/{productId}")
                        .WithUpstreamTemplate("/api/products/{productId}")
                        .WithUpstreamHttpMethod("Get")
                        .WithUpstreamTemplatePattern("/api/products/.*$")
                        .Build()
                }))
                .BDDfy();
        }

        [Fact]
        public void should_create_with_headers_to_extract()
        {
            var expected = new List<ReRoute>
            {
                new ReRouteBuilder()
                    .WithDownstreamTemplate("/products/{productId}")
                    .WithUpstreamTemplate("/api/products/{productId}")
                    .WithUpstreamHttpMethod("Get")
                    .WithUpstreamTemplatePattern("/api/products/.*$")
                    .WithAuthenticationProvider("IdentityServer")
                    .WithAuthenticationProviderUrl("http://localhost:51888")
                    .WithRequireHttps(false)
                    .WithScopeSecret("secret")
                    .WithAuthenticationProviderScopeName("api")
                    .WithConfigurationHeaderExtractorProperties(new List<ConfigurationHeaderExtractorProperties>
                    {
                        new ConfigurationHeaderExtractorProperties("CustomerId", "CustomerId", "", 0),
                    })
                    .Build()
            };

            this.Given(x => x.GivenTheYamlConfigIs(new YamlConfiguration
            {
                ReRoutes = new List<YamlReRoute>
                {
                    new YamlReRoute
                    {
                        UpstreamTemplate = "/api/products/{productId}",
                        DownstreamTemplate = "/products/{productId}",
                        UpstreamHttpMethod = "Get",
                        AuthenticationOptions = new YamlAuthenticationOptions
                            {
                                AdditionalScopes =  new List<string>(),
                                Provider = "IdentityServer",
                                ProviderRootUrl = "http://localhost:51888",
                                RequireHttps = false,
                                ScopeName = "api",
                                ScopeSecret = "secret"
                            },
                        AddHeadersToRequest =
                        {
                            {"CustomerId", "Claims[CustomerId] > value"},
                        }
                    }
                }
            }))
                .And(x => x.GivenTheYamlConfigIsValid())
                .And(x => x.GivenTheConfigHeaderExtractorReturns(new ConfigurationHeaderExtractorProperties("CustomerId", "CustomerId", "", 0)))
                .When(x => x.WhenIInstanciateTheOcelotConfig())
                .Then(x => x.ThenTheReRoutesAre(expected))
                .And(x => x.ThenTheAuthenticationOptionsAre(expected))
                .BDDfy();
        }

        private void GivenTheConfigHeaderExtractorReturns(ConfigurationHeaderExtractorProperties expected)
        {
            _configExtractor
                .Setup(x => x.Extract(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new OkResponse<ConfigurationHeaderExtractorProperties>(expected));
        }

        [Fact]
        public void should_create_with_authentication_properties()
        {
            var expected = new List<ReRoute>
            {
                new ReRouteBuilder()
                    .WithDownstreamTemplate("/products/{productId}")
                    .WithUpstreamTemplate("/api/products/{productId}")
                    .WithUpstreamHttpMethod("Get")
                    .WithUpstreamTemplatePattern("/api/products/.*$")
                    .WithAuthenticationProvider("IdentityServer")
                    .WithAuthenticationProviderUrl("http://localhost:51888")
                    .WithRequireHttps(false)
                    .WithScopeSecret("secret")
                    .WithAuthenticationProviderScopeName("api")
                    .Build()
            };

            this.Given(x => x.GivenTheYamlConfigIs(new YamlConfiguration
            {
                ReRoutes = new List<YamlReRoute>
                {
                    new YamlReRoute
                    {
                        UpstreamTemplate = "/api/products/{productId}",
                        DownstreamTemplate = "/products/{productId}",
                        UpstreamHttpMethod = "Get",
                        AuthenticationOptions = new YamlAuthenticationOptions
                            {
                                AdditionalScopes =  new List<string>(),
                                Provider = "IdentityServer",
                                ProviderRootUrl = "http://localhost:51888",
                                RequireHttps = false,
                                ScopeName = "api",
                                ScopeSecret = "secret"
                            }
                    }
                }
            }))
                .And(x => x.GivenTheYamlConfigIsValid())
                .When(x => x.WhenIInstanciateTheOcelotConfig())
                .Then(x => x.ThenTheReRoutesAre(expected))
                .And(x => x.ThenTheAuthenticationOptionsAre(expected))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_more_than_one_placeholder()
        {
            this.Given(x => x.GivenTheYamlConfigIs(new YamlConfiguration
            {
                ReRoutes = new List<YamlReRoute>
                {
                    new YamlReRoute
                    {
                        UpstreamTemplate = "/api/products/{productId}/variants/{variantId}",
                        DownstreamTemplate = "/products/{productId}",
                        UpstreamHttpMethod = "Get"
                    }
                }
            }))
                .And(x => x.GivenTheYamlConfigIsValid())
                .When(x => x.WhenIInstanciateTheOcelotConfig())
                .Then(x => x.ThenTheReRoutesAre(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamTemplate("/products/{productId}")
                        .WithUpstreamTemplate("/api/products/{productId}/variants/{variantId}")
                        .WithUpstreamHttpMethod("Get")
                        .WithUpstreamTemplatePattern("/api/products/.*/variants/.*$")
                        .Build()
                }))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_more_than_one_placeholder_with_trailing_slash()
        {
            this.Given(x => x.GivenTheYamlConfigIs(new YamlConfiguration
            {
                ReRoutes = new List<YamlReRoute>
                {
                    new YamlReRoute
                    {
                        UpstreamTemplate = "/api/products/{productId}/variants/{variantId}/",
                        DownstreamTemplate = "/products/{productId}",
                        UpstreamHttpMethod = "Get"
                    }
                }
            }))
                .And(x => x.GivenTheYamlConfigIsValid())
                .When(x => x.WhenIInstanciateTheOcelotConfig())
                .Then(x => x.ThenTheReRoutesAre(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamTemplate("/products/{productId}")
                        .WithUpstreamTemplate("/api/products/{productId}/variants/{variantId}/")
                        .WithUpstreamHttpMethod("Get")
                        .WithUpstreamTemplatePattern("/api/products/.*/variants/.*/$")
                        .Build()
                }))
                .BDDfy();
        }

        [Fact]
        public void should_create_template_pattern_that_matches_to_end_of_string()
        {
            this.Given(x => x.GivenTheYamlConfigIs(new YamlConfiguration
            {
                ReRoutes = new List<YamlReRoute>
                {
                    new YamlReRoute
                    {
                        UpstreamTemplate = "/",
                        DownstreamTemplate = "/api/products/",
                        UpstreamHttpMethod = "Get"
                    }
                }
            }))
                .And(x => x.GivenTheYamlConfigIsValid())
                .When(x => x.WhenIInstanciateTheOcelotConfig())
                .Then(x => x.ThenTheReRoutesAre(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamTemplate("/api/products/")
                        .WithUpstreamTemplate("/")
                        .WithUpstreamHttpMethod("Get")
                        .WithUpstreamTemplatePattern("/$")
                        .Build()
                }))
                .BDDfy();
        }

        private void GivenTheYamlConfigIsValid()
        {
            _validator
                .Setup(x => x.IsValid(It.IsAny<YamlConfiguration>()))
                .Returns(new OkResponse<ConfigurationValidationResult>(new ConfigurationValidationResult(false)));
        }

        private void GivenTheYamlConfigIs(YamlConfiguration yamlConfiguration)
        {
            _yamlConfiguration = yamlConfiguration;
            _yamlConfig
                .Setup(x => x.Value)
                .Returns(_yamlConfiguration);
        }

        private void WhenIInstanciateTheOcelotConfig()
        {
            _config = new OcelotConfiguration(_yamlConfig.Object, _validator.Object,
                _configExtractor.Object, _logger.Object);
        }

        private void ThenTheReRoutesAre(List<ReRoute> expectedReRoutes)
        {
            for (int i = 0; i < _config.ReRoutes.Count; i++)
            {
                var result = _config.ReRoutes[i];
                var expected = expectedReRoutes[i];

                result.DownstreamTemplate.ShouldBe(expected.DownstreamTemplate);
                result.UpstreamHttpMethod.ShouldBe(expected.UpstreamHttpMethod);
                result.UpstreamTemplate.ShouldBe(expected.UpstreamTemplate);
                result.UpstreamTemplatePattern.ShouldBe(expected.UpstreamTemplatePattern);
            }
        }

        private void ThenTheAuthenticationOptionsAre(List<ReRoute> expectedReRoutes)
        {
            for (int i = 0; i < _config.ReRoutes.Count; i++)
            {
                var result = _config.ReRoutes[i].AuthenticationOptions;
                var expected = expectedReRoutes[i].AuthenticationOptions;

                result.AdditionalScopes.ShouldBe(expected.AdditionalScopes);
                result.Provider.ShouldBe(expected.Provider);
                result.ProviderRootUrl.ShouldBe(expected.ProviderRootUrl);
                result.RequireHttps.ShouldBe(expected.RequireHttps);
                result.ScopeName.ShouldBe(expected.ScopeName);
                result.ScopeSecret.ShouldBe(expected.ScopeSecret);

            }
        }
    }
}
