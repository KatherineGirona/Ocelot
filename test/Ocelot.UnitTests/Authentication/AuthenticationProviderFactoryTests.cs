﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;
using Ocelot.Library.Infrastructure.Authentication;
using Ocelot.Library.Infrastructure.Errors;
using Ocelot.Library.Infrastructure.Responses;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Authentication
{
    public class AuthenticationProviderFactoryTests
    {
        private readonly IAuthenticationProviderFactory _authenticationProviderFactory;
        private readonly Mock<IApplicationBuilder> _app;
        private readonly Mock<IAuthenticationHandlerCreator> _creator;

        private string _provider;
        private Response<AuthenticationHandler> _result;

        public AuthenticationProviderFactoryTests()
        {
            _app = new Mock<IApplicationBuilder>();
            _creator = new Mock<IAuthenticationHandlerCreator>();
            _authenticationProviderFactory = new AuthenticationProviderFactory(_creator.Object);
        }

        [Fact]
        public void should_return_identity_server_access_token_provider()
        {
            this.Given(x => x.GivenTheProviderIs("IdentityServer.AccessToken"))
                .And(x => x.GivenTheCreatorReturns())
                .When(x => x.WhenIGetFromTheFactory())
                .Then(x => x.ThenTheHandlerIsReturned("IdentityServer.AccessToken"))
                .BDDfy();
        }

        [Fact]
        public void should_return_error_if_cannot_create_handler()
        {
            this.Given(x => x.GivenTheProviderIs("IdentityServer.AccessToken"))
                .And(x => x.GivenTheCreatorReturnsAnError())
                .When(x => x.WhenIGetFromTheFactory())
                .Then(x => x.ThenAnErrorResponseIsReturned())
                .BDDfy();
        }

        private void GivenTheCreatorReturnsAnError()
        {
            _creator
                .Setup(x => x.CreateIdentityServerAuthenticationHandler(It.IsAny<IApplicationBuilder>()))
                .Returns(new ErrorResponse<RequestDelegate>(new List<Error>
            {
                new UnableToCreateAuthenticationHandlerError($"Unable to create authentication handler for xxx")
            }));
        }

        private void GivenTheCreatorReturns()
        {
            _creator
                .Setup(x => x.CreateIdentityServerAuthenticationHandler(It.IsAny<IApplicationBuilder>()))
                .Returns(new OkResponse<RequestDelegate>(x => Task.CompletedTask));
        }

        private void GivenTheProviderIs(string provider)
        {
            _provider = provider;
        }

        private void WhenIGetFromTheFactory()
        {
            _result = _authenticationProviderFactory.Get(_provider, _app.Object);
        }

        private void ThenTheHandlerIsReturned(string expected)
        {
            _result.Data.Provider.ShouldBe(expected);
        }

        private void ThenAnErrorResponseIsReturned()
        {
            _result.IsError.ShouldBeTrue();
        }
    }
}
