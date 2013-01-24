namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Used to decorate read performance tests, which are executed twice: once to warm up the JIT, and the second time to measure the time taken to execute.
    /// The time taken is included in the test result.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class ReadPerformanceTestAttribute : FactAttribute
    {
        private readonly WhiteSpacePreservation[] whiteSpacePreservations;

        public ReadPerformanceTestAttribute()
        {
            this.whiteSpacePreservations = Enum.GetValues(typeof(WhiteSpacePreservation)).Cast<WhiteSpacePreservation>().ToArray();
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            foreach (var whiteSpacePreservation in this.whiteSpacePreservations)
            {
                yield return new ReadPerformanceTestCommand(method, whiteSpacePreservation);
            }
        }
    }
}
