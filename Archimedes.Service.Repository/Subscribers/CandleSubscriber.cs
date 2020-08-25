using Archimedes.Library.Domain;
using Archimedes.Library.RabbitMq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Repository
{
    public class CandleSubscriber : ICandleSubscriber
    {
        private readonly ILogger<CandleSubscriber> _log;
        private readonly Config _config;
        private readonly IClient _httpClient;
        private readonly ICandleConsumer _consumer;

        public CandleSubscriber(ILogger<CandleSubscriber> log, IOptions<Config> config, IClient client, ICandleConsumer consumer)
        {
            _config = config.Value;
            _log = log;
            _httpClient = client;
            _consumer = consumer;
        }

        public void Consume()
        {
            _consumer.HandleMessage += Consumer_HandleMessage;
            _consumer.Subscribe();
        }

        private void Consumer_HandleMessage(object sender, MessageHandlerEventArgs e)
        {
            _log.LogInformation($"Received from CandleResponseQueue Message: {e.Message}");
            var handler = MessageHandlerFactory.Get(e.Message);
            handler.Process(e.Message, _httpClient, _log, _config);
        }
    }
}