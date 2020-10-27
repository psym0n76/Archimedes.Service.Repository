using System.Threading.Tasks;
using Archimedes.Library.Message.Dto;
using Microsoft.AspNetCore.SignalR;

namespace Archimedes.Service.Repository.Hubs
{
    public class PriceHub : Hub<IPriceHub>
    {
        public async Task Add(PriceDto value)
        {
            await Clients.All.Add(value);
        }

        public async Task Delete(PriceDto value)
        {
            await Clients.All.Delete(value);
        }

        public async Task Update(PriceDto value)
        {
            await Clients.All.Update(value);
        }
    }
}