using System;
using System.Net.Http;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Repository
{
    public class PriceClient : IPriceClient
    {
        private readonly Config _config;
        private readonly ILogger<PriceClient> _log;
        private readonly HttpClient _client;

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1

        public PriceClient(IOptions<Config> config, HttpClient httpClient, ILogger<PriceClient> log)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _config = config.Value;
            _log = log; 
        }

        public async Task Post(PriceResponse message)
        {
            if (message.Payload == null)
            {
                _log.LogError($"Price message is null");
                return;
            }

            var records = message.Payload.Count;
            var url = $"{_config.ApiRepositoryUrl}/price";
            var payload = new JsonContent(message.Payload);

            var response = await _client.PostAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Failed to POST to {url}");
            }

            _log.LogInformation($"Successfully POST {records} to {url}");
        }
    }
}
