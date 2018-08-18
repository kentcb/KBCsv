namespace KBCsv.PerformanceTests.Utility
{
    using System;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Used to decorate read performance tests, which are executed twice: once to warm up the JIT, and the second time to measure the time taken to execute.
    /// The time taken 
    /// </summary>
    [XunitTestCaseDiscoverer("KBCsv.PerformanceTests.Utility.ReadPerformanceTestDiscoverer", "KBCsv.PerformanceTests")]
    [TraitDiscoverer("KBCsv.PerformanceTests.Utility.PerformanceTraitDiscoverer", "KBCsv.PerformanceTests")]
    public sealed class ReadPerformanceTestAttribute : FactAttribute, ITraitAttribute
    {
    }
}