using System.Net.Http;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Repository
{
    public class HttpClientRequest : IHttpClientRequest
    {
        private readonly Config _config;
        private readonly ILogger<HttpClientRequest> _log;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientRequest(IOptions<Config> config, IHttpClientFactory httpClientFactory,
            ILogger<HttpClientRequest> log)
        {
            _config = config.Value;
            _httpClientFactory = httpClientFactory;
            _log = log;
        }

        public async void PostPrice(PriceResponse message)
        {
            if (message.Payload == null)
            {
                _log.LogError($"Price message is null");
                return;
            }

            var records = message.Payload.Count;
            var url = $"{_config.ApiRepositoryUrl}/price";
            var payload = new JsonContent(message.Payload);

            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.PostAsync(url, payload);
                if (!response.IsSuccessStatusCode)
                {
                    _log.LogError($"Failed to POST to {url}");
                    return;
                }

                _log.LogInformation($"Successfully POST {records} to {url}");
            }
        }
    }
}