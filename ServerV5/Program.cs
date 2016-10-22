using System;
using NServiceBus;

namespace ServerV5
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server-V5";

            var config = new BusConfiguration();
            config.EndpointName("Server");
            //config.EndpointName("Server-Lazy");

            config.UseSerialization<JsonSerializer>();

            config.UseTransport<RabbitMQTransport>().DisableCallbackReceiver();
            config.UsePersistence<InMemoryPersistence>();
            config.EnableInstallers();

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(type =>
            {
                return type.Namespace == "Shared";
            });

            using (var bus = Bus.Create(config))
            {
                bus.Start();
                Console.WriteLine("Server started. Press any key to quit");
                Console.ReadKey();
            }

            PlaceOrderHandler.DisplayStats();
        }
    }
}
