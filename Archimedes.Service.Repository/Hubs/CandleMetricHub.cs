using System.Threading.Tasks;
using Archimedes.Library.Message.Dto;
using Microsoft.AspNetCore.SignalR;

namespace Archimedes.Service.Repository.Hubs
{
    public class CandleMetricHub : Hub<ICandleMetricHub>
    {
        public async Task Add(CandleMetricDto value)
        {
            await Clients.All.Add(value);
        }

        public async Task Delete(CandleMetricDto value)
        {
            await Clients.All.Delete(value);
        }

        public async Task Update(CandleMetricDto value)
        {
            await Clients.All.Update(value);
        }
    }
}