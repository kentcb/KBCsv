namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Used to decorate performance tests, which are executed twice: once to warm up the JIT, and the second time to measure the time taken to execute.
    /// The time taken is included in the test output.
    /// </summary>
    [CLSCompliant(false)]
    [XunitTestCaseDiscoverer("Kent.Boogaart.KBCsv.PerformanceTests.Utility.PerformanceTestDiscoverer", "Kent.Boogaart.KBCsv.PerformanceTests")]
    [TraitDiscoverer("Kent.Boogaart.KBCsv.PerformanceTests.Utility.PerformanceTraitDiscoverer", "Kent.Boogaart.KBCsv.PerformanceTests")]
    public sealed class PerformanceTestAttribute : FactAttribute, ITraitAttribute
    {
    }
}