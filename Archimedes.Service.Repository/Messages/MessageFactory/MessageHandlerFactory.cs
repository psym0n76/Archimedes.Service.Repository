using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public static class MessageHandlerFactory
    {
        public static IMessage Get<T>(T message)
        {
            switch (message)
            {
                case PriceMessage c:
                {
                    return new PriceMessageProcessor();
                }

                case CandleMessage c:
                {
                    return new CandleMessageProcessor();
                }

                default:
                {
                    return null;
                }
            }
        }
    }
}