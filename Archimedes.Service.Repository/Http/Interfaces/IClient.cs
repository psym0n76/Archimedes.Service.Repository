using System.Threading.Tasks;
using Archimedes.Library.Message;

namespace Archimedes.Service.Repository
{
    public interface IClient
    {
        Task Post(ResponsePrice message);
        Task Post(ResponseCandle message);
    }
}