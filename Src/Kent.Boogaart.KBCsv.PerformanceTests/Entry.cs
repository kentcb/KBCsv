namespace Kent.Boogaart.KBCsv.PerformanceTests
{
    using System;
    using System.Diagnostics;
    using Kent.Boogaart.KBCsv.PerformanceTests;
    using Kent.Boogaart.KBCsv.PerformanceTests.Utility;

    // allows easy performance profiling
    internal static class Entry
    {
        private static void Main(string[] args)
        {
            var fixture = new ReadPerformanceFixture();

            // warm up
            fixture.read_csv_with_delimiters(WhiteSpacePreservation.All);

            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 10; ++i)
            {
                fixture.read_csv_with_delimiters(WhiteSpacePreservation.All);
            }

            stopwatch.Stop();
            Console.WriteLine("{0}ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
