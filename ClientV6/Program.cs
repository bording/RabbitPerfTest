using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus;
using Shared;

namespace ClientV6
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Client-V6";

            var config = new EndpointConfiguration("Client");
            config.UseSerialization<JsonSerializer>();

            config.SendOnly();

            var transport = config.UseTransport<RabbitMQTransport>();
            transport.Routing().RouteToEndpoint(typeof(PlaceOrder), "Server");
            //transport.Routing().RouteToEndpoint(typeof(PlaceOrder), "Server-Lazy");

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(type =>
            {
                return type.Namespace == "Shared";
            });

            //conventions.DefiningExpressMessagesAs(type =>
            //{
            //    return type.Namespace == "Shared";
            //});

            var endpoint = await Endpoint.Start(config);

            var numberOfMessages = 200000;
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            await DirectSendAwaitAll(endpoint, numberOfMessages);
            stopwatch.Stop();

            await endpoint.Stop();

            var seconds = Convert.ToDecimal(stopwatch.ElapsedTicks) / Stopwatch.Frequency;
            Console.WriteLine($"Messages: {numberOfMessages} Timer: {seconds} seconds. m/s: {numberOfMessages / seconds}");
            Console.ReadKey();
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
