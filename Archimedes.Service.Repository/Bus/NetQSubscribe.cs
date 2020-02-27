using System;
using EasyNetQ;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Fx.Service.Repository
{
    public class NetQSubscribe : INetQSubscriber
    {
        private readonly Config  _config;
        private readonly ILogger<NetQSubscribe> _log;
        private readonly IHttpClientRequest _client;

        public NetQSubscribe(IOptions<Config> configuration,  ILogger<NetQSubscribe> log, IHttpClientRequest client)
        {
            _config = configuration.Value;
            _log = log;
            _client = client;
        }

        public void SubscribeCandleMessage()
        {
            using (var bus = RabbitHutch.CreateBus(_config.RabbitHutchConnection))
            {
                bus.Subscribe<IResponse>("Candle", @interface =>
                {
                    if (@interface is ResponseCandle candle)
                    {
                        HandleTextMessage(candle);
                    }
                });

               _log.LogInformation("Listening for Candle messages. Hit <return> to quit.");
            }
        }


        private void HandleTextMessage<T>(T message) where T : IResponse
        {
            if (message == null) return;
            if (message.GetType() == typeof(ResponseCandle))
            {
                var candle = message as ResponseCandle;

                try
                {
                    _client.PostPrice(candle);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error posting Candle payload to {_config.DatabaseServerConnection} database, {e.Message}");
                    throw;
                }

                _log.LogInformation($"Got message: {candle.Status} and {candle.Text} and {candle.Payload.GetType()}");
            }
        }
    }
}