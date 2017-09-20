using System;
using System.Configuration;
using System.Threading.Tasks;
using NServiceBus;

namespace ServerV6
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Server-V6";

            var config = new EndpointConfiguration("Server");
            //var config = new EndpointConfiguration("Server-Lazy");
            config.UseSerialization<JsonSerializer>();

            var prefetchMultiplier = Convert.ToUInt16(ConfigurationManager.AppSettings["PrefetchMultiplier"]);
            var concurrency = Convert.ToInt32(ConfigurationManager.AppSettings["Concurrency"]);

            var transport = config.UseTransport<RabbitMQTransport>();
            transport.PrefetchMultiplier(prefetchMultiplier);
            transport.DelayedDelivery().DisableTimeoutManager();

            config.UsePersistence<InMemoryPersistence>();
            config.EnableInstallers();
            config.SendFailedMessagesTo("error");

            config.Recoverability().DisableLegacyRetriesSatellite();
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
