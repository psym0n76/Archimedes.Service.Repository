using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public static class MessageHandlerFactory
    {
        public static IMessage Get<T>(T message)
        {
            if (message.GetType() == typeof(ResponsePrice))
            {
                return new PriceMessage();
            }
            return null;
        }
    }
}