namespace KBCsv.PerformanceTests.Utility
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed class ReadPerformanceTestCase : XunitTestCase
    {
        private WhiteSpacePreservation whiteSpacePreservation;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", true)]
        public ReadPerformanceTestCase() { }

        public ReadPerformanceTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay testMethodDisplay, ITestMethod testMethod, WhiteSpacePreservation whiteSpacePreservation)
            : base(diagnosticMessageSink, testMethodDisplay, testMethod)
        {
            this.whiteSpacePreservation = whiteSpacePreservation;
            this.TestMethodArguments = new object[] { this.whiteSpacePreservation };
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

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("WhiteSpacePreservation", this.whiteSpacePreservation);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            this.whiteSpacePreservation = data.GetValue<WhiteSpacePreservation>("WhiteSpacePreservation");
        }
    }
}