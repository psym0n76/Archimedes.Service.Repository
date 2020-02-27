﻿using System.Net.Http;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Fx.Service.Repository
{
    public class HttpClientRequest : IHttpClientRequest
    {
        private readonly Config  _config;
        private readonly ILogger<HttpClientRequest> _log;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientRequest(IOptions<Config> configuration, IHttpClientFactory httpClientFactory, ILogger<HttpClientRequest> log)
        {
            _config = configuration.Value;
            _httpClientFactory = httpClientFactory;
            _log = log;
        }
        public async void PostPrice(ResponseCandle message)
        {
            var records = message.Payload.Count;
            var url = "http://localhost:6103/api/v1/price";
            var payload = new JsonContent(message.Payload);

            using(var  client = _httpClientFactory.CreateClient())
            {
                var response = await client.PostAsync(url, payload);
                if (response.IsSuccessStatusCode)
                {
                    _log.LogInformation($"Successfully POST {records} to {url}");
                }
                else
                {
                    _log.LogError($"Failed to POST to {url}");
                }
            }
        }
    }
}