using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using NServiceBus;
using Shared;

namespace ClientV7
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if NETCOREAPP2_0
            Console.Title = "Client-V7-netcoreapp2.0";
#else
            Console.Title = "Client-V7-net47";
#endif

            var config = new EndpointConfiguration("Client");
            config.UseSerialization<NewtonsoftSerializer>();

            config.SendOnly();

            var transport = config.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString(ConfigurationManager.ConnectionStrings["transport"].ConnectionString);
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
