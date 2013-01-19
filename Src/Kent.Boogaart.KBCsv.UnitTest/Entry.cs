using System;
using System.Diagnostics;
using Kent.Boogaart.KBCsv.UnitTest.Utility;

namespace Kent.Boogaart.KBCsv.UnitTest
{
    internal static class Entry
    {
        private static void Main(string[] args)
        {
            var fixture = new PerformanceFixture();
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
