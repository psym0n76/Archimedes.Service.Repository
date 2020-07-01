using System;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class CandleMessage : IMessage
    {
        public void Process<T>(T message, IClient httpClient, ILogger log, Config config)
        {
            var candle = message as ResponseCandle;

            try
            {
                httpClient.Post(candle);
            }
            catch (Exception e)
            {
                log.LogError(
                    $"Error posting Candle payload to {config.DatabaseServerConnection} - {e.Message}");
                throw;
            }

            if (candle != null)
                log.LogInformation(
                    $"Received message: {candle.Status} and {candle.Text} and {candle.Payload}");
        }
    }
}