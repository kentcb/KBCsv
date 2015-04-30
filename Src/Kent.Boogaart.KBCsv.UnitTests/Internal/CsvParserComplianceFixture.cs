namespace Kent.Boogaart.KBCsv.UnitTests.Internal
{
    using System;
    using System.IO;
    using Kent.Boogaart.KBCsv.Internal;
    using Xunit;

    // refer to the documentation for the exact rules the following tests are asserting
    public sealed class CsvParserComplianceFixture
    {
        [Fact]
        public void parser_complies_with_rule_RS()
        {
            var csv = "Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}";
            string[] lineBreaks = { "\r\n", "\r", "\n" };

            foreach (var lineBreak in lineBreaks)
            {
                var parser = this.CreateParserFromString(string.Format(csv, lineBreaks));
                var records = new DataRecord[3];

                Assert.Equal(3, parser.ParseRecords(null, records, 0, records.Length));

                Assert.Equal("Kent", records[0][0]);
                Assert.Equal("25", records[0][1]);
                Assert.Equal("M", records[0][2]);

                Assert.Equal("Belinda", records[1][0]);
                Assert.Equal("26", records[1][1]);
                Assert.Equal("F", records[1][2]);

                Assert.Equal("Tempany", records[2][0]);
                Assert.Equal("0", records[2][1]);
                Assert.Equal("F", records[2][2]);

                Assert.False(parser.HasMoreRecords);
            }
        }

        [Fact]
        public void parser_complies_with_rule_RL()
        {
            var csv = "Kent,25,M,{0}Belinda,26,F{0}Tempany,0,F,{1}";
            string[] lineBreaks = { "\r\n", "\r", "\n" };
            string[] eofMarkers = { "\r\n", "\r", "\n", string.Empty };

            foreach (var lineBreak in lineBreaks)
            {
                foreach (var eofMarker in eofMarkers)
                {
                    var parser = this.CreateParserFromString(string.Format(csv, lineBreak, eofMarker));
                    var records = new DataRecord[3];

                    Assert.Equal(3, parser.ParseRecords(null, records, 0, records.Length));

                    Assert.Equal("Kent", records[0][0]);
                    Assert.Equal("25", records[0][1]);
                    Assert.Equal("M", records[0][2]);
                    Assert.Equal("", records[0][3]);

                    Assert.Equal("Belinda", records[1][0]);
                    Assert.Equal("26", records[1][1]);
                    Assert.Equal("F", records[1][2]);

                    Assert.Equal("Tempany", records[2][0]);
                    Assert.Equal("0", records[2][1]);
                    Assert.Equal("F", records[2][2]);
                    Assert.Equal("", records[2][3]);

                    Assert.False(parser.HasMoreRecords);
                }
            }
        }

        [Fact]
        public void parser_complies_with_rule_VS()
        {
            var csv = "Kent{0}25{0}M{1}Belinda{0}26{0}F{0}Description{1}Tempany{0}10{0}F{0}Description{0}Something else{1}Xak{0}2{0}M{0}{1}";
            char[] separators = { ',', '\t', ':', '.' };

            foreach (var separator in separators)
            {
                var parser = this.CreateParserFromString(string.Format(csv, separator, Environment.NewLine));
                var records = new DataRecord[5];
                parser.ValueSeparator = separator;

                Assert.Equal(4, parser.ParseRecords(null, records, 0, records.Length));

                Assert.Equal("Kent", records[0][0]);
                Assert.Equal("25", records[0][1]);
                Assert.Equal("M", records[0][2]);

                Assert.Equal("Belinda", records[1][0]);
                Assert.Equal("26", records[1][1]);
                Assert.Equal("F", records[1][2]);
                Assert.Equal("Description", records[1][3]);

                Assert.Equal("Tempany", records[2][0]);
                Assert.Equal("10", records[2][1]);
                Assert.Equal("F", records[2][2]);
                Assert.Equal("Description", records[2][3]);
                Assert.Equal("Something else", records[2][4]);

                Assert.Equal("Xak", records[3][0]);
                Assert.Equal("2", records[3][1]);
                Assert.Equal("M", records[3][2]);
                Assert.Equal("", records[3][3]);

                Assert.False(parser.HasMoreRecords);
            }
        }

        [Fact]
        public void parser_complies_with_rule_VD()
        {
            var csv = "{0}Kent{0} {0}Boogaart{0},  {0}25{0}  ,M{1}{0}Belinda{0} {0}Boogaart{0},26,{0}F{0}{1}";
            char[] delimiters = { '"', '\'', ':', '-', '~', '_' };

            foreach (var delimiter in delimiters)
            {
                var parser = this.CreateParserFromString(string.Format(csv, delimiter, Environment.NewLine));
                var records = new DataRecord[2];
                parser.ValueDelimiter = delimiter;

                Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

                Assert.Equal("Kent Boogaart", records[0][0]);
                Assert.Equal("25", records[0][1]);
                Assert.Equal("M", records[0][2]);

                Assert.Equal("Belinda Boogaart", records[1][0]);
                Assert.Equal("26", records[1][1]);
                Assert.Equal("F", records[1][2]);

                Assert.False(parser.HasMoreRecords);
            }
        }

        [Fact]
        public void parser_complies_with_rule_VCW()
        {
            var csv = string.Format("{0}  Kent  {0},25  ,  M{1}{0}\tBelinda\t{0},26\t,\tF{1}", '"', Environment.NewLine);
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("  Kent  ", records[0][0]);
            Assert.Equal("25", records[0][1]);
            Assert.Equal("M", records[0][2]);

            Assert.Equal("\tBelinda\t", records[1][0]);
            Assert.Equal("26", records[1][1]);
            Assert.Equal("F", records[1][2]);

            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void parser_complies_with_rule_VCD()
        {
            var csv = "{0}Kent {0}{0}the man{0}{0} Boogaart{0}{1}{0}Belinda {0}{0}the babe{0}{0}{0}{1}";
            char[] delimiters = { '"', '\'', ':', '-', '~', '_' };

            foreach (var delimiter in delimiters)
            {
                var parser = this.CreateParserFromString(string.Format(csv, delimiter, Environment.NewLine));
                var records = new DataRecord[2];
                parser.ValueDelimiter = delimiter;

                Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

                Assert.Equal(string.Format("Kent {0}the man{0} Boogaart", delimiter), records[0][0]);

                Assert.Equal(string.Format("Belinda {0}the babe{0}", delimiter), records[1][0]);

                Assert.False(parser.HasMoreRecords);
            }
        }

        [Fact]
        public void parser_complies_with_rule_VCS()
        {
            var csv = "MUSE{0}'October{0} 2004'{1}Pearl Jam{0}'February{0} 2003'{1}Gomez{0}{1}";
            char[] separators = { ',', '\t', ':', '.' };

            foreach (var separator in separators)
            {
                var parser = this.CreateParserFromString(string.Format(csv, separator, Environment.NewLine));
                var records = new DataRecord[3];
                parser.ValueDelimiter = '\'';
                parser.ValueSeparator = separator;

                Assert.Equal(3, parser.ParseRecords(null, records, 0, records.Length));

                Assert.Equal("MUSE", records[0][0]);
                Assert.Equal(string.Format("October{0} 2004", separator), records[0][1]);

                Assert.Equal("Pearl Jam", records[1][0]);
                Assert.Equal(string.Format("February{0} 2003", separator), records[1][1]);

                Assert.Equal("Gomez", records[2][0]);
                Assert.Equal("", records[2][1]);

                Assert.False(parser.HasMoreRecords);
            }
        }

        [Fact]
        public void parser_complies_with_rule_VCB()
        {
            var csv = "Kent,A description of Kent{1}Belinda,'A description{0}of Belinda over two lines'{1}";
            string[] lineBreaks = { "\r\n", "\r", "\n" };

            foreach (var lineBreak in lineBreaks)
            {
                var parser = this.CreateParserFromString(string.Format(csv, lineBreaks));
                var records = new DataRecord[2];
                parser.ValueDelimiter = '\'';

                Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

                Assert.Equal("Kent", records[0][0]);
                Assert.Equal("A description of Kent", records[0][1]);

                Assert.Equal("Belinda", records[1][0]);
                Assert.Equal(@"A description
of Belinda over two lines", records[1][1]);

                Assert.False(parser.HasMoreRecords);
            }
        }

        #region Supporting Members

        private CsvParser CreateParserFromString(string csv)
        {
            return new CsvParser(new StringReader(csv));
        }

        #endregion
    }
}
