﻿using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface providing a method to create a new http client
    /// </summary>
    public interface IHTTPClientProvider
    {
        /// <summary>
        /// Returns an HTTPClient based on the provided configuration
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IHTTPClient CreateClient(ILogger logger, HTTPClientConfiguration configuration);
    }
}
