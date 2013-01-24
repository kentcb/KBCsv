namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System;
    using System.Diagnostics;
    using Xunit.Sdk;

    /// <summary>
    /// A command used to execute a write performance test.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class WritePerformanceTestCommand : TestCommand
    {
        private readonly bool forceDelimit;

        public WritePerformanceTestCommand(IMethodInfo method, bool forceDelimit)
            : base(method, method.Name, -1)
        {
            this.forceDelimit = forceDelimit;
        }

        public override MethodResult Execute(object testClass)
        {
            var parameters = new object[] { this.forceDelimit };

            // warm up
            this.testMethod.Invoke(testClass, parameters);

            // now run the test method again, this time measuring the time taken
            var stopwatch = Stopwatch.StartNew();
            this.testMethod.Invoke(testClass, parameters);
            stopwatch.Stop();

            // put the time taken into the pass result
            return new PassedResult(this.testMethod, string.Format("{0} with force delimit '{1}': {2}ms", this.testMethod.Name, this.forceDelimit, stopwatch.ElapsedMilliseconds));
        }
    }
}
