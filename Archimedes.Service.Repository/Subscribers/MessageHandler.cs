using System;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Repository
{
    public class MessageHandler : IMessageHandler
    {
        private readonly Config _config;
        private readonly ILogger<Subscriber> _log;
        private readonly IHttpClientRequest _client;

        public MessageHandler(IHttpClientRequest client, ILogger<Subscriber> log, IOptions<Config> configuration)
        {
            _client = client;
            _log = log;
            _config = configuration.Value;
        }

        public void Process<T>(T message) where T : IResponse
        {
            if (message == null) return;
            if (message.GetType() != typeof(ResponseCandle)) return;
            var candle = message as ResponseCandle;

            try
            {
                _client.PostPrice(candle);
            }
            catch (Exception e)
            {
                _log.LogError(
                    $"Error posting Candle payload to {_config.DatabaseServerConnection} database, {e.Message}");
                throw;
            }

            if (candle != null)
                _log.LogInformation(
                    $"Got message: {candle.Status} and {candle.Text} and {candle.Payload.GetType()}");
        }
    }
}