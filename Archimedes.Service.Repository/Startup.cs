using System.Threading;
using Archimedes.Library.Domain;
using Archimedes.Library.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            services.AddHttpClient<IClient, HttpClientHandler>();

            services.AddLogging();

            services.AddTransient<ICandleSubscriber, CandleSubscriber>();
            services.AddTransient<IPriceSubscriber, PriceSubscriber>();

            services.AddTransient<ICandleConsumer>(x => new CandleConsumer(config.RabbitHost, config.RabbitPort, config.RabbitExchange,"CandleResponseQueue"));
            services.AddTransient<IPriceConsumer>(x => new PriceConsumer(config.RabbitHost, config.RabbitPort, config.RabbitExchange,"PriceResponseQueue"));

            services.AddHostedService<CandleSubscriberService>();
            services.AddHostedService<PriceSubscriberService>();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> logger)
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
            });
        }
    }
}
