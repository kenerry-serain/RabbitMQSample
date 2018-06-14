using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQSample.EventBus.Abstractions;
using RabbitMQSample.EventBus.Implementations;
using RabbitMQSample.EventBus.RabbitMQ.Abstractions;
using RabbitMQSample.EventBus.RabbitMQ.Implementation;
using RabbitMQSample.Product.Application.Commands;

namespace RabbitMQSample.Product
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging();
            services.AddMediatR(typeof(Startup));
            services.AddSingleton<IEventBusSubscriptionManager, EventBusSubscriptionManager>();
            services.AddSingleton<IRabbitMqPersistentConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    //VirtualHost = settings.EventBusHost,
                    //UserName = settings.EventBusUserName,
                    //Password = settings.EventBusPassword,
                    AutomaticRecoveryEnabled = true
                };

                return new RabbitMqPersistentConnection
                (
                    factory
                );
            });

            services.AddSingleton<IRabbitMqEventBus>(sp =>
            {
                var rabbitMqPersistentConnection = sp.GetRequiredService<IRabbitMqPersistentConnection>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionManager>();
                return new RabbitMqEventBus
                (
                    sp,
                    eventBusSubcriptionsManager,
                    rabbitMqPersistentConnection,
                    "Product.Microservice.Queue"
                );
            });

            services.AddScoped(typeof(IRequestHandler<RegisterProductCommand>), typeof(RegisterProductCommandHandler));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
