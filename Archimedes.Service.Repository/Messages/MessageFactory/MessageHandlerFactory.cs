using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public static class MessageHandlerFactory
    {
        public static IMessage Get<T>(T message)
        {
            switch (message)
            {
                case ResponsePrice c:
                {
                    return new PriceMessage();
                }

                case ResponseCandle c:
                {
                    return new CandleMessage();
                }

                default:
                {
                    return null;
                }
            }
        }
    }
}