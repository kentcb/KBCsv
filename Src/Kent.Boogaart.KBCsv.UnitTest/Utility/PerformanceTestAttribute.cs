using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Sdk;

namespace Kent.Boogaart.KBCsv.UnitTest.Utility
{
    /// <summary>
    /// Used to decorate performance tests, which are executed twice: once to warm up the JIT, and the second time to measure the time taken to execute.
    /// The time taken is included in the test result.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class PerformanceTestAttribute : FactAttribute
    {
        private readonly Stopwatch stopwatch;

        public PerformanceTestAttribute()
        {
            this.stopwatch = new Stopwatch();
            this.Skip = "Performance tests must be explicitly enabled.";
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            yield return new PerformanceTestCommand(method);
        }
    }
}
