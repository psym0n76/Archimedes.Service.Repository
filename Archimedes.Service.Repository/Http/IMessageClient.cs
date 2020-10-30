using System.Threading.Tasks;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Service.Repository
{
    public interface IMessageClient
    {
        void Post(CandleMessage message);
        void Post(PriceMessage message);
        Task DeletePricesOlderThanOneHour();
        Task UpdateMarketMetrics(MarketDto message);

        Task<CandleMetricsDto> GetCandleMetrics(CandleMessage message);
    }
}