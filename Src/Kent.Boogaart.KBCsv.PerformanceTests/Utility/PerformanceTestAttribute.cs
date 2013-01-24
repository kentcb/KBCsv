namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Used to decorate performance tests, which are executed twice: once to warm up the JIT, and the second time to measure the time taken to execute.
    /// The time taken is included in the test result.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class PerformanceTestAttribute : FactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            yield return new PerformanceTestCommand(method);
        }
    }
}
