using System;
using System.Linq;
using System.Threading;
using Archimedes.Library.Message;
using Archimedes.Library.RabbitMq;
using Archimedes.Service.Repository.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Archimedes.Service.Repository
{
    public class PriceSubscriber : IPriceSubscriber
    {
        private readonly ILogger<PriceSubscriber> _logger;
        private readonly IPriceConsumer _consumer;
        private readonly IMessageClient _messageClient;
        private readonly IHubContext<PriceHub> _context;

        public PriceSubscriber(ILogger<PriceSubscriber> log, IPriceConsumer consumer, IMessageClient messageClient, IHubContext<PriceHub> context)
        {
            _logger = log;
            _consumer = consumer;
            _messageClient = messageClient;
            _context = context;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe();
        }

        private void Consumer_HandleMessage(object sender, MessageHandlerEventArgs args)
        {
            PostPriceMessageToRepository(args);
        }

        private void PostPriceMessageToRepository(MessageHandlerEventArgs args)
        {
            //_logger.LogInformation($"Received from PriceResponseQueue Message: {args.Message}");

            try
            {
                var message = JsonConvert.DeserializeObject<PriceMessage>(args.Message);
                _messageClient.Post(message);
                _context.Clients.All.SendAsync("Update", message.Prices.First());
            }

            catch (JsonException j)
            {
                _logger.LogError($"Unable to Parse Price message {args.Message}{j.Message} {j.StackTrace}");
            }

            catch (Exception e)
            {
                _logger.LogError($"Unable to Post Price message to API {e.Message} {e.StackTrace}");
            }
        }
    }
}