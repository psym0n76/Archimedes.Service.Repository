using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using EasyNetQ.AutoSubscribe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Repository
{
    public class PriceSubscriber : IConsume<ResponsePrice>
    {
        private readonly ILogger<PriceSubscriber> _log;
        private readonly Config _config;
        private readonly IHttpClientRequest _httpClient;

        public PriceSubscriber(ILogger<PriceSubscriber> log, IOptions<Config> config, IHttpClientRequest client)
        {
            _config = config.Value;
            _log = log;
            _httpClient = client;
        }

        //[AutoSubscriberConsumer(SubscriptionId = "Candle")]
        public void Consume(ResponsePrice message)
        {
            _log.LogInformation($"Received Price message {message.Text}");

            var handler = MessageHandlerFactory.Get(message);
            handler.Process(message, _httpClient, _log, _config);
        }
    }
}