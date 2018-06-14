using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQSample.EventBus.Abstractions;
using RabbitMQSample.EventBus.Implementations;
using RabbitMQSample.EventBus.RabbitMQ.Abstractions;
using RabbitMQSample.EventBus.RabbitMQ.Implementation;
using RabbitMQSample.IntegrationEvents.Events;
using RabbitMQSample.Stock.EventsHandlers;

namespace RabbitMQSample.Stock
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
            services.AddSingleton<IEventBusSubscriptionManager, EventBusSubscriptionManager>();
            services.AddSingleton<IRabbitMqPersistentConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
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

            services.AddScoped<RegisteredProductEventHandler>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();

            var eventBus = app.ApplicationServices.GetRequiredService<IRabbitMqEventBus>();
            eventBus.Subscribe<RegisteredProductEvent, RegisteredProductEventHandler>();
            eventBus.StartConsumer();
        }
    }
}
