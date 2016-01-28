namespace KBCsv.PerformanceTests.Utility
{
    using Xunit.Abstractions;

    public sealed class NullMessageSink : IMessageSink
    {
        public static readonly NullMessageSink Instance = new NullMessageSink();

        private NullMessageSink()
        {
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            return true;
        }
    }
}