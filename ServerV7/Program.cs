using System;
using System.Configuration;
using System.Threading.Tasks;
using NServiceBus;

namespace ServerV7
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if NETCOREAPP2_0
            Console.Title = "Server-V7-netcoreapp2.0";
#else
            Console.Title = "Server-V7-net47";
#endif

            var config = new EndpointConfiguration("Server");
            //var config = new EndpointConfiguration("Server-Lazy");
            config.UseSerialization<NewtonsoftSerializer>();

            var prefetchMultiplier = Convert.ToUInt16(ConfigurationManager.AppSettings["PrefetchMultiplier"]);
            var concurrency = Convert.ToInt32(ConfigurationManager.AppSettings["Concurrency"]);

            var transport = config.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString(ConfigurationManager.ConnectionStrings["transport"].ConnectionString);
            transport.PrefetchMultiplier(prefetchMultiplier);
            transport.DelayedDelivery().DisableTimeoutManager();

            config.UsePersistence<InMemoryPersistence>();
            config.EnableInstallers();
            config.SendFailedMessagesTo("error");

            config.LimitMessageProcessingConcurrencyTo(concurrency);

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(type =>
            {
                return type.Namespace == "Shared";
            });

            var endpoint = await Endpoint.Start(config);

            Console.WriteLine("Server started. Press any key to quit");
            Console.ReadKey();

            await endpoint.Stop();

            PlaceOrderHandler.DisplayStats();
        }
    }
}
