using System;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class PriceMessage : IMessage
    {
        public void Process<T>(T message, IHttpClientRequest httpClient, ILogger log, Config config)
        {
            var price = message as PriceResponse;

            try
            {
                httpClient.PostPrice(price);
            }
            catch (Exception e)
            {
                log.LogError(
                    $"Error posting Price payload to {config.DatabaseServerConnection} database, {e.Message}");
                throw;
            }

            if (price != null)
                log.LogInformation(
                    $"Received message: {price.Status} and {price.Text} and {price.Payload.GetType().FullName}");
        }
    }
}