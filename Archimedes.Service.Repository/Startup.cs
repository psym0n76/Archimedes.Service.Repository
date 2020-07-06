using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using EasyNetQ;
using EasyNetQ.AutoSubscribe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

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

            //services.AddHostedService<TestService>();u go ahead


            // message handlersadd a startup derlay
            services.AddScoped<PriceSubscriber>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSingleton(RabbitHutch.CreateBus(config.RabbitHutchConnection));
            services.AddSingleton<MessageDispatcher>();

            services.AddSingleton<AutoSubscriber>(provider => new AutoSubscriber(provider.GetRequiredService<IBus>(), "subs:")
            {
                AutoSubscriberMessageDispatcher = provider.GetRequiredService<MessageDispatcher>()
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> logger,IOptions<Config> config)
        {
            //explains how to setup the app pool to autostart the application
            //https://www.taithienbo.com/how-to-auto-start-and-keep-an-asp-net-core-web-application-and-keep-it-running-on-iis/

            logger.LogInformation("Started configuration:");



            //var ping = new Ping();
            //var retry = 1;

            //while (true)
            //{

            //    logger.LogInformation($"Pinging ({retry}): 127.0.0.1:5673");
            //    var reply = ping.Send("localhost:5673");

            //    if (reply == null)
            //    {
            //        logger.LogInformation("Pinging: ERROR");
            //        break;
            //    }

            //    if (reply.Status == IPStatus.Success)
            //    {
            //        logger.LogInformation("Pinging: SUCCESS");
            //        break;
            //    }

            //    retry++;
            //    Thread.Sleep(10);
            //}

            Thread.Sleep(200);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.ApplicationServices.GetRequiredService<AutoSubscriber>().GenerateSubscriptionId = c => $"{c.ConcreteType.Name}";


            app.ApplicationServices.GetRequiredService<AutoSubscriber>().Subscribe(Assembly.GetExecutingAssembly());

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
