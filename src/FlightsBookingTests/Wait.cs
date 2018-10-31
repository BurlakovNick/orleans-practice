using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;

namespace FlightsBookingTests
{
    public static class Wait
    {
        public static async Task For<T>(Func<Task<T>> getter, T expected, TimeSpan timeout = default(TimeSpan), TimeSpan checkInterval = default(TimeSpan))
        {
            var actualTimeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(5) : timeout;
            var actualCheckInterval = checkInterval == default(TimeSpan) ? TimeSpan.FromMilliseconds(500) : timeout;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                var actual = await getter();

                Console.WriteLine($"Actual: {actual}");

                if (actual.Equals(expected))
                {
                    Console.WriteLine($"Assert success. Wait time: {stopwatch.ElapsedMilliseconds} ms");
                    stopwatch.Stop();
                    return;
                }

                Console.WriteLine("Assert fail.");
                if (stopwatch.Elapsed > actualTimeout)
                {
                    Console.WriteLine($"Timeout. Wait time: {stopwatch.ElapsedMilliseconds} ms");
                    stopwatch.Stop();
                    actual.Should().Be(expected);
                    return;
                }

                await Task.Delay(actualCheckInterval);
            }
        }
    }
}