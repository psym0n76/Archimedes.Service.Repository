using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Library.RabbitMq;
using Archimedes.Service.Repository.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Archimedes.Service.Repository
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.Configure<Config>(Configuration.GetSection("AppSettings"));

            var config = Configuration.GetSection("AppSettings").Get<Config>();

            services.AddHttpClient<IMessageClient, MessageClient>();

            services.AddLogging();
            services.AddSignalR();
            services.AddTransient<ICandleSubscriber, CandleSubscriber>();
            services.AddTransient<IPriceSubscriber, PriceSubscriber>();

            services.AddTransient<IProducer<StrategyMessage>>(x => new Producer<StrategyMessage>(config.RabbitHost, config.RabbitPort,config.RabbitExchange));
            services.AddTransient<ICandleConsumer>(x => new CandleConsumer(config.RabbitHost, config.RabbitPort, config.RabbitExchange,"CandleResponseQueue"));
            services.AddTransient<IPriceConsumer>(x => new PriceConsumer(config.RabbitHost, config.RabbitPort, config.RabbitExchange,"PriceResponseQueue"));

            services.AddHostedService<CandleSubscriberService>();
            services.AddHostedService<PriceSubscriberService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //explains how to setup the app pool to autostart the application
            //https://www.taithienbo.com/how-to-auto-start-and-keep-an-asp-net-core-web-application-and-keep-it-running-on-iis/

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MarketHub>("/hubs/market");
            });
        }
    }
}
