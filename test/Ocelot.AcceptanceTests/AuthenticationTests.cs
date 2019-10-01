using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using IdentityServer4.Models;
using IdentityServer4.Services.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using TestStack.BDDfy;
using Xunit;
using YamlDotNet.Serialization;

namespace Ocelot.AcceptanceTests
{
    using System.Security.Claims;
    using Library.Configuration.Yaml;

    public class AuthenticationTests : IDisposable
    {
        private TestServer _ocelotServer;
        private HttpClient _ocelotClient;
        private HttpResponseMessage _response;
        private readonly string _configurationPath;
        private StringContent _postContent;
        private IWebHost _servicebuilder;

        // Sadly we need to change this when we update the netcoreapp version to make the test update the config correctly
        private double _netCoreAppVersion = 1.4;
        private BearerToken _token;
        private IWebHost _identityServerBuilder;

        public AuthenticationTests()
        {
            _configurationPath = $"./bin/Debug/netcoreapp{_netCoreAppVersion}/configuration.yaml";
        }

        [Fact]
        public void should_return_401_using_identity_server_access_token()
        {
            this.Given(x => x.GivenThereIsAnIdentityServerOn("http://localhost:51888", "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn("http://localhost:51876", 201, string.Empty))
                .And(x => x.GivenThereIsAConfiguration(new YamlConfiguration
                {
                    ReRoutes = new List<YamlReRoute>
                    {
                        new YamlReRoute
                        {
                            DownstreamTemplate = "http://localhost:51876/",
                            UpstreamTemplate = "/",
                            UpstreamHttpMethod = "Post",
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
                .And(x => x.GivenTheApiGatewayIsRunning())
                .And(x => x.GivenThePostHasContent("postContent"))
                .When(x => x.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => x.ThenTheStatusCodeShouldBe(HttpStatusCode.Unauthorized))
                .BDDfy();
        }

        [Fact]
        public void should_return_401_using_identity_server_reference_token()
        {
            this.Given(x => x.GivenThereIsAnIdentityServerOn("http://localhost:51888", "api", AccessTokenType.Reference))
                .And(x => x.GivenThereIsAServiceRunningOn("http://localhost:51876", 201, string.Empty))
                .And(x => x.GivenThereIsAConfiguration(new YamlConfiguration
                {
                    ReRoutes = new List<YamlReRoute>
                    {
                        new YamlReRoute
                        {
                            DownstreamTemplate = "http://localhost:51876/",
                            UpstreamTemplate = "/",
                            UpstreamHttpMethod = "Post",
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
                .And(x => x.GivenTheApiGatewayIsRunning())
                .And(x => x.GivenThePostHasContent("postContent"))
                .When(x => x.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => x.ThenTheStatusCodeShouldBe(HttpStatusCode.Unauthorized))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_using_identity_server()
        {

            this.Given(x => x.GivenThereIsAnIdentityServerOn("http://localhost:51888", "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn("http://localhost:51876", 200, "Hello from Laura"))
                .And(x => x.GivenIHaveAToken("http://localhost:51888"))
                .And(x => x.GivenThereIsAConfiguration(new YamlConfiguration
                {
                    ReRoutes = new List<YamlReRoute>
                    {
                        new YamlReRoute
                        {
                            DownstreamTemplate = "http://localhost:51876/",
                            UpstreamTemplate = "/",
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
                .And(x => x.GivenTheApiGatewayIsRunning())
                .And(x => x.GivenIHaveAddedATokenToMyRequest())
                .When(x => x.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => x.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_and_foward_claim_as_header()
        {

            this.Given(x => x.GivenThereIsAnIdentityServerOn("http://localhost:51888", "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn("http://localhost:51876", 200, "Hello from Laura"))
                .And(x => x.GivenIHaveAToken("http://localhost:51888"))
                .And(x => x.GivenThereIsAConfiguration(new YamlConfiguration
                {
                    ReRoutes = new List<YamlReRoute>
                    {
                        new YamlReRoute
                        {
                            DownstreamTemplate = "http://localhost:51876/",
                            UpstreamTemplate = "/",
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
                                { "CustomerId", "Claims[CustomerId] -> value" },
                                { "LocationId", "Claims[LocationId] -> value"},
                                { "UserId", "Claims[Subject] -> delimiter(|) -> value[0]" },
                                { "UserId", "Claims[Subject] -> delimiter(|) -> value[1]" }
                            }
                        }
                    }
                }))
                .And(x => x.GivenTheApiGatewayIsRunning())
                .And(x => x.GivenIHaveAddedATokenToMyRequest())
                .When(x => x.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => x.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_return_201_using_identity_server_access_token()
        {
            this.Given(x => x.GivenThereIsAnIdentityServerOn("http://localhost:51888", "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn("http://localhost:51876", 201, string.Empty))
                .And(x => x.GivenIHaveAToken("http://localhost:51888"))
                .And(x => x.GivenThereIsAConfiguration(new YamlConfiguration
                {
                    ReRoutes = new List<YamlReRoute>
                    {
                        new YamlReRoute
                        {
                            DownstreamTemplate = "http://localhost:51876/",
                            UpstreamTemplate = "/",
                            UpstreamHttpMethod = "Post",
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
                .And(x => x.GivenTheApiGatewayIsRunning())
                .And(x => x.GivenIHaveAddedATokenToMyRequest())
                .And(x => x.GivenThePostHasContent("postContent"))
                .When(x => x.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => x.ThenTheStatusCodeShouldBe(HttpStatusCode.Created))
                .BDDfy();
        }

        [Fact]
        public void should_return_201_using_identity_server_reference_token()
        {
            this.Given(x => x.GivenThereIsAnIdentityServerOn("http://localhost:51888", "api", AccessTokenType.Reference))
                .And(x => x.GivenThereIsAServiceRunningOn("http://localhost:51876", 201, string.Empty))
                .And(x => x.GivenIHaveAToken("http://localhost:51888"))
                .And(x => x.GivenThereIsAConfiguration(new YamlConfiguration
                {
                    ReRoutes = new List<YamlReRoute>
                    {
                        new YamlReRoute
                        {
                            DownstreamTemplate = "http://localhost:51876/",
                            UpstreamTemplate = "/",
                            UpstreamHttpMethod = "Post",
                            AuthenticationOptions = new YamlAuthenticationOptions
                            {
                                AdditionalScopes = new List<string>(),
                                Provider = "IdentityServer",
                                ProviderRootUrl = "http://localhost:51888",
                                RequireHttps = false,
                                ScopeName = "api",
                                ScopeSecret = "secret"
                            }
                        }
                    }
                }))
                .And(x => x.GivenTheApiGatewayIsRunning())
                .And(x => x.GivenIHaveAddedATokenToMyRequest())
                .And(x => x.GivenThePostHasContent("postContent"))
                .When(x => x.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => x.ThenTheStatusCodeShouldBe(HttpStatusCode.Created))
                .BDDfy();
        }

        private void WhenIGetUrlOnTheApiGateway(string url)
        {   
            _response = _ocelotClient.GetAsync(url).Result;     
        }

        private void WhenIPostUrlOnTheApiGateway(string url)
        {
            _response = _ocelotClient.PostAsync(url, _postContent).Result;
        }

        private void ThenTheResponseBodyShouldBe(string expectedBody)
        {
            _response.Content.ReadAsStringAsync().Result.ShouldBe(expectedBody);
        }

        private void GivenThePostHasContent(string postcontent)
        {
            _postContent = new StringContent(postcontent);
        }

        /// <summary>
        /// This is annoying cos it should be in the constructor but we need to set up the yaml file before calling startup so its a step.
        /// </summary>
        private void GivenTheApiGatewayIsRunning()
        {
            _ocelotServer = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());

            _ocelotClient = _ocelotServer.CreateClient();
        }

        private void GivenThereIsAConfiguration(YamlConfiguration yamlConfiguration)
        {
            var serializer = new Serializer();

            if (File.Exists(_configurationPath))
            {
                File.Delete(_configurationPath);
            }

            using (TextWriter writer = File.CreateText(_configurationPath))
            {
                serializer.Serialize(writer, yamlConfiguration);
            }
        }

        private void GivenThereIsAServiceRunningOn(string url, int statusCode, string responseBody)
        {
            _servicebuilder = new WebHostBuilder()
                .UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        context.Response.StatusCode = statusCode;
                        await context.Response.WriteAsync(responseBody);
                    });
                })
                .Build();

            _servicebuilder.Start();
        }

        private void GivenThereIsAnIdentityServerOn(string url, string scopeName, AccessTokenType tokenType)
        {
            _identityServerBuilder = new WebHostBuilder()
                .UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddDeveloperIdentityServer()
                        .AddInMemoryScopes(new List<Scope>
                        {
                            new Scope
                            {
                                Name = scopeName,
                                Description = "My API",
                                Enabled = true,
                                AllowUnrestrictedIntrospection = true,
                                ScopeSecrets = new List<Secret>()
                                {
                                    new Secret
                                    {
                                        Value = "secret".Sha256()
                                    }
                                }
                            },

                            StandardScopes.OpenId,
                            StandardScopes.OfflineAccess
                        })
                        .AddInMemoryClients(new List<Client>
                        {
                            new Client
                            {
                                ClientId = "client",
                                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                                ClientSecrets = new List<Secret> {new Secret("secret".Sha256())},
                                AllowedScopes = new List<string> { scopeName, "openid", "offline_access" },
                                AccessTokenType = tokenType,
                                Enabled = true,
                                RequireClientSecret = false
                            }
                        })
                        .AddInMemoryUsers(new List<InMemoryUser>
                        {
                            new InMemoryUser
                            {
                                Username = "test",
                                Password = "test",
                                Enabled = true,
                                Subject = "registered|1231231",
                                Claims = new List<Claim>
                                {
                                   new Claim("CustomerId", "123"),
                                   new Claim("LocationId", "321")
                                }
                            }
                        });
                })
                .Configure(app =>
                {
                    app.UseIdentityServer();
                })
                .Build();

            _identityServerBuilder.Start();

            VerifyIdentiryServerStarted(url);

        }

        private void VerifyIdentiryServerStarted(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync($"{url}/.well-known/openid-configuration").Result;
                response.EnsureSuccessStatusCode();
            }
        }

        private void GivenIHaveAToken(string url)
        {
            var tokenUrl = $"{url}/connect/token";
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", "client"),
                new KeyValuePair<string, string>("client_secret", "secret"),
                new KeyValuePair<string, string>("scope", "api"),
                new KeyValuePair<string, string>("username", "test"),
                new KeyValuePair<string, string>("password", "test"),
                new KeyValuePair<string, string>("grant_type", "password")
            };
            var content = new FormUrlEncodedContent(formData);

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync(tokenUrl, content).Result;
                response.EnsureSuccessStatusCode();
                var responseContent = response.Content.ReadAsStringAsync().Result;
                _token = JsonConvert.DeserializeObject<BearerToken>(responseContent);
            }
        }

        private void GivenIHaveAddedATokenToMyRequest()
        {
            _ocelotClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        }

        private void ThenTheStatusCodeShouldBe(HttpStatusCode expectedHttpStatusCode)
        {
            _response.StatusCode.ShouldBe(expectedHttpStatusCode);
        }

        public void Dispose()
        {
            _servicebuilder?.Dispose();
            _ocelotClient?.Dispose();
            _ocelotServer?.Dispose();
            _identityServerBuilder?.Dispose();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class BearerToken
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }
    }
}
