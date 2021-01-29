using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Service.Repository
{
    public interface IHttpRepository
    {
        Task<bool> PostCandle(CandleDto candle);
        Task<bool> PostCandles(List<CandleDto> candle);
        void PostPrice(PriceMessage message);
        Task DeletePricesOlderThanOneHour();
        Task UpdateMarket(MarketDto message);

        Task<CandleMetricsDto> GetCandleMetrics(CandleMessage message);
    }
}