// <copyright file="HttpClientAdapter.cs" company="Odysseus Solutions, LLC">
// Copyright (c) Odysseus Solutions, LLC. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;

namespace ADOAnalyser
{

    /// <summary>
    /// Adapter for HttpClient.
    /// </summary>
    public static class HttpClientAdapter
    {
        /// <summary>
        /// Instance of HttpClient.
        /// </summary>
        public static readonly HttpClient Client;

        /// <summary>
        /// Initializes static members of the <see cref="HttpClientAdapter"/> class.
        /// Initializes properties and members of .<see cref="HttpClientAdapter">
        /// </summary>
        static HttpClientAdapter()
        {
            var handler = GetHttpClientHandler();

            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.ConnectionClose = false;
        }

        /// <summary>
        /// Generates new instance of <see cref="HttpClient" />. Use only when proxy is required and reuse the same instace for same proxy. 
        /// Otherwise, just use singleton instance HttpClientAdapter.Client
        /// </summary>
        /// <param name="proxy">instance of <see cref="WebProxy" /></param>
        /// <returns>new instance of <see cref="HttpClient" /></returns>
        public static HttpClient GetNewInstanceWithProxy(WebProxy proxy)
        {
            var handler = GetHttpClientHandler();
            handler.Proxy = proxy;
            handler.UseProxy = true;

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.ConnectionClose = false;

            return client;
        }

        private static HttpClientHandler GetHttpClientHandler()
        {
            return new ServicePointManagingMessageHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
        }
    }
}
