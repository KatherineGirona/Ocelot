﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ocelot.Configuration
{
    /// <summary>
    /// RateLimit Options
    /// </summary>
    public class RateLimitOptions
    {
        public RateLimitOptions(bool enbleRateLimiting, string clientIdHeader, List<string> clientWhitelist,bool disableRateLimitHeaders,
            string quotaExceededMessage, string rateLimitCounterPrefix, RateLimitRule rateLimitRule)
        {
            EnableRateLimiting = enbleRateLimiting;
            ClientIdHeader = clientIdHeader;
            ClientWhitelist = clientWhitelist;
            DisableRateLimitHeaders = disableRateLimitHeaders;
            QuotaExceededMessage = quotaExceededMessage;
            RateLimitCounterPrefix = rateLimitCounterPrefix;
            RateLimitRule = rateLimitRule;
        }


        public RateLimitRule RateLimitRule { get;  private set; }

        public List<string> ClientWhitelist { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP header that holds the client identifier, by default is X-ClientId
        /// </summary>
        public string ClientIdHeader { get; private set; } = "ClientId";

        /// <summary>
        /// Gets or sets the HTTP Status code returned when rate limiting occurs, by default value is set to 429 (Too Many Requests)
        /// </summary>
        public int HttpStatusCode { get; private set; } = 429;

        /// <summary>
        /// Gets or sets a value that will be used as a formatter for the QuotaExceeded response message.
        /// If none specified the default will be: 
        /// API calls quota exceeded! maximum admitted {0} per {1}
        /// </summary>
        public string QuotaExceededMessage { get; private set; }

        /// <summary>
        /// Gets or sets the counter prefix, used to compose the rate limit counter cache key
        /// </summary>
        public string RateLimitCounterPrefix { get; private set; } = "ocelot";

        /// <summary>
        /// Enables endpoint rate limiting based URL path and HTTP verb
        /// </summary>
        public bool EnableRateLimiting { get; private set; }

        /// <summary>
        /// Disables X-Rate-Limit and Rety-After headers
        /// </summary>
        public bool DisableRateLimitHeaders { get; private set; }
    }

    public class RateLimitRule
    {
        /// <summary>
        /// Rate limit period as in 1s, 1m, 1h
        /// </summary>
        public string Period { get; set; }

        public TimeSpan? PeriodTimespan { get; set; }
        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public long Limit { get; set; }
    }
}
