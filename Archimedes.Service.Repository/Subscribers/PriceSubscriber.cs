using System.Threading;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Library.RabbitMq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Archimedes.Service.Repository
{
    public class PriceSubscriber : IPriceSubscriber
    {
        private readonly ILogger<PriceSubscriber> _log;
        private readonly Config _config;
        private readonly IClient _httpClient;
        private readonly IPriceConsumer _consumer;

        public PriceSubscriber(ILogger<PriceSubscriber> log, IOptions<Config> config, IClient client, IPriceConsumer consumer)
        {
            _config = config.Value;
            _log = log;
            _httpClient = client;
            _consumer = consumer;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe();
        }

        private void Consumer_HandleMessage(object sender, MessageHandlerEventArgs e)
        {
            _log.LogInformation($"Received from PriceResponseQueue Message: {e.Message}");
            var message = JsonConvert.DeserializeObject<PriceMessage>(e.Message);
            var handler = MessageHandlerFactory.Get(message);
            handler.Process(e.Message, _httpClient, _log, _config);
        }
    }
}