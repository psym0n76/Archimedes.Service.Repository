using System;
using System.Net.Http;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Repository
{
    public class HttpClientHandler : IClient
    {
        private readonly ILogger<HttpClientHandler> _logger;
        private readonly HttpClient _client;

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1

        public HttpClientHandler(IOptions<Config> config, HttpClient httpClient, ILogger<HttpClientHandler> logger)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }

        public async Task Post(ResponseCandle message)
        {
            if (message.Payload == null)
            {
                _logger.LogError($"Candle payload is empty");
                return;
            }

            var payload = new JsonContent(message.Payload);
            var response = await _client.PostAsync("candle", payload);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}/candle");
                return;
            }

            _logger.LogWarning(
                $"Successfully Posted {message.Payload.Count} Candle(s) {response.ReasonPhrase} from {_client.BaseAddress}/candle");

        }

        public async Task Post(ResponsePrice message)
        {
            {
                if (message.Payload == null)
                {
                    _logger.LogError($"Price payload is empty");
                    return;
                }

                var payload = new JsonContent(message.Payload);
                var response = await _client.PostAsync("price", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}/price");
                    return;
                }

                _logger.LogWarning(
                    $"Successfully Posted {message.Payload.Count} Price(s) {response.ReasonPhrase} from {_client.BaseAddress}/price");

            }
        }
    }
}
