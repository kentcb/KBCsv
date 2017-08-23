namespace KBCsv.UnitTests.Internal
{
    using System.IO;
    using KBCsv.Internal;
    using Xunit;

    public sealed class LineTerminatorOverrideFixture
    {
        [Fact]
        public void override_parser_complies_with_2_1()
        {
            const char ValueSeparator = '\v';
            const char LineTerminatorOverride = '\f';
            var csv = $"a,aa{ValueSeparator}bbb{ValueSeparator}ccc{LineTerminatorOverride}zzz{ValueSeparator}yyy{ValueSeparator}x,xx{LineTerminatorOverride}";
            var parser = this.CreateParserFromString(csv);
            parser.LineTerminatorOverride = LineTerminatorOverride;
            parser.ValueSeparator = ValueSeparator;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("a,aa", records[0][0]);
            Assert.Equal("bbb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.Equal("zzz", records[1][0]);
            Assert.Equal("yyy", records[1][1]);
            Assert.Equal("x,xx", records[1][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void override_parser_complies_with_2_2()
        {
            const char LineTerminatorOverride = '\f';
            var csv = $"aaa,bbb,ccc{LineTerminatorOverride}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
            parser.LineTerminatorOverride = LineTerminatorOverride;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal("bbb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.Equal("zzz", records[1][0]);
            Assert.Equal("yyy", records[1][1]);
            Assert.Equal("xxx", records[1][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void override_parser_complies_with_2_3()
        {
            const char LineTerminatorOverride = '\f';
            var csv = $"field_name,field_name,field_name{LineTerminatorOverride}aaa,bbb,ccc{LineTerminatorOverride}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
            parser.LineTerminatorOverride = LineTerminatorOverride;
            var records = new DataRecord[3];

            Assert.Equal(3, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("field_name", records[0][0]);
            Assert.Equal("field_name", records[0][1]);
            Assert.Equal("field_name", records[0][2]);

            Assert.Equal("aaa", records[1][0]);
            Assert.Equal("bbb", records[1][1]);
            Assert.Equal("ccc", records[1][2]);

            Assert.Equal("zzz", records[2][0]);
            Assert.Equal("yyy", records[2][1]);
            Assert.Equal("xxx", records[2][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void override_parser_complies_with_2_4()
        {
            const char ValueSeparator = '\v';
            var csv = $"aaa{ValueSeparator}bbb{ValueSeparator}ccc";
            var parser = this.CreateParserFromString(csv);
            parser.ValueSeparator = ValueSeparator;
            var records = new DataRecord[1];

            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal("bbb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void override_parser_complies_with_2_5()
        {
            const char LineTerminatorOverride = '\f';
            var csv = $"\"aaa\",\"bbb\",\"ccc\"{LineTerminatorOverride}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
            parser.LineTerminatorOverride = LineTerminatorOverride;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal("bbb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.Equal("zzz", records[1][0]);
            Assert.Equal("yyy", records[1][1]);
            Assert.Equal("xxx", records[1][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void override_parser_complies_with_2_6()
        {
            const char LineTerminatorOverride = '\f';
            var csv = $"\"aaa\",\"b{LineTerminatorOverride}bb\",\"ccc\"{LineTerminatorOverride}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
            parser.LineTerminatorOverride = LineTerminatorOverride;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal($"b{LineTerminatorOverride}bb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.Equal("zzz", records[1][0]);
            Assert.Equal("yyy", records[1][1]);
            Assert.Equal("xxx", records[1][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void override_parser_complies_with_2_7()
        {
            const char ValueSeparator = '\v';
            var csv = @"""aaa"",""b""""bb"",""ccc""";
            csv = csv.Replace(',', ValueSeparator);
            var parser = this.CreateParserFromString(csv);
            parser.ValueSeparator = ValueSeparator;
            var records = new DataRecord[1];

            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal(@"b""bb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void override_skip_records_skips_the_specified_number_of_records()
        {
            const char LineTerminatorOverride = '\f';
            var csv = $"first{LineTerminatorOverride}second{LineTerminatorOverride}third{LineTerminatorOverride}fourth";
            var parser = this.CreateParserFromString(csv);
            parser.LineTerminatorOverride = LineTerminatorOverride;
            Assert.Equal(2, parser.SkipRecords(2));
            var records = new DataRecord[1];
            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));
            Assert.Equal("third", records[0][0]);
        }
        #region Supporting Members

        private CsvParser CreateParserFromString(string csv)
        {
            return new CsvParser(new StringReader(csv));
        }

        #endregion
    }
}