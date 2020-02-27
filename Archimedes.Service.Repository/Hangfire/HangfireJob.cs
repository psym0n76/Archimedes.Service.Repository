using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Archimedes.Fx.Service.Repository
{
    public class HangfireJob : IHangfireJob
    {
        private readonly ILogger<HangfireJob> _log;
        private readonly INetQSubscriber _subscriber;

        public HangfireJob(ILogger<HangfireJob> log, INetQSubscriber subscriber)
        {
            _log = log;
            _subscriber = subscriber;
        }

        public void RunJob()
        {
            _log.LogInformation("Job started: ");
            SubscribeToQueue();
        }

        private void SubscribeToQueue()
        {
            Task.Run(() => { _subscriber.SubscribeCandleMessage(); });
        }
    }
}