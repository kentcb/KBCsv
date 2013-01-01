using System;

namespace Kent.Boogaart.KBCsv.UnitTest
{
    internal static class Entry
    {
        private static void Main(string[] args)
        {
            var fixture = new PerformanceFixture();

            for (var i = 0; i < 10; ++i)
            {
                fixture.read_stackoverflow_data();
            }
        }
    }
}
