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
                fixture.compare_old_to_new();
            }
        }
    }
}
