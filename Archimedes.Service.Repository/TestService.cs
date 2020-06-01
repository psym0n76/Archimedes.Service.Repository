using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Archimedes.Service.Repository
{
    public class TestService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}