namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System.Globalization;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed class PerformanceMessageBus : IMessageBus
    {
        private readonly IMessageBus inner;

        public PerformanceMessageBus(IMessageBus inner)
        {
            this.inner = inner;
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            var testPassedMessage = message as ITestPassed;

            if (testPassedMessage != null)
            {
                var output = string.Format(
                    CultureInfo.InvariantCulture,
                    "Execution time: {0}ms",
                    testPassedMessage.ExecutionTime * 1000);

                message = new TestPassed(testPassedMessage.Test, testPassedMessage.ExecutionTime, output);
            }

            return this.inner.QueueMessage(message);
        }
    }
}