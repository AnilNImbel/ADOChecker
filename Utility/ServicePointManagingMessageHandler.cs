// <copyright file="ServicePointManagingMessageHandler.cs" company="Odysseus Solutions, LLC">
// Copyright (c) Odysseus Solutions, LLC. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ADOAnalyser
{
    /// <summary>
    /// ServicePoint Managing Message Handler,
    /// Sets ConnectionLeaseTimeout for each base uri to detect DNS changes
    /// </summary>
    public class ServicePointManagingMessageHandler : HttpClientHandler
    {
        private readonly ConcurrentDictionary<Uri, ServicePoint> uriCache = new ConcurrentDictionary<Uri, ServicePoint>();

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.IsAbsoluteUri)
            {
                var baseUri = new Uri($"{request.RequestUri.Scheme}://{request.RequestUri.Host}");

                uriCache.GetOrAdd(baseUri, uri =>
                {
                    var sp = ServicePointManager.FindServicePoint(uri);
                    sp.ConnectionLeaseTimeout = 60000; // 1 Minute

                    return sp;
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
