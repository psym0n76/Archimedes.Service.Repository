using Archimedes.Library.Message;
using EasyNetQ;
using System.Collections.Generic;
using Archimedes.Library.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Fx.Service.Repository
{
    public class HangfireJob : IHangfireJob
    {
        private readonly Config  _config;
        private readonly ILogger<HangfireJob> _log;

        public HangfireJob(IOptions<Config> configuration, ILogger<HangfireJob> log)
        {
            _config = configuration.Value;
            _log = log;
        }

        public void RunJob()
        {
            _log.LogInformation("Job started: ");
            //do stuff - listen to rabbit q for updates to data base
        }


        public void SendCandleRequest(string queueName)
        {
            _log.LogInformation("Send candle request: ");

            var request = new RequestCandle()
            {
                Properties = new List<string>(),
                Status = "status",
                Text = queueName
            };

            using (var bus = RabbitHutch.CreateBus($"host={_config.RabbitHutchConnection}"))
            {
                bus.Publish(request);
            }
        }
    }
}