namespace KBCsv.UnitTests.Internal
{
    using System.IO;
    using KBCsv.Internal;
    using Xunit;

    // see https://tools.ietf.org/html/rfc4180
    public sealed class CsvParserRfc4180ComplianceFixture
    {
        [Fact]
        public void parser_complies_with_2_1()
        {
            var CRLF = "\r\n";
            var csv = $"aaa,bbb,ccc{CRLF}zzz,yyy,xxx{CRLF}";
            var parser = this.CreateParserFromString(csv);
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
        public void parser_complies_with_2_2()
        {
            var CRLF = "\r\n";
            var csv = $"aaa,bbb,ccc{CRLF}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
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
        public void parser_complies_with_2_3()
        {
            var CRLF = "\r\n";
            var csv = $"field_name,field_name,field_name{CRLF}aaa,bbb,ccc{CRLF}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
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
        public void parser_complies_with_2_4()
        {
            var csv = $"aaa,bbb,ccc";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[1];

            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal("bbb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void parser_complies_with_2_5()
        {
            var CRLF = "\r\n";
            var csv = $"\"aaa\",\"bbb\",\"ccc\"{CRLF}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
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
        public void parser_complies_with_2_6()
        {
            var CRLF = "\r\n";
            var csv = $"\"aaa\",\"b{CRLF}bb\",\"ccc\"{CRLF}zzz,yyy,xxx";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal($"b{CRLF}bb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.Equal("zzz", records[1][0]);
            Assert.Equal("yyy", records[1][1]);
            Assert.Equal("xxx", records[1][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void parser_complies_with_2_7()
        {
            var csv = @"""aaa"",""b""""bb"",""ccc""";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[1];

            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("aaa", records[0][0]);
            Assert.Equal(@"b""bb", records[0][1]);
            Assert.Equal("ccc", records[0][2]);

            Assert.False(parser.HasMoreRecords);
        }

        #region Supporting Members

        private CsvParser CreateParserFromString(string csv)
        {
            return new CsvParser(new StringReader(csv));
        }

        #endregion
    }
}