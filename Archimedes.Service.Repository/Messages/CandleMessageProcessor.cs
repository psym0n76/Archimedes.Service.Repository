using System;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class CandleMessageProcessor : IMessage
    {
        public void Process<T>(T message, IClient httpClient, ILogger log, Config config)
        {
            var candle = message as CandleMessage;

            try
            {
                httpClient.Post(candle);
            }
            catch (Exception e)
            {
                log.LogError(
                    $"Error posting Candle Payload: {candle} to {config.DatabaseServerConnection} - {e.Message} {e.StackTrace}");
            }

            if (candle != null)
                log.LogInformation(
                    $"Received message: {candle}");
        }
    }
}