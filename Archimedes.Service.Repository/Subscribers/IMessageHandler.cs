using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface IMessageHandler
    {
        void Process<T>(T message) where T : IResponse;
    }
}