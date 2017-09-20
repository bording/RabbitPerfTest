using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using Shared;

namespace ServerV7
{
    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        const int warmup = 10000;
        const int maximum = 200000;
        static int messageCount;
        static readonly Stopwatch stopwatch = new Stopwatch();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            var count = Interlocked.Increment(ref messageCount);

            if (count == warmup)
            {
                stopwatch.Start();
            }
            else if (count == maximum)
            {
                stopwatch.Stop();
            }

            return Task.FromResult(0);
        }

        public static void DisplayStats()
        {
            var seconds = Convert.ToDecimal(stopwatch.ElapsedTicks) / Stopwatch.Frequency;

            int totalMessages = messageCount;

            if (messageCount > maximum)
            {
                totalMessages = maximum;
            }

            var messages = totalMessages - warmup;

            if (seconds != 0)
            {
                Console.WriteLine($"Messages: {messages} Timer: {seconds} seconds. m/s: {messages / seconds}");
                Console.ReadKey();
            }
        }
    }
}

