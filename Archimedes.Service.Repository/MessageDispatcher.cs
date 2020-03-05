﻿using System;
using System.Threading.Tasks;
using EasyNetQ.AutoSubscribe;
using Microsoft.Extensions.DependencyInjection;

namespace Archimedes.Service.Repository
{
    public class MessageDispatcher : IAutoSubscriberMessageDispatcher
    {

        private readonly IServiceProvider _provider;

        public MessageDispatcher(IServiceProvider provider)
        {
            _provider = provider;
        }


        public void Dispatch<TMessage, TConsumer>(TMessage message) where TMessage : class
            where TConsumer : class, IConsume<TMessage>
        {
            using (var scope = _provider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
                consumer.Consume(message);
            }
        }

        public async Task DispatchAsync<TMessage, TConsumer>(TMessage message) where TMessage : class
            where TConsumer : class, IConsumeAsync<TMessage>
        {
            using(var scope = _provider.CreateScope())
            {
                var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
                await consumer.ConsumeAsync(message);
            }
        }
    }
}