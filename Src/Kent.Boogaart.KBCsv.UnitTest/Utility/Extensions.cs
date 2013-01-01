using System.Collections.Generic;

namespace Kent.Boogaart.KBCsv.UnitTest.Utility
{
    public static class Extensions
    {
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> @this, int count)
        {
            for (var i = 0; i < count; ++i)
            {
                foreach (var item in @this)
                {
                    yield return item;
                }
            }
        }
    }
}