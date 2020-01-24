using Microsoft.Extensions.Options;

namespace Archimedes.Fx.Service.Repository
{
    public interface IHangfireJob
    {
        void RunJob();
    }
}