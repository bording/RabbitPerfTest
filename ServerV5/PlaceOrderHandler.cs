using System;
using System.Diagnostics;
using System.Threading;
using NServiceBus;
using Shared;

namespace ServerV5
{
    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        const int warmup = 10000;
        const int maximum = 200000;
        static int messageCount;
        static Stopwatch stopwatch = new Stopwatch();

        public void Handle(PlaceOrder message)
        {
            Interlocked.Increment(ref messageCount);

            if (messageCount == warmup)
            {
                stopwatch.Start();
            }
            else if (messageCount == maximum)
            {
                stopwatch.Stop();
            }
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