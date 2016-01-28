namespace KBCsv.PerformanceTests
{
    using KBCsv.PerformanceTests.Utility;
    using Xunit;

    public sealed class ReadPerformanceFixture
    {
        [ReadPerformanceTest]
        public void read_plain_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.PlainDataLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void read_csv_with_copious_whitespace(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.CopiousWhiteSpaceLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void read_csv_with_copious_escaped_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.CopiousEscapedDelimitersLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void read_csv_with_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.DelimitedLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void read_csv_with_unnecessary_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.UnnecessarilyDelimitedLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void read_stackoverflow_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 30000;

            using (var textReader = new EnumerableStringReader(Data.StackoverflowLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(20 * repeatCount, csvReader.RecordNumber);
            }
        }
    }
}
