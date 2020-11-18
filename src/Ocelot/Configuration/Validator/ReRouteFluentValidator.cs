﻿using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Ocelot.Configuration.File;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Configuration.Validator
{
    public class ReRouteFluentValidator : AbstractValidator<FileReRoute>
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public ReRouteFluentValidator(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;

            RuleFor(reRoute => reRoute.DownstreamPathTemplate)
                .Must(path => path.StartsWith("/"))
                .WithMessage("downstream path {PropertyValue} doesnt start with forward slash");
            RuleFor(reRoute => reRoute.UpstreamPathTemplate)
                .Must(path => path.StartsWith("/"))
                .WithMessage("upstream path {PropertyValue} doesnt start with forward slash");
            RuleFor(reRoute => reRoute.DownstreamPathTemplate)
                .Must(path => !path.Contains("https://") && !path.Contains("http://"))
                .WithMessage("downstream path {PropertyValue} contains scheme");
            RuleFor(reRoute => reRoute.RateLimitOptions)
                .Must(IsValidPeriod)
                .WithMessage("rate limit period {PropertyValue} not contains (s,m,h,d)");
            RuleFor(reRoute => reRoute.AuthenticationOptions)
                .MustAsync(IsSupportedAuthenticationProviders)
                .WithMessage("{PropertyValue} is unsupported authentication provider");
        }

        private async Task<bool> IsSupportedAuthenticationProviders(FileAuthenticationOptions authenticationOptions, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(authenticationOptions.AuthenticationProviderKey))
            {
                return true;
            }
            var schemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
            var supportedSchemes = schemes.Select(scheme => scheme.Name).ToList();

            return supportedSchemes.Contains(authenticationOptions.AuthenticationProviderKey);
        }

        private static bool IsValidPeriod(FileRateLimitRule rateLimitOptions)
        {
            string period = rateLimitOptions.Period;

            return !rateLimitOptions.EnableRateLimiting || period.Contains("s") || period.Contains("m") || period.Contains("h") || period.Contains("d");
        }
    }
}
