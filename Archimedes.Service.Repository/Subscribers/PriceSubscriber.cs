using System;
using System.Linq;
using System.Threading;
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
        private readonly IPriceFanoutConsumer _consumer;
        private readonly IHttpRepository _messageClient;
        private readonly IHubContext<PriceHub> _context;

        public PriceSubscriber(ILogger<PriceSubscriber> log, IPriceFanoutConsumer consumer, IHttpRepository messageClient, IHubContext<PriceHub> context)
        {
            _logger = log;
            _consumer = consumer;
            _messageClient = messageClient;
            _context = context;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        private void Consumer_HandleMessage(object sender, PriceMessageHandlerEventArgs e)
        {
            PostPriceMessageToRepository(e);
        }

        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(cancellationToken);
        }

        private void PostPriceMessageToRepository(PriceMessageHandlerEventArgs args)
        {
            try
            {

                if (!args.Prices.Any())
                {
                    _logger.LogError($"Missing prices {args.Message}");
                    return;
                }

                var message = args.Message;
                _messageClient.PostPrice(message);
                _context.Clients.All.SendAsync("Update", message.Prices.First());
            }

            catch (JsonException j)
            {
                _logger.LogError($"Unable to Parse Price message {args.Message}{j.Message} {j.StackTrace}");
            }

            catch (Exception e)
            {
                _logger.LogError($"Unable to PostPrice Price message to API {e.Message} {e.StackTrace}");
            }
        }
    }
}