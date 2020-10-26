using System.Threading.Tasks;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Service.Repository.Hubs
{
    public interface IMarketMetricHub
    {
        Task Add(MarketDto value);
        Task Delete(MarketDto value);
        Task Update(MarketDto value); 
    }
}