namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed class NullMessageBus : IMessageBus
    {
        public static NullMessageBus Instance = new NullMessageBus();

        private NullMessageBus()
        {
        }

        public void Dispose()
        {
        }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            return true;
        }
    }
}