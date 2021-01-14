using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Service.Repository
{
    public interface IHttpRepository
    {
        Task PostCandle(CandleDto candle);
        Task PostCandles(List<CandleDto> candle);
        void Post(PriceMessage message);
        Task DeletePricesOlderThanOneHour();
        Task UpdateMarketMetrics(MarketDto message);

        Task<CandleMetricsDto> GetCandleMetrics(CandleMessage message);
    }
}