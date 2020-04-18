using Archimedes.Library.Domain;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public interface IMessage
    {
        void Process<T>(T message, IPriceClient httpClient, ILogger log, Config config);
    }
}