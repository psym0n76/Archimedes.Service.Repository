using System.Threading.Tasks;
using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface IMessageClient
    {
         void Post(CandleMessage message);
        Task Post(PriceMessage message);
        Task UpdateMarketMetrics(CandleMessage message);
    }
}