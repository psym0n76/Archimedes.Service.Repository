using System.Reflection;
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

namespace Archimedes.Service.Repository
{
    public class Startup
    {
        //https://www.youtube.com/watch?v=oXNslgIXIbQ logging
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddLogging();
            services.AddScoped<IHttpClientRequest, HttpClientRequest>();
            services.AddScoped<ISubscriber, Subscriber>();
            services.AddScoped<IMessageHandler, MessageHandler>();

            services.AddSingleton(Configuration);
            services.Configure<Config>(Configuration.GetSection("AppSettings"));

            var config = Configuration.GetSection("AppSettings").Get<Config>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSingleton(RabbitHutch.CreateBus(config.RabbitHutchConnection));
            services.AddSingleton(provider => new AutoSubscriber(provider.GetRequiredService<IBus>(), Assembly.GetExecutingAssembly().GetName().Name));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> logger)
        {
            logger.LogInformation("Started configuration:");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
