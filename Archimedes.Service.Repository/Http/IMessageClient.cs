using System.Threading.Tasks;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Service.Repository
{
    public interface IMessageClient
    {
         void Post(CandleMessage message);
        Task Post(PriceMessage message);
        Task UpdateMarketMetrics(MarketDto message);

        Task<CandleMetricsDto> GetCandleMetrics(CandleMessage message);
    }
}