namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [CLSCompliant(false)]
    public sealed class PerformanceTestCase : XunitTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", true)]
        public PerformanceTestCase() { }

        public PerformanceTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay testMethodDisplay, ITestMethod testMethod)
            : base(diagnosticMessageSink, testMethodDisplay, testMethod)
        {
        }

        public async override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            // warm up
            await base.RunAsync(NullMessageSink.Instance, NullMessageBus.Instance, constructorArguments, aggregator, cancellationTokenSource);

            var performanceMessageBus = new PerformanceMessageBus(messageBus);
            return await base.RunAsync(diagnosticMessageSink, performanceMessageBus, constructorArguments, aggregator, cancellationTokenSource);
        }
    }
}