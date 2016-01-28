namespace KBCsv.PerformanceTests.Utility
{
    using System.Collections.Generic;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed class WritePerformanceTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public WritePerformanceTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            yield return new WritePerformanceTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, true);
            yield return new WritePerformanceTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, false);
        }
    }
}