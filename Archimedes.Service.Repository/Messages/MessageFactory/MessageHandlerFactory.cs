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

                default:
                {
                    return null;
                }
            }
        }
    }
}