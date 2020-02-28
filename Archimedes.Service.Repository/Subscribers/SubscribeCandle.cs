using Archimedes.Library.Message;
using EasyNetQ.AutoSubscribe;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class SubscribeCandle : IConsume<ResponseCandle>
    {
        private readonly ILogger<SubscribeCandle> _log;
        private readonly IMessageHandler _message;

        public SubscribeCandle(ILogger<SubscribeCandle> log, IMessageHandler message)
        {
            _log = log;
            _message = message;
        }

        //[AutoSubscriberConsumer(SubscriptionId = "Candle")]
        public void Consume(ResponseCandle message)
        {
            _log.LogInformation($"Received Candle message {message.Text}");
            _message.Process(message);
        }
    }
}