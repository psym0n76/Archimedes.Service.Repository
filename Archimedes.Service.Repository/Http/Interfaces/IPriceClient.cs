using System.Threading.Tasks;
using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface IPriceClient
    {
        Task Post(PriceResponse message);
    }
}