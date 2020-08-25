using System.Threading;

namespace Archimedes.Service.Repository
{
    public interface IPriceSubscriber
    {
        void Consume(CancellationToken cancellationToken);
    }
}