using System;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class PriceMessageProcessor : IMessage
    {
        public void Process<T>(T message, IClient httpClient, ILogger log, Config config)
        {
            var price = message as PriceMessage;

            try
            {
                httpClient.Post(price);
            }
            catch (Exception e)
            {
                log.LogError(
                    $"Error posting Price payload: {price} {config.DatabaseServerConnection} - {e.Message} {e.StackTrace}");
            }

            if (price != null)
                log.LogInformation(
                    $"Received message: {price}");
        }
    }
}