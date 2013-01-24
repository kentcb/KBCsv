namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using System.Diagnostics;
    using Xunit.Sdk;

    /// <summary>
    /// A command used to execute a general performance test.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class PerformanceTestCommand : TestCommand
    {
        public PerformanceTestCommand(IMethodInfo method)
            : base(method, method.Name, -1)
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
            return new PassedResult(this.testMethod, string.Format("{0}: {1}ms", this.testMethod.Name, stopwatch.ElapsedMilliseconds));
        }
    }
}
