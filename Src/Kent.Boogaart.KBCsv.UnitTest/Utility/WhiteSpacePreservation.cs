using System;

namespace Kent.Boogaart.KBCsv.UnitTest.Utility
{
    [Flags]
    public enum WhiteSpacePreservation
    {
        None = 0,
        Leading = 1,
        Trailing = 2,
        All = Leading | Trailing
    }
}