using System.Threading;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Library.RabbitMq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Archimedes.Service.Repository
{
    public class CandleSubscriber : ICandleSubscriber
    {
        private readonly ILogger<CandleSubscriber> _logger;
        private readonly Config _config;
        private readonly IClient _httpClient;
        private readonly ICandleConsumer _consumer;

        public CandleSubscriber(ILogger<CandleSubscriber> logger, IOptions<Config> config, IClient client, ICandleConsumer consumer)
        {
            _config = config.Value;
            _logger = logger;
            _httpClient = client;
            _consumer = consumer;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }


        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(cancellationToken);
        }

        private void Consumer_HandleMessage(object sender, MessageHandlerEventArgs e)
        {
            _logger.LogInformation($"Received from CandleResponseQueue Message: {e.Message}");
            var message = JsonConvert.DeserializeObject<CandleMessage>(e.Message);
            var handler = MessageHandlerFactory.Get(message);

            // not passing the correct message though !!!
            handler.Process(e.Message, _httpClient, _logger, _config);
        }
    }
}