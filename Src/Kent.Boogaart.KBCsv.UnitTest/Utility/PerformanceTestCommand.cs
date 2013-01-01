using System;
using System.Diagnostics;
using Xunit.Sdk;

namespace Kent.Boogaart.KBCsv.UnitTest.Utility
{
    /// <summary>
    /// A command used to execute a performance test.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class PerformanceTestCommand : TestCommand
    {
        public PerformanceTestCommand(IMethodInfo method)
            : base(method)
        {
        }

        public override MethodResult Execute(object testClass)
        {
            // warm up
            this.testMethod.Invoke(testClass);

            // now run the test method again, this time measuring the time taken
            var stopwatch = Stopwatch.StartNew();
            this.testMethod.Invoke(testClass);
            stopwatch.Stop();

            // put the time taken into the pass result
            return new PassedResult(this.testMethod, string.Format("PERFORMANCE MEASUREMENT: {0}ms", stopwatch.ElapsedMilliseconds));
        }
    }
}
