using System;
using System.Diagnostics;
using Xunit.Sdk;

namespace Kent.Boogaart.KBCsv.UnitTest.Utility
{
    /// <summary>
    /// A command used to execute a performance test.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class ReadPerformanceTestCommand : TestCommand
    {
        private readonly WhiteSpacePreservation whiteSpacePreservation;

        public ReadPerformanceTestCommand(IMethodInfo method, WhiteSpacePreservation whiteSpacePreservation)
            : base(method)
        {
            this.whiteSpacePreservation = whiteSpacePreservation;
        }

        public override MethodResult Execute(object testClass)
        {
            var parameters = new object[] { this.whiteSpacePreservation };

            // warm up
            this.testMethod.Invoke(testClass, parameters);

            // now run the test method again, this time measuring the time taken
            var stopwatch = Stopwatch.StartNew();
            this.testMethod.Invoke(testClass, parameters);
            stopwatch.Stop();

            // put the time taken into the pass result
            return new PassedResult(this.testMethod, string.Format("With white space preservation '{0}': {1}ms", this.whiteSpacePreservation, stopwatch.ElapsedMilliseconds));
        }
    }
}
