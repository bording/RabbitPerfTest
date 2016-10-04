using NServiceBus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            Console.Title = "Server";

            var config = new EndpointConfiguration("Server");
            config.UseSerialization<JsonSerializer>();

            var prefetchMultiplier = Convert.ToUInt16(ConfigurationManager.AppSettings["PrefetchMultiplier"]);
            var concurrency = Convert.ToInt32(ConfigurationManager.AppSettings["Concurrency"]);

            config.UseTransport<RabbitMQTransport>().PrefetchMultiplier(prefetchMultiplier);
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
        }
    }
}
