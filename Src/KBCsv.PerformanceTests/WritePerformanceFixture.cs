namespace KBCsv.PerformanceTests
{
    using System.IO;
    using KBCsv.PerformanceTests.Utility;
    using Xunit;
    
    public sealed class WritePerformanceFixture
    {
        [WritePerformanceTest]
        public void write_plain_csv(bool forceDelimit)
        {
            var repeatCount = 200000;

            using (var csvWriter = new CsvWriter(new StringWriter()))
            {
                csvWriter.ForceDelimit = forceDelimit;

                foreach (var record in Data.PlainDataValues.Repeat(repeatCount))
                {
                    csvWriter.WriteRecord(record);
                }

                Assert.Equal(3 * repeatCount, csvWriter.RecordNumber);
            }
        }

        [WritePerformanceTest]
        public void write_csv_with_copious_whitespace(bool forceDelimit)
        {
            var repeatCount = 200000;

            using (var csvWriter = new CsvWriter(new StringWriter()))
            {
                csvWriter.ForceDelimit = forceDelimit;

                foreach (var record in Data.CopiousWhiteSpaceValues.Repeat(repeatCount))
                {
                    csvWriter.WriteRecord(record);
                }

                Assert.Equal(3 * repeatCount, csvWriter.RecordNumber);
            }
        }

        [WritePerformanceTest]
        public void write_csv_with_copious_escaped_delimiters(bool forceDelimit)
        {
            var repeatCount = 1000000;

            using (var csvWriter = new CsvWriter(new StringWriter()))
            {
                csvWriter.ForceDelimit = forceDelimit;

                foreach (var record in Data.CopiousEscapedDelimitersValues.Repeat(repeatCount))
                {
                    csvWriter.WriteRecord(record);
                }

                Assert.Equal(repeatCount, csvWriter.RecordNumber);
            }
        }

        [WritePerformanceTest]
        public void write_csv_with_delimiters(bool forceDelimit)
        {
            var repeatCount = 100000;

            using (var csvWriter = new CsvWriter(new StringWriter()))
            {
                csvWriter.ForceDelimit = forceDelimit;

                foreach (var record in Data.DelimitedValues.Repeat(repeatCount))
                {
                    csvWriter.WriteRecord(record);
                }

                Assert.Equal(3 * repeatCount, csvWriter.RecordNumber);
            }
        }

        [WritePerformanceTest]
        public void write_csv_with_unnecessary_delimiters(bool forceDelimit)
        {
            var repeatCount = 200000;

            using (var csvWriter = new CsvWriter(new StringWriter()))
            {
                csvWriter.ForceDelimit = forceDelimit;

                foreach (var record in Data.UnnecessarilyDelimitedValues.Repeat(repeatCount))
                {
                    csvWriter.WriteRecord(record);
                }

                Assert.Equal(3 * repeatCount, csvWriter.RecordNumber);
            }
        }

        [WritePerformanceTest]
        public void write_stackoverflow_csv(bool forceDelimit)
        {
            var repeatCount = 10000;

            using (var csvWriter = new CsvWriter(new StringWriter()))
            {
                csvWriter.ForceDelimit = forceDelimit;

                foreach (var record in Data.StackoverflowValues.Repeat(repeatCount))
                {
                    csvWriter.WriteRecord(record);
                }

                Assert.Equal(20 * repeatCount, csvWriter.RecordNumber);
            }
        }
    }
}