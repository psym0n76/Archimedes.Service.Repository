using System;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class PriceMessage : IMessage
    {
        public void Process<T>(T message, IClient httpClient, ILogger log, Config config)
        {
            var price = message as ResponsePrice;

            try
            {
                httpClient.Post(price);
            }
            catch (Exception e)
            {
                log.LogError(
                    $"Error posting Price payload to {config.DatabaseServerConnection} database, {e.Message}");
                throw;
            }

            if (price != null)
                log.LogInformation(
                    $"Received message: {price.Status} and {price.Text} and {price}");
        }
    }
}