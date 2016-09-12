using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            config.UseSerialization<JsonSerializer>();

            config.UseTransport<RabbitMQTransport>().DisableCallbackReceiver();
            config.UsePersistence<InMemoryPersistence>();
            config.EnableInstallers();

            using (var bus = Bus.Create(config))
            {
                bus.Start();
                Console.WriteLine("Server started. Press any key to quit");
                Console.ReadKey();
            }
        }
    }
}
