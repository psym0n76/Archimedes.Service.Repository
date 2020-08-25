using System.Threading.Tasks;
using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface IClient
    {
        Task Post(PriceMessage message);
        Task Post(CandleMessage message);
    }
}