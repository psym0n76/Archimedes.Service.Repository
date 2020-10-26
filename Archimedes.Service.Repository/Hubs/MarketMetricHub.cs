using System.Threading.Tasks;
using Archimedes.Library.Message.Dto;
using Microsoft.AspNetCore.SignalR;

namespace Archimedes.Service.Repository.Hubs
{
    public class MarketMetricHub : Hub<IMarketMetricHub>
    {
        public async Task Add(MarketDto value)
        {
            await Clients.All.Add(value);
        }

        public async Task Delete(MarketDto value)
        {
            await Clients.All.Delete(value);
        }

        public async Task Update(MarketDto value)
        {
            await Clients.All.Update(value);
        }
    }
}