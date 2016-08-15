using NServiceBus;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            Console.Title = "Client";

            var config = new EndpointConfiguration("Client");
            config.UseSerialization<JsonSerializer>();

            config.SendOnly();

            var transport = config.UseTransport<RabbitMQTransport>();
            transport.Routing().RouteToEndpoint(typeof(PlaceOrder), "Server");

            var endpoint = await Endpoint.Start(config);

            var numberOfMessages = 200000;
            await DirectSendAwaitAll(endpoint, numberOfMessages);

            await endpoint.Stop();
        }

        static Task DirectSendAwaitAll(IEndpointInstance endpoint, int number)
        {
            var sends = new List<Task>();

            for (int i = 0; i < number; i++)
            {
                var send = endpoint.Send(new PlaceOrder { Id = Guid.NewGuid(), Product = "Message Body" });

                sends.Add(send);
            }

            return Task.WhenAll(sends);
        }
    }
}
