using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface ISubscriber
    {
        void Consume(ResponseCandle message);
    }
}