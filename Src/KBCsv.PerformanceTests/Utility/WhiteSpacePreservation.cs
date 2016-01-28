namespace KBCsv.PerformanceTests.Utility
{
    using System;

    [Flags]
    public enum WhiteSpacePreservation
    {
        None = 0,
        Leading = 1,
        Trailing = 2,
        All = Leading | Trailing
    }
}