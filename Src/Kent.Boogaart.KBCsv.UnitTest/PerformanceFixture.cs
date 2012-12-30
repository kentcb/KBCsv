using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
    public sealed class PerformanceFixture
    {
        [Fact]
        public void measure_reading_stackoverflow_answers()
        {
            // warm up
            using (var reader = new CsvReader(@"C:\Repository\KBCsv\trunk\Src\Kent.Boogaart.KBCsv.UnitTest\StackoverflowAnswers.csv"))
            {
                this.measure_reading_stackoverflow_answers_work(reader);
            }

            using (var reader = new CsvReader(@"C:\Repository\KBCsv\trunk\Src\Kent.Boogaart.KBCsv.UnitTest\StackoverflowAnswers.csv"))
            {
                using (new PerfBlock("Reading stackoverflow answers."))
                {
                    this.measure_reading_stackoverflow_answers_work(reader);
                }

                Console.WriteLine("{0} answers read.", reader.RecordNumber);
            }
        }

        private void measure_reading_stackoverflow_answers_work(CsvReader reader)
        {
            //reader.ReadHeaderRecord();

            while (reader.HasMoreRecords)
            {
                var record = reader.ReadDataRecord();
                //Assert.Equal(12, record.Values.Count);
            }
        }

        private struct PerfBlock : IDisposable
        {
            private readonly string message;
            private readonly object[] messageArgs;
            private readonly Stopwatch stopwatch;

            public PerfBlock(string message, params object[] messageArgs)
            {
                this.message = message;
                this.messageArgs = messageArgs;
                this.stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                this.stopwatch.Stop();
                var formattedMessage = string.Format(this.message, this.messageArgs);
                Console.WriteLine("{0} took {1} ({2}ms)", formattedMessage, this.stopwatch.Elapsed, this.stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
