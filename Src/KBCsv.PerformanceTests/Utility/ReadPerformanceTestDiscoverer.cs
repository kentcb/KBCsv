namespace KBCsv.PerformanceTests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed class ReadPerformanceTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private static readonly WhiteSpacePreservation[] whiteSpacePreservations = Enum
            .GetValues(typeof(WhiteSpacePreservation))
            .Cast<WhiteSpacePreservation>()
            .ToArray();

        private readonly IMessageSink diagnosticMessageSink;

        public ReadPerformanceTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return whiteSpacePreservations
                .Select(x => new ReadPerformanceTestCase(this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, x));
        }
    }
}