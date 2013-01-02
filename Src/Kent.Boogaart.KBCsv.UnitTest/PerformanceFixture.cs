using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kent.Boogaart.KBCsv.UnitTest.Utility;
using Xunit;
using Xunit.Extensions;

namespace Kent.Boogaart.KBCsv.UnitTest
{
    public sealed class PerformanceFixture
    {
[Fact(Skip = "Temporary test")]
public void compare_old_to_new()
{
    var repeatCount = 100000;

    using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
    using (var csvReader = new CsvReader(textReader))
    {
        while (csvReader.HasMoreRecords)
        {
            csvReader.ReadDataRecordAsStrings();
        }

        Assert.Equal(20 * repeatCount, csvReader.RecordNumber);
    }

    using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
    using (var csvReader = new CsvReader(textReader))
    {
        var stopwatch = Stopwatch.StartNew();

        while (csvReader.HasMoreRecords)
        {
            csvReader.ReadDataRecordAsStrings();
        }

        stopwatch.Stop();

        Assert.Equal(20 * repeatCount, csvReader.RecordNumber);

        Console.WriteLine("Done: {0}ms", stopwatch.ElapsedMilliseconds);
    }
}

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void read_plain_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.PlainData.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void skip_plain_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.PlainData.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void read_csv_with_copious_whitespace(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousWhitespace.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void skip_csv_with_copious_whitespace(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousWhitespace.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void read_csv_with_copious_escaped_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousEscapedDelimiters.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void skip_csv_with_copious_escaped_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousEscapedDelimiters.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void read_stackoverflow_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 30000;

            using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void skip_stackoverflow_csv(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 30000;

            using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void read_csv_with_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.DelimitedData.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void skip_csv_with_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.DelimitedData.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void read_csv_with_unnecessary_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.UnnecessarilyDelimitedData.Repeat(repeatCount)))
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

        [ReadPerformanceTest(Skip = "Performance tests skipped by default.")]
        public void skip_csv_with_unnecessary_delimiters(WhiteSpacePreservation whiteSpacePreservation)
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.UnnecessarilyDelimitedData.Repeat(repeatCount)))
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

        #region Supporting Members

        private IEnumerable<string> PlainData
        {
            get
            {
                yield return "Kent,Boogaart,33,Sidcup" + Environment.NewLine;
                yield return "Belinda,Boogaart,34,Sidcup" + Environment.NewLine;
                yield return "Tempany,Boogaart,8,Sidcup" + Environment.NewLine;
            }
        }

        private IEnumerable<string> CopiousWhitespace
        {
            get
            {
                yield return "        abc        ,              def,ghi            " + Environment.NewLine;
                yield return "\t\t\t\tabc\t\t\t\t,\t\t\t\t\t\t\tdef,ghi\t\t\t\t\t\t" + Environment.NewLine;
                yield return "\t \t   abc    \t  ,  \t     \t   def,ghi\t        \t" + Environment.NewLine;
            }
        }

        private IEnumerable<string> CopiousEscapedDelimiters
        {
            get
            {
                yield return "\"\"\"\"\"\"\"\"\"\"" + Environment.NewLine;
            }
        }

        private IEnumerable<string> DelimitedData
        {
            get
            {
                yield return @"""She said ""hello"""",""first,second,third"",""a value over
two lines""" + Environment.NewLine;
                yield return @"""He said ""hi there"""",""fourth,fifth,sixth"",""a value over
three
lines""" + Environment.NewLine;
                yield return @"""She said ""what's up?"""",""seventh,eighth,ninth,tenth"",""a value
over
four
lines""" + Environment.NewLine;
            }
        }

        private IEnumerable<string> UnnecessarilyDelimitedData
        {
            get
            {
                yield return @"""first"",""second"",""third fourth fifth""" + Environment.NewLine;
                yield return @"""1"",""2"",""3 4 5""" + Environment.NewLine;
                yield return @"""abc"",""def"",""ghijklmnoopqrstuvwxyz - 0123456789""" + Environment.NewLine;
            }
        }

        private IEnumerable<string> StackoverflowData
        {
            get
            {
                yield return @"""1"",563355,62701,0,1235000081,""php,error,gd,image-processing"",220,2,563372,67183,2,1235000501" + Environment.NewLine;
                yield return @"""2"",563355,62701,0,1235000081,""php,error,gd,image-processing"",220,2,563374,66554,0,1235000551" + Environment.NewLine;
                yield return @"""3"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,563358,15842,3,1235000177" + Environment.NewLine;
                yield return @"""4"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,563413,893,18,1235001545" + Environment.NewLine;
                yield return @"""5"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,563454,11649,4,1235002457" + Environment.NewLine;
                yield return @"""6"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,563472,50742,6,1235002809" + Environment.NewLine;
                yield return @"""7"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,563484,8899,1,1235003266" + Environment.NewLine;
                yield return @"""8"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,563635,60190,12,1235007817" + Environment.NewLine;
                yield return @"""9"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,563642,65235,1,1235007913" + Environment.NewLine;
                yield return @"""10"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,564028,32797,8,1235020626" + Environment.NewLine;
                yield return @"""11"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,564747,19619,1,1235040652" + Environment.NewLine;
                yield return @"""12"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,568102,10728,3,1235098147" + Environment.NewLine;
                yield return @"""13"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,568213,68757,5,1235101761" + Environment.NewLine;
                yield return @"""14"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,568221,26177,0,1235102039" + Environment.NewLine;
                yield return @"""15"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,568975,31141,0,1235124207" + Environment.NewLine;
                yield return @"""16"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,569399,21734,3,1235132888" + Environment.NewLine;
                yield return @"""17"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,931176,114979,0,1243739978" + Environment.NewLine;
                yield return @"""18"",563356,15842,10,1235000140,""lisp,scheme,subjective,clojure"",1047,16,931214,82368,1,1243741787" + Environment.NewLine;
                yield return @"""19"",563365,68122,0,1235000369,""cocoa-touch,objective-c,design-patterns"",108,3,563376,68122,0,1235000573" + Environment.NewLine;
                yield return @"""20"",563365,68122,0,1235000369,""cocoa-touch,objective-c,design-patterns"",108,3,563379,66344,2,1235000607" + Environment.NewLine;
            }
        }

        // TODO:
        // delimited data
        // data delimited unnecessarily
        // complex data (with multi-lines, delimited, whitespace et cetera)
        // do a skip permutation for all tests

        #endregion
    }
}
