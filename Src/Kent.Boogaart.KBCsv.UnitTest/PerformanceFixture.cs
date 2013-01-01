using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kent.Boogaart.KBCsv.UnitTest.Utility;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
    public sealed class PerformanceFixture
    {
[PerformanceTest]
public void stackoverflow_real()
{
    using (var csvReader = new CsvReader(@"C:\Repository\KBCsv\trunk\Src\Kent.Boogaart.KBCsv.UnitTest\StackoverflowAnswers.csv"))
    {
        while (csvReader.HasMoreRecords)
        {
            csvReader.ReadDataRecord();
        }

        Assert.Equal(263541, csvReader.RecordNumber);
    }
}

[Fact]
public void compare_old_to_new()
{
    var repeatCount = 200000;

    using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
    using (var csvReader = new CsvReader(textReader))
    {
        while (csvReader.HasMoreRecords)
        {
            csvReader.ReadDataRecord();
        }

        Assert.Equal(20 * repeatCount, csvReader.RecordNumber);
    }

    using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
    using (var csvReader = new CsvReader(textReader))
    {
        var stopwatch = Stopwatch.StartNew();

        while (csvReader.HasMoreRecords)
        {
            csvReader.ReadDataRecord();
        }

        stopwatch.Stop();

        Assert.Equal(20 * repeatCount, csvReader.RecordNumber);

        Console.WriteLine("Done: {0}ms", stopwatch.ElapsedMilliseconds);
    }
}

        [PerformanceTest]
        public void read_stackoverflow_data()
        {
            var repeatCount = 30000;

            using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(20 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void skip_stackoverflow_data()
        {
            var repeatCount = 200000;

            using (var textReader = new EnumerableStringReader(this.StackoverflowData.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                while (csvReader.HasMoreRecords)
                {
                    csvReader.SkipRecord();
                }

                Assert.Equal(20 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_plain_csv_all_whitespace_unpreserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.PlainData.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_plain_csv_leading_whitespace_preserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.PlainData.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = true;

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_plain_csv_trailing_whitespace_preserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.PlainData.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveTrailingWhiteSpace = true;

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_plain_csv_all_whitespace_preserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.PlainData.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = true;
                csvReader.PreserveTrailingWhiteSpace = true;
                
                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_csv_with_copious_all_whitespace_unpreserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousWhitespace.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_csv_with_copious_leading_whitespace_preserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousWhitespace.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = true;

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_csv_with_copious_trailing_whitespace_preserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousWhitespace.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveTrailingWhiteSpace = true;

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
                }

                Assert.Equal(3 * repeatCount, csvReader.RecordNumber);
            }
        }

        [PerformanceTest]
        public void read_csv_with_copious_all_whitespace_preserved()
        {
            var repeatCount = 100000;

            using (var textReader = new EnumerableStringReader(this.CopiousWhitespace.Repeat(repeatCount)))
            using (var csvReader = new CsvReader(textReader))
            {
                csvReader.PreserveLeadingWhiteSpace = true;
                csvReader.PreserveTrailingWhiteSpace = true;

                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecord();
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
        // complex data (with multi-lines, delimited, whitespace et cetera)
        // do a skip permutation for all tests

        #endregion
    }
}
