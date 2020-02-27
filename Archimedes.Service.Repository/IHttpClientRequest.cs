using Archimedes.Library.Message;

namespace Archimedes.Fx.Service.Repository
{
    public interface IHttpClientRequest
    {
        void PostPrice(ResponseCandle message);
    }
}