using System;
using System.IO;
using System.Text;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
    /// <summary>
    /// Unit tests the <see cref="CsvParser"/> class, including individual tests for the rules defined in the design document.
    /// </summary>
    public sealed class CsvParserTest
    {
        private CsvParser _csvParser;

        public CsvParserTest()
        {
            CreateParser(string.Empty);
        }

        [Fact]
        public void TestPreserveLeadingWhiteSpace()
        {
            CreateParser(string.Format(" value1,value2   ,    value3   {0} value1,value2   ,    value3   ", Environment.NewLine));
            Assert.False(_csvParser.PreserveLeadingWhiteSpace);
            AssertRecord("value1", "value2", "value3");
            _csvParser.PreserveLeadingWhiteSpace = true;
            Assert.True(_csvParser.PreserveLeadingWhiteSpace);
            AssertRecord(" value1", "value2", "    value3");
        }

        [Fact]
        public void TestPreserveTrailingWhiteSpace()
        {
            CreateParser(string.Format(" value1,value2   ,    value3   {0} value1,value2   ,    value3   ", Environment.NewLine));
            Assert.False(_csvParser.PreserveTrailingWhiteSpace);
            AssertRecord("value1", "value2", "value3");
            _csvParser.PreserveTrailingWhiteSpace = true;
            Assert.True(_csvParser.PreserveTrailingWhiteSpace);
            AssertRecord("value1", "value2   ", "value3   ");
        }

        [Fact]
        public void TestPreserveAllWhiteSpace()
        {
            CreateParser(string.Format(" value1,value2   ,    value3   {0}value1  ,value 2 ,    value 3   ", Environment.NewLine));
            _csvParser.PreserveLeadingWhiteSpace = true;
            _csvParser.PreserveTrailingWhiteSpace = true;
            AssertRecord(" value1", "value2   ", "    value3   ");
            AssertRecord("value1  ", "value 2 ", "    value 3   ");
        }

        [Fact]
        public void empty_values_should_parse_correctly()
        {
            var csv = @" , 
 
   
""  """;

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var record = reader.ReadDataRecordAsStrings();
                Assert.Equal("", record[0]);
                Assert.Equal("", record[1]);

                record = reader.ReadDataRecordAsStrings();
                Assert.Equal("", record[0]);

                record = reader.ReadDataRecordAsStrings();
                Assert.Equal("", record[0]);

                record = reader.ReadDataRecordAsStrings();
                Assert.Equal("  ", record[0]);
            }
        }

        [Fact]
        public void TestValueDelimiterSameAsSeparator_DelimiterFirst()
        {
            _csvParser.ValueDelimiter = '-';
            var ex = Assert.Throws<ArgumentException>(() => _csvParser.ValueSeparator = '-');
            Assert.Equal("The value separator and value delimiter must be different.", ex.Message);
        }

        [Fact]
        public void TestValueSeparatorSpace()
        {
            var ex = Assert.Throws<ArgumentException>(() => _csvParser.ValueSeparator = ' ');
            Assert.Equal("Space is not a valid value separator or delimiter.", ex.Message);
        }

        [Fact]
        public void TestValueSeparator()
        {
            CreateParser(string.Format("value1,value2,value3{0}value1-value2-value3", Environment.NewLine));
            Assert.Equal(CsvParser.DefaultValueSeparator, _csvParser.ValueSeparator);
            AssertRecord("value1", "value2", "value3");
            _csvParser.ValueSeparator = '-';
            Assert.Equal('-', _csvParser.ValueSeparator);
            AssertRecord("value1", "value2", "value3");
        }

        [Fact]
        public void TestValueDelimiterSameAsSeparator_SeparatorFirst()
        {
            _csvParser.ValueSeparator = '-';
            var ex = Assert.Throws<ArgumentException>(() => _csvParser.ValueDelimiter = '-');
            Assert.Equal("The value separator and value delimiter must be different.", ex.Message);
        }

        [Fact]
        public void TestValueDelimiterSpace()
        {
            var ex = Assert.Throws<ArgumentException>(() => _csvParser.ValueDelimiter = ' ');
            Assert.Equal("Space is not a valid value separator or delimiter.", ex.Message);
        }

        [Fact]
        public void TestValueDelimiter()
        {
            CreateParser(string.Format("value1,\" value2  \",value3{0}value1,' value2  ',value3", Environment.NewLine));
            Assert.Equal(CsvParser.DefaultValueDelimiter, _csvParser.ValueDelimiter);
            AssertRecord("value1", " value2  ", "value3");
            _csvParser.ValueDelimiter = '\'';
            Assert.Equal('\'', _csvParser.ValueDelimiter);
            AssertRecord("value1", " value2  ", "value3");
        }

        [Fact]
        public void TestHasMoreRecords()
        {
            CreateParser(string.Format("value1, value2{0}value1, value2{0}value1, value2{0}", Environment.NewLine));
            Assert.True(_csvParser.HasMoreRecords);
            _csvParser.ParseRecord();
            Assert.True(_csvParser.HasMoreRecords);
            _csvParser.ParseRecord();
            Assert.True(_csvParser.HasMoreRecords);
            _csvParser.ParseRecord();
            Assert.False(_csvParser.HasMoreRecords);
            Assert.Null(_csvParser.ParseRecord());
        }

        [Fact]
        public void TestPassedFirstRecord()
        {
            CreateParser(string.Format("value1, value2{0}value1,value2", Environment.NewLine));
            Assert.False(_csvParser.PassedFirstRecord);
            _csvParser.ParseRecord();
            Assert.True(_csvParser.PassedFirstRecord);
            _csvParser.PassedFirstRecord = false;
            Assert.False(_csvParser.PassedFirstRecord);
            _csvParser.SkipRecord();
            Assert.True(_csvParser.PassedFirstRecord);
        }

        [Fact]
        public void TestConstructorReaderNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvParser = new CsvParser(null));
        }

        [Fact]
        public void TestSkipRecord()
        {
            CreateParser(string.Format("value1, value2{0}value1, value2{1}\"value1\", value2{0},value1, \"value \"\"2\"\" with suffix\"{0},value1, \"value \"\"2\"\"\"", Environment.NewLine, "\n"));
            Assert.True(_csvParser.HasMoreRecords);
            Assert.True(_csvParser.SkipRecord());
            Assert.True(_csvParser.HasMoreRecords);
            Assert.True(_csvParser.SkipRecord());
            Assert.True(_csvParser.HasMoreRecords);
            Assert.True(_csvParser.SkipRecord());
            Assert.True(_csvParser.HasMoreRecords);
            Assert.True(_csvParser.SkipRecord());
            Assert.True(_csvParser.HasMoreRecords);
            Assert.True(_csvParser.SkipRecord());
            Assert.False(_csvParser.HasMoreRecords);
            Assert.False(_csvParser.SkipRecord());
        }

        [Fact]
        public void TestParseRecord()
        {
            CreateParser(string.Format("value1, value2{0}value 1, value2{1}\"value1 \", \"value2\"{0}", Environment.NewLine, "\n"));
            Assert.True(_csvParser.HasMoreRecords);
            AssertRecord("value1", "value2");
            AssertRecord("value 1", "value2");
            AssertRecord("value1 ", "value2");
            Assert.False(_csvParser.HasMoreRecords);
        }

        [Fact]
        public void TestDispose()
        {
            CreateParser("value1,value2,value3");
            (_csvParser as IDisposable).Dispose();
            Assert.Throws<ObjectDisposedException>(() => _csvParser.ParseRecord());
        }

        [Fact]
        public void TestClose()
        {
            CreateParser("value1,value2,value3");
            _csvParser.Close();
            Assert.Throws<ObjectDisposedException>(() => _csvParser.ParseRecord());
        }

        [Fact]
        public void TestLargeComplexData()
        {
            //the idea of this test is really just to exercise as many code paths as possible
            //it's too hard to assert the actual results - that is left for more specific tests

            StringBuilder sb = new StringBuilder();
            //note the repeatable seed, chosen specifically because it yields 95% coverage
            Random random = new Random(4096);
            char valueSeparator = ',';
            char valueDelimiter = '"';

            for (int record = 0; record < 1500; ++record)
            {
                int fields = random.Next(1, 500);

                for (int field = 0; field < fields; ++field)
                {
                    bool delimitValue = (random.Next(0, 2) == 0);

                    if (delimitValue)
                    {
                        sb.Append(valueDelimiter);
                    }

                    //random leading whitespace
                    sb.Append(' ', random.Next(0, 150));

                    sb.Append("value").Append(field);

                    //random trailing whitespace
                    sb.Append(' ', random.Next(0, 150));

                    if (delimitValue)
                    {
                        sb.Append(valueDelimiter);
                    }

                    if (field < (fields - 1))
                    {
                        sb.Append(valueSeparator);
                    }
                }

                //randomly end the line
                int r = random.Next(0, 3);

                if (r == 0)
                {
                    sb.Append("\r");
                }
                else if (r == 1)
                {
                    sb.Append("\n");
                }
                else
                {
                    sb.Append("\r\n");
                }
            }

            //no whitespace preservation
            CreateParser(sb.ToString());
            _csvParser.ValueSeparator = valueSeparator;
            _csvParser.ValueDelimiter = valueDelimiter;

            while (_csvParser.HasMoreRecords)
            {
                string[] record = _csvParser.ParseRecord();

                for (int i = 0; i < record.Length; ++i)
                {
                    Assert.Equal("value" + i, record[i].Trim());
                }
            }

            //leading whitespace preservation
            CreateParser(sb.ToString());
            _csvParser.PreserveLeadingWhiteSpace = true;
            _csvParser.ValueSeparator = valueSeparator;
            _csvParser.ValueDelimiter = valueDelimiter;

            while (_csvParser.HasMoreRecords)
            {
                string[] record = _csvParser.ParseRecord();

                for (int i = 0; i < record.Length; ++i)
                {
                    Assert.Equal("value" + i, record[i].Trim());
                }
            }

            //trailing whitespace preservation
            CreateParser(sb.ToString());
            _csvParser.PreserveTrailingWhiteSpace = true;
            _csvParser.ValueSeparator = valueSeparator;
            _csvParser.ValueDelimiter = valueDelimiter;

            while (_csvParser.HasMoreRecords)
            {
                string[] record = _csvParser.ParseRecord();

                for (int i = 0; i < record.Length; ++i)
                {
                    Assert.Equal("value" + i, record[i].Trim());
                }
            }

            //leading and trailing whitespace preservation
            CreateParser(sb.ToString());
            _csvParser.PreserveLeadingWhiteSpace = true;
            _csvParser.PreserveTrailingWhiteSpace = true;
            _csvParser.ValueSeparator = valueSeparator;
            _csvParser.ValueDelimiter = valueDelimiter;

            while (_csvParser.HasMoreRecords)
            {
                string[] record = _csvParser.ParseRecord();

                for (int i = 0; i < record.Length; ++i)
                {
                    Assert.Equal("value" + i, record[i].Trim());
                }
            }
        }


        /**
         * Refer to the design documentation for the exact rules the following tests are asserting.
         */

        [Fact]
        public void TestRule_RS()
        {
            string csv = "Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}";
            string[] lineBreaks = {"\r\n", "\r", "\n"};

            foreach (string lineBreak in lineBreaks)
            {
                CreateParser(string.Format(csv, lineBreak));
                AssertRecord("Kent", "25", "M");
                AssertRecord("Belinda", "26", "F");
                AssertRecord("Tempany", "0", "F");
            }
        }

        [Fact]
        public void TestRule_RL()
        {
            string csv = "Kent,25,M,{0}Belinda,26,F{0}Tempany,0,F,{1}";
            string[] lineBreaks = {"\r\n", "\r", "\n"};
            string[] eofMarkers = {"\r\n", "\r", "\n", string.Empty};

            foreach (string lineBreak in lineBreaks)
            {
                foreach (string eofMarker in eofMarkers)
                {
                    CreateParser(string.Format(csv, lineBreak, eofMarker));
                    AssertRecord("Kent", "25", "M", "");
                    AssertRecord("Belinda", "26", "F");
                    AssertRecord("Tempany", "0", "F", "");
                }
            }
        }

        [Fact]
        public void TestRule_VS()
        {
            string csv = "Kent{0}25{0}M{1}Belinda{0}26{0}F{0}Description{1}Tempany{0}0{0}F{0}Description{0}Something else{1}";
            char[] separators = {',', '\t', ':', '.'};

            foreach (char separator in separators)
            {
                CreateParser(string.Format(csv, separator, Environment.NewLine));
                _csvParser.ValueSeparator = separator;
                AssertRecord("Kent", "25", "M");
                AssertRecord("Belinda", "26", "F", "Description");
                AssertRecord("Tempany", "0", "F", "Description", "Something else");
            }
        }

        [Fact]
        public void TestRule_VD()
        {
            string csv = "{0}Kent{0} {0}Boogaart{0},  {0}25{0}  ,M{1}{0}Belinda{0} {0}Boogaart{0},26,{0}F{0}{1}";
            char[] delimiters = {'"', '\'', ':', '-', '~', '_'};

            foreach (char delimiter in delimiters)
            {
                CreateParser(string.Format(csv, delimiter, Environment.NewLine));
                _csvParser.ValueDelimiter = delimiter;
                AssertRecord("Kent Boogaart", "25", "M");
                AssertRecord("Belinda Boogaart", "26", "F");
            }
        }

        [Fact]
        public void TestRule_VCW()
        {
            string csv = string.Format("{0}  Kent  {0},25  ,  M{1}{0}\tBelinda\t{0},26\t,\tF{1}", '"', Environment.NewLine);
            CreateParser(csv);
            AssertRecord("  Kent  ", "25", "M");
            AssertRecord("\tBelinda\t", "26", "F");
        }

        [Fact]
        public void TestRule_VCD()
        {
            string csv = "{0}Kent {0}{0}the man{0}{0} Boogaart{0}{1}{0}Belinda {0}{0}the babe{0}{0}{0}{1}";
            char[] delimiters = {'"', '\'', ':', '-', '~', '_'};

            foreach (char delimiter in delimiters)
            {
                CreateParser(string.Format(csv, delimiter, Environment.NewLine));
                _csvParser.ValueDelimiter = delimiter;
                AssertRecord(string.Format("Kent {0}the man{0} Boogaart", delimiter));
                AssertRecord(string.Format("Belinda {0}the babe{0}", delimiter));
            }
        }

        [Fact]
        public void TestRule_VCS()
        {
            string csv = "MUSE{0}'October{0} 2004'{1}Pearl Jam{0}'February{0} 2003'{1}Gomez{0}{1}";
            char[] separators = {',', '\t', ':', '.'};

            foreach (char separator in separators)
            {
                CreateParser(string.Format(csv, separator, Environment.NewLine));
                _csvParser.ValueDelimiter = '\'';
                _csvParser.ValueSeparator = separator;
                AssertRecord("MUSE", string.Format("October{0} 2004", separator));
                AssertRecord("Pearl Jam", string.Format("February{0} 2003", separator));
                AssertRecord("Gomez", string.Empty);
            }
        }

        [Fact]
        public void TestRule_VCB()
        {
            string csv = "Kent,A description of Kent{1}Belinda,'A description{0}of Belinda over two lines'{1}";
            string[] lineBreaks = {"\r\n", "\r", "\n"};

            foreach (string lineBreak in lineBreaks)
            {
                CreateParser(string.Format(csv, lineBreak, Environment.NewLine));
                _csvParser.ValueDelimiter = '\'';
                AssertRecord("Kent", "A description of Kent");
                AssertRecord("Belinda", string.Format("A description{0}of Belinda over two lines", lineBreak));
            }
        }

        private void CreateParser(string csv)
        {
            _csvParser = new CsvParser(new StringReader(csv));
        }

        private void AssertRecord(params string[] vals)
        {
            string[] record = _csvParser.ParseRecord();

            Assert.Equal(vals.Length, record.Length);
            
            for (int i = 0; i < record.Length; ++i)
            {
                Assert.Equal(vals[i], record[i]);
            }
        }
    }
}
