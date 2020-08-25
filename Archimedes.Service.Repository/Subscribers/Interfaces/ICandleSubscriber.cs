using System.Threading;

namespace Archimedes.Service.Repository
{
    public interface ICandleSubscriber
    {
        void Consume(CancellationToken cancellationToken);
    }
}