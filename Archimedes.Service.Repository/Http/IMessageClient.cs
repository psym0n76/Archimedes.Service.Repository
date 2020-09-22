using System.Threading.Tasks;
using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface IMessageClient
    {
        Task Post(CandleMessage message);
        Task Post(PriceMessage message);
    }
}