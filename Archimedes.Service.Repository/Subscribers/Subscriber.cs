using Archimedes.Library.Message;
using EasyNetQ.AutoSubscribe;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class Subscriber : IConsume<ResponseCandle>, ISubscriber
    {
        private readonly ILogger<Subscriber> _log;
        private readonly IMessageHandler _message;

        public Subscriber(ILogger<Subscriber> log, IMessageHandler message)
        {
            _log = log;
            _message = message;
        }

        [AutoSubscriberConsumer(SubscriptionId = "Candle")]
        public void Consume(ResponseCandle message)
        {
            _log.LogInformation($"Received Candle message {message.Text}");
            _message.Process(message);
        }
    }
}