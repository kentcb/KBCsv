namespace KBCsv.PerformanceTests.Utility
{
    using System;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Used to decorate write performance tests, which are executed twice: once to warm up the JIT, and the second time to measure the time taken to execute.
    /// </summary>
    [CLSCompliant(false)]
    [XunitTestCaseDiscoverer("KBCsv.PerformanceTests.Utility.WritePerformanceTestDiscoverer", "KBCsv.PerformanceTests")]
    [TraitDiscoverer("KBCsv.PerformanceTests.Utility.PerformanceTraitDiscoverer", "KBCsv.PerformanceTests")]
    public sealed class WritePerformanceTestAttribute : FactAttribute, ITraitAttribute
    {
    }
}