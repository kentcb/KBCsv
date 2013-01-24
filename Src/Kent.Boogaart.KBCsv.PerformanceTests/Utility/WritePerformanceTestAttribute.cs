namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Used to decorate write performance tests, which are executed twice: once to warm up the JIT, and the second time to measure the time taken to execute.
    /// The time taken is included in the test result.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class WritePerformanceTestAttribute : FactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            yield return new WritePerformanceTestCommand(method, false);
            yield return new WritePerformanceTestCommand(method, true);
        }
    }
}
