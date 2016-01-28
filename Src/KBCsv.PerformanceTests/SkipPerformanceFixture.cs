namespace KBCsv.PerformanceTests
{
    using KBCsv.PerformanceTests.Utility;
    using Xunit;

    public sealed class SkipPerformanceFixture
    {
        [ReadPerformanceTest]
        public void skip_plain_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.PlainDataLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.SkipRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void skip_csv_with_copious_whitespace(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.CopiousWhiteSpaceLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.SkipRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void skip_csv_with_copious_escaped_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.CopiousEscapedDelimitersLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.SkipRecord();
                }

                Assert.Equal(repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void skip_csv_with_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.DelimitedLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.SkipRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void skip_csv_with_unnecessary_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(Data.UnnecessarilyDelimitedLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.SkipRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [ReadPerformanceTest]
        public void skip_stackoverflow_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 30000;

            using (var textReader = new EnumerableStringReader(Data.StackoverflowLines.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Leading);
                csvReader.PreserveTrailingWhiteSpace = whiteSpacePreservation.HasFlag(WhiteSpacePreservation.Trailing);

                while (csvReader.HasMoreRecords)
                {
                    csvReader.SkipRecord();
                }

                Assert.Equal(20 * repeatCount, csvReader.RecordNumber);
            }
        }
    }
}
