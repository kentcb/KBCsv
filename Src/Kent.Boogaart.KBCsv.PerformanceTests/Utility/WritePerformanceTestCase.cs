namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [CLSCompliant(false)]
    public sealed class WritePerformanceTestCase : XunitTestCase
    {
        private bool forceDelimit;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", true)]
        public WritePerformanceTestCase() { }

        public WritePerformanceTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay testMethodDisplay, ITestMethod testMethod, bool forceDelimit)
            : base(diagnosticMessageSink, testMethodDisplay, testMethod)
        {
            this.forceDelimit = forceDelimit;
            this.TestMethodArguments = new object[] { this.forceDelimit };
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

            data.AddValue("ForceDelimit", this.forceDelimit);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            this.forceDelimit = data.GetValue<bool>("ForceDelimit");
        }
    }
}