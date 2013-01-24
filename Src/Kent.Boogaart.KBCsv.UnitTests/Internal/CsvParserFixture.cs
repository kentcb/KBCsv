namespace Kent.Boogaart.KBCsv.UnitTests.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using Kent.Boogaart.KBCsv.Internal;
    using Xunit;

    // NOTE: you may want View White Space turned on, so you can differentiate spaces and tabs in the CSV
    public sealed class CsvParserFixture
    {
        [Fact]
        public void leading_whitespace_is_discarded_when_preserve_leading_whitespace_is_false()
        {
            var csv = @" value1,  	value2,   	     		 value3
value4,   value5,	value6";
            var parser = this.CreateParserFromString(csv);
            parser.PreserveLeadingWhiteSpace = false;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("value1", records[0][0]);
            Assert.Equal("value2", records[0][1]);
            Assert.Equal("value3", records[0][2]);
            Assert.Equal("value4", records[1][0]);
            Assert.Equal("value5", records[1][1]);
            Assert.Equal("value6", records[1][2]);
        }

        [Fact]
        public void leading_whitespace_is_retained_when_preserve_leading_whitespace_is_true()
        {
            var csv = @" value1,  	value2,   	     		 value3
value4,   value5,	value6";
            var parser = this.CreateParserFromString(csv);
            parser.PreserveLeadingWhiteSpace = true;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal(" value1", records[0][0]);
            Assert.Equal("  	value2", records[0][1]);
            Assert.Equal("   	     		 value3", records[0][2]);
            Assert.Equal("value4", records[1][0]);
            Assert.Equal("   value5", records[1][1]);
            Assert.Equal("	value6", records[1][2]);
        }

        [Fact]
        public void trailing_whitespace_is_discarded_when_preserve_trailing_whitespace_is_false()
        {
            var csv = @"value1 ,value2  	,value3   	     		 
value4,value5   ,value6	";
            var parser = this.CreateParserFromString(csv);
            parser.PreserveTrailingWhiteSpace = false;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("value1", records[0][0]);
            Assert.Equal("value2", records[0][1]);
            Assert.Equal("value3", records[0][2]);
            Assert.Equal("value4", records[1][0]);
            Assert.Equal("value5", records[1][1]);
            Assert.Equal("value6", records[1][2]);
        }

        [Fact]
        public void trailing_whitespace_is_retained_when_preserve_trailing_whitespace_is_true()
        {
            var csv = @"value1 ,value2  	,value3   	     		 
value4,value5   ,value6	";
            var parser = this.CreateParserFromString(csv);
            parser.PreserveTrailingWhiteSpace = true;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("value1 ", records[0][0]);
            Assert.Equal("value2  	", records[0][1]);
            Assert.Equal("value3   	     		 ", records[0][2]);
            Assert.Equal("value4", records[1][0]);
            Assert.Equal("value5   ", records[1][1]);
            Assert.Equal("value6	", records[1][2]);
        }

        [Fact]
        public void leading_and_trailing_whitespace_are_discarded_when_preserve_leading_and_trailing_whitespace_are_both_false()
        {
            var csv = @" value1 ,  	value2  	,   	     		 value3   	     		 
value4,   value5   ,	value6	";
            var parser = this.CreateParserFromString(csv);
            parser.PreserveLeadingWhiteSpace = false;
            parser.PreserveTrailingWhiteSpace = false;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("value1", records[0][0]);
            Assert.Equal("value2", records[0][1]);
            Assert.Equal("value3", records[0][2]);
            Assert.Equal("value4", records[1][0]);
            Assert.Equal("value5", records[1][1]);
            Assert.Equal("value6", records[1][2]);
        }

        [Fact]
        public void leading_and_trailing_whitespace_are_retained_when_preserve_leading_and_trailing_whitespace_are_both_true()
        {
            var csv = @" value1 ,  	value2  	,   	     		 value3   	     		 
value4,   value5   ,	value6	";
            var parser = this.CreateParserFromString(csv);
            parser.PreserveLeadingWhiteSpace = true;
            parser.PreserveTrailingWhiteSpace = true;
            var records = new DataRecord[2];

            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal(" value1 ", records[0][0]);
            Assert.Equal("  	value2  	", records[0][1]);
            Assert.Equal("   	     		 value3   	     		 ", records[0][2]);
            Assert.Equal("value4", records[1][0]);
            Assert.Equal("   value5   ", records[1][1]);
            Assert.Equal("	value6	", records[1][2]);
        }

        [Fact]
        public void empty_string_does_not_parse_to_a_record()
        {
            var parser = this.CreateParserFromString(string.Empty);
            var records = new DataRecord[1];
            Assert.Equal(0, parser.ParseRecords(null, records, 0, records.Length));
        }


        [Fact]
        public void empty_values_parse_correctly()
        {
            var csv = @" , 
         
           
        ""  """;
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[4];
            Assert.Equal(4, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("", records[0][0]);
            Assert.Equal("", records[0][1]);

            Assert.Equal("", records[1][0]);

            Assert.Equal("", records[2][0]);

            Assert.Equal("  ", records[3][0]);
        }

        [Fact]
        public void value_delimiter_throws_if_being_set_to_same_value_as_value_separator()
        {
            var parser = this.CreateParserFromString(string.Empty);
            parser.ValueSeparator = '-';
            var ex = Assert.Throws<ArgumentException>(() => parser.ValueDelimiter = '-');
            Assert.Equal("Value separator and delimiter cannot be the same.", ex.Message);
        }

        [Fact]
        public void value_separator_throws_if_being_set_to_same_value_as_value_delimiter()
        {
            var parser = this.CreateParserFromString(string.Empty);
            parser.ValueDelimiter = '-';
            var ex = Assert.Throws<ArgumentException>(() => parser.ValueSeparator = '-');
            Assert.Equal("Value separator and delimiter cannot be the same.", ex.Message);
        }

        [Fact]
        public void value_delimiter_can_be_used_to_alter_the_character_that_delimits_values()
        {
            var csv = @"'   value1  ', 'value2''','value	3	'";
            var parser = this.CreateParserFromString(csv);
            parser.ValueDelimiter = '\'';
            var records = new DataRecord[1];

            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("   value1  ", records[0][0]);
            Assert.Equal("value2'", records[0][1]);
            Assert.Equal("value	3	", records[0][2]);
        }

        [Fact]
        public void value_separator_can_be_used_to_alter_the_character_that_separates_values()
        {
            var csv = @"value1	value2		value4";
            var parser = this.CreateParserFromString(csv);
            parser.ValueSeparator = '\t';
            var records = new DataRecord[1];

            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("value1", records[0][0]);
            Assert.Equal("value2", records[0][1]);
            Assert.Equal("", records[0][2]);
            Assert.Equal("value4", records[0][3]);
        }

        [Fact]
        public void has_more_records_returns_true_if_there_are_more_records()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[2];

            Assert.True(parser.HasMoreRecords);
            parser.ParseRecords(null, records, 0, records.Length);
            Assert.True(parser.HasMoreRecords);
            parser.ParseRecords(null, records, 0, records.Length);
            Assert.True(parser.HasMoreRecords);
            parser.ParseRecords(null, records, 0, records.Length);
            Assert.True(parser.HasMoreRecords);
        }

        [Fact]
        public void has_more_records_returns_false_if_there_are_no_more_records()
        {
            var csv = @"first
second
third";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[2];

            Assert.True(parser.HasMoreRecords);
            parser.ParseRecords(null, records, 0, records.Length);
            Assert.True(parser.HasMoreRecords);
            parser.ParseRecords(null, records, 0, records.Length);
            Assert.False(parser.HasMoreRecords);
        }

        [Fact]
        public void skip_records_returns_zero_if_there_are_no_records_to_skip()
        {
            var parser = this.CreateParserFromString(string.Empty);
            Assert.Equal(0, parser.SkipRecords(1));
            Assert.Equal(0, parser.SkipRecords(100));
        }

        [Fact]
        public void skip_records_returns_zero_if_there_is_no_more_data_left()
        {
            var csv = @"first
second";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[2];
            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));
            Assert.Equal(0, parser.SkipRecords(1));
            Assert.Equal(0, parser.SkipRecords(100));
        }

        [Fact]
        public void skip_records_skips_the_specified_number_of_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(2, parser.SkipRecords(2));
            var records = new DataRecord[1];
            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));
            Assert.Equal("third", records[0][0]);
        }

        [Fact]
        public void skip_records_stops_skipping_if_it_cannot_skip_any_more_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(4, parser.SkipRecords(100));
        }

        [Fact]
        public void skip_records_works_with_complex_data()
        {
            var csv = @"value0,  value1   ,		value2	   	,'   value3  	 ''something in value3
over two lines''', value4 !23*&(#$@#*&#$)&][}{}{;.<>/?, value5
second record,foo,bar,biz,baz,fuz,fiz,'faz,fuz,buz'
third
fourth";
            var parser = this.CreateParserFromString(csv);
            parser.ValueDelimiter = '\'';
            Assert.Equal(2, parser.SkipRecords(2));
            var records = new DataRecord[1];
            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));
            Assert.Equal("third", records[0][0]);
        }

        [Fact]
        public async void skip_records_async_returns_zero_if_there_are_no_records_to_skip()
        {
            var parser = this.CreateParserFromString(string.Empty);
            Assert.Equal(0, await parser.SkipRecordsAsync(1));
            Assert.Equal(0, await parser.SkipRecordsAsync(100));
        }

        [Fact]
        public async void skip_records_async_returns_zero_if_there_is_no_more_data_left()
        {
            var csv = @"first
second";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[2];
            Assert.Equal(2, parser.ParseRecords(null, records, 0, records.Length));
            Assert.Equal(0, await parser.SkipRecordsAsync(1));
            Assert.Equal(0, await parser.SkipRecordsAsync(100));
        }

        [Fact]
        public async void skip_records_async_skips_the_specified_number_of_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(2, await parser.SkipRecordsAsync(2));
            var records = new DataRecord[1];
            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));
            Assert.Equal("third", records[0][0]);
        }

        [Fact]
        public async void skip_records_async_stops_skipping_if_it_cannot_skip_any_more_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(4, await parser.SkipRecordsAsync(100));
        }

        [Fact]
        public async void skip_records_async_works_with_complex_data()
        {
            var csv = @"value0,  value1   ,		value2	   	,'   value3  	 ''something in value3
over two lines''', value4 !23*&(#$@#*&#$)&][}{}{;.<>/?, value5
second record,foo,bar,biz,baz,fuz,fiz,'faz,fuz,buz'
third
fourth";
            var parser = this.CreateParserFromString(csv);
            parser.ValueDelimiter = '\'';
            Assert.Equal(2, await parser.SkipRecordsAsync(2));
            var records = new DataRecord[1];
            Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));
            Assert.Equal("third", records[0][0]);
        }

        [Fact]
        public void parse_records_returns_zero_if_there_are_no_more_records_to_parse()
        {
            var parser = this.CreateParserFromString(string.Empty);
            Assert.Equal(0, parser.ParseRecords(null, new DataRecord[1], 0, 1));
            Assert.Equal(0, parser.ParseRecords(null, new DataRecord[100], 0, 100));
        }

        [Fact]
        public void parse_records_returns_zero_if_there_is_no_more_data_left()
        {
            var csv = @"first
second";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(2, parser.SkipRecords(2));
            Assert.Equal(0, parser.ParseRecords(null, new DataRecord[1], 0, 1));
            Assert.Equal(0, parser.ParseRecords(null, new DataRecord[100], 0, 100));
        }

        [Fact]
        public void parse_records_parses_only_the_specified_number_of_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[100];
            Assert.Equal(2, parser.ParseRecords(null, records, 0, 2));
            Assert.Equal("first", records[0][0]);
            Assert.Equal("second", records[1][0]);
            Assert.Equal(2, parser.ParseRecords(null, records, 0, 2));
            Assert.Equal("third", records[0][0]);
            Assert.Equal("fourth", records[1][0]);
        }

        [Fact]
        public void parse_records_populates_the_buffer_from_the_specified_offset()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[100];
            Assert.Equal(2, parser.ParseRecords(null, records, 5, 2));
            Assert.Null(records[0]);
            Assert.Equal("first", records[5][0]);
            Assert.Equal("second", records[6][0]);
            Assert.Equal(2, parser.ParseRecords(null, records, 10, 2));
            Assert.Equal("third", records[10][0]);
            Assert.Equal("fourth", records[11][0]);
        }

        [Fact]
        public void parse_records_uses_the_specified_header_record()
        {
            var headerRecord = new HeaderRecord(true, "Name", "Age");
            var csv = @"Kent,33
Belinda,34
Tempany,8
Xak,0";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[4];
            Assert.Equal(4, parser.ParseRecords(headerRecord, records, 0, records.Length));
            Assert.Same(headerRecord, records[0].HeaderRecord);
            Assert.Same(headerRecord, records[1].HeaderRecord);
            Assert.Same(headerRecord, records[2].HeaderRecord);
            Assert.Same(headerRecord, records[3].HeaderRecord);
        }

        [Fact]
        public void parse_records_stops_parsing_if_it_cannot_parse_any_more_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(4, parser.ParseRecords(null, new DataRecord[100], 0, 100));
        }

        [Fact]
        public void parse_records_works_with_complex_data()
        {
            var csv = @"value0,  value1   ,		value2	   	,'   value3  	 ''something in value3
over two lines''', value4 !23*&(#$@#*&#$)&][}{}{;.<>/?, value5
second record,foo,bar,biz,baz,fuz,fiz,'faz,fuz,buz'
third
fourth";
            var parser = this.CreateParserFromString(csv);
            parser.ValueDelimiter = '\'';
            var records = new DataRecord[4];

            Assert.Equal(4, parser.ParseRecords(null, records, 0, records.Length));

            Assert.Equal("value0", records[0][0]);
            Assert.Equal("value1", records[0][1]);
            Assert.Equal("value2", records[0][2]);
            Assert.Equal(@"   value3  	 'something in value3
over two lines'", records[0][3]);
            Assert.Equal("value4 !23*&(#$@#*&#$)&][}{}{;.<>/?", records[0][4]);
            Assert.Equal("value5", records[0][5]);

            Assert.Equal("second record", records[1][0]);
            Assert.Equal("foo", records[1][1]);
            Assert.Equal("bar", records[1][2]);
            Assert.Equal("biz", records[1][3]);
            Assert.Equal("baz", records[1][4]);
            Assert.Equal("fuz", records[1][5]);
            Assert.Equal("fiz", records[1][6]);
            Assert.Equal("faz,fuz,buz", records[1][7]);

            Assert.Equal("third", records[2][0]);

            Assert.Equal("fourth", records[3][0]);
        }

        [Fact]
        public void parse_records_can_handle_large_complex_data_set()
        {
            // the idea of this test is really just to exercise as many code paths as possible
            // it's too hard to assert the actual results - that is left for more specific tests

            var sb = new StringBuilder();

            // note the repeatable seed, chosen specifically because it yields 95% coverage
            var random = new Random(4096);
            var valueSeparator = ',';
            var valueDelimiter = '"';

            for (var record = 0; record < 150; ++record)
            {
                var fields = random.Next(1, 50);

                for (var field = 0; field < fields; ++field)
                {
                    var delimitValue = (random.Next(0, 2) == 0);

                    if (delimitValue)
                    {
                        sb.Append(valueDelimiter);
                    }

                    // random leading whitespace
                    sb.Append(' ', random.Next(0, 150));

                    sb.Append("value").Append(field);

                    // random trailing whitespace
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

                // randomly end the line
                var r = random.Next(0, 3);

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

            var records = new DataRecord[1];

            // no whitespace preservation
            var parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
            }

            // leading whitespace preservation
            parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;
            parser.PreserveLeadingWhiteSpace = true;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
            }

            // trailing whitespace preservation
            parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;
            parser.PreserveTrailingWhiteSpace = true;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
            }

            // leading and trailing whitespace preservation
            parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;
            parser.PreserveLeadingWhiteSpace = true;
            parser.PreserveTrailingWhiteSpace = true;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, parser.ParseRecords(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
            }
        }

        [Fact]
        public async void parse_records_async_returns_zero_if_there_are_no_more_records_to_parse()
        {
            var parser = this.CreateParserFromString(string.Empty);
            Assert.Equal(0, await parser.ParseRecordsAsync(null, new DataRecord[1], 0, 1));
            Assert.Equal(0, await parser.ParseRecordsAsync(null, new DataRecord[100], 0, 100));
        }

        [Fact]
        public async void parse_records_async_returns_zero_if_there_is_no_more_data_left()
        {
            var csv = @"first
second";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(2, parser.SkipRecords(2));
            Assert.Equal(0, await parser.ParseRecordsAsync(null, new DataRecord[1], 0, 1));
            Assert.Equal(0, await parser.ParseRecordsAsync(null, new DataRecord[100], 0, 100));
        }

        [Fact]
        public async void parse_records_async_parses_only_the_specified_number_of_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[400];
            Assert.Equal(2, await parser.ParseRecordsAsync(null, records, 0, 2));
            Assert.Equal("first", records[0][0]);
            Assert.Equal("second", records[1][0]);
            Assert.Equal(2, await parser.ParseRecordsAsync(null, records, 0, 2));
            Assert.Equal("third", records[0][0]);
            Assert.Equal("fourth", records[1][0]);
        }

        [Fact]
        public async void parse_records_async_populates_the_buffer_from_the_specified_offset()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[100];
            Assert.Equal(2, await parser.ParseRecordsAsync(null, records, 5, 2));
            Assert.Null(records[0]);
            Assert.Equal("first", records[5][0]);
            Assert.Equal("second", records[6][0]);
            Assert.Equal(2, parser.ParseRecords(null, records, 10, 2));
            Assert.Equal("third", records[10][0]);
            Assert.Equal("fourth", records[11][0]);
        }

        [Fact]
        public async void parse_records_async_uses_the_specified_header_record()
        {
            var headerRecord = new HeaderRecord(true, "Name", "Age");
            var csv = @"Kent,33
Belinda,34
Tempany,8
Xak,0";
            var parser = this.CreateParserFromString(csv);
            var records = new DataRecord[4];
            Assert.Equal(4, await parser.ParseRecordsAsync(headerRecord, records, 0, records.Length));
            Assert.Same(headerRecord, records[0].HeaderRecord);
            Assert.Same(headerRecord, records[1].HeaderRecord);
            Assert.Same(headerRecord, records[2].HeaderRecord);
            Assert.Same(headerRecord, records[3].HeaderRecord);
        }

        [Fact]
        public async void parse_records_async_stops_parsing_if_it_cannot_parse_any_more_records()
        {
            var csv = @"first
second
third
fourth";
            var parser = this.CreateParserFromString(csv);
            Assert.Equal(4, await parser.ParseRecordsAsync(null, new DataRecord[100], 0, 100));
        }

        [Fact]
        public async void parse_records_async_works_with_complex_data()
        {
            var csv = @"value0,  value1   ,		value2	   	,'   value3  	 ''something in value3
over two lines''', value4 !23*&(#$@#*&#$)&][}{}{;.<>/?, value5
second record,foo,bar,biz,baz,fuz,fiz,'faz,fuz,buz'
third
fourth";
            var parser = this.CreateParserFromString(csv);
            parser.ValueDelimiter = '\'';
            var records = new DataRecord[4];

            Assert.Equal(4, await parser.ParseRecordsAsync(null, records, 0, records.Length));

            Assert.Equal("value0", records[0][0]);
            Assert.Equal("value1", records[0][1]);
            Assert.Equal("value2", records[0][2]);
            Assert.Equal(@"   value3  	 'something in value3
over two lines'", records[0][3]);
            Assert.Equal("value4 !23*&(#$@#*&#$)&][}{}{;.<>/?", records[0][4]);
            Assert.Equal("value5", records[0][5]);

            Assert.Equal("second record", records[1][0]);
            Assert.Equal("foo", records[1][1]);
            Assert.Equal("bar", records[1][2]);
            Assert.Equal("biz", records[1][3]);
            Assert.Equal("baz", records[1][4]);
            Assert.Equal("fuz", records[1][5]);
            Assert.Equal("fiz", records[1][6]);
            Assert.Equal("faz,fuz,buz", records[1][7]);

            Assert.Equal("third", records[2][0]);

            Assert.Equal("fourth", records[3][0]);
        }

        [Fact]
        public async void parse_records_async_can_handle_large_complex_data_set()
        {
            // the idea of this test is really just to exercise as many code paths as possible
            // it's too hard to assert the actual results - that is left for more specific tests

            var sb = new StringBuilder();

            // note the repeatable seed, chosen specifically because it yields 95% coverage
            var random = new Random(4096);
            var valueSeparator = ',';
            var valueDelimiter = '"';

            for (var record = 0; record < 150; ++record)
            {
                var fields = random.Next(1, 50);

                for (var field = 0; field < fields; ++field)
                {
                    var delimitValue = (random.Next(0, 2) == 0);

                    if (delimitValue)
                    {
                        sb.Append(valueDelimiter);
                    }

                    // random leading whitespace
                    sb.Append(' ', random.Next(0, 150));

                    sb.Append("value").Append(field);

                    // random trailing whitespace
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

                // randomly end the line
                var r = random.Next(0, 3);

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

            var records = new DataRecord[1];

            // no whitespace preservation
            var parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, await parser.ParseRecordsAsync(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
            }

            // leading whitespace preservation
            parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;
            parser.PreserveLeadingWhiteSpace = true;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, await parser.ParseRecordsAsync(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
            }

            // trailing whitespace preservation
            parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;
            parser.PreserveTrailingWhiteSpace = true;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, await parser.ParseRecordsAsync(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
            }

            // leading and trailing whitespace preservation
            parser = this.CreateParserFromString(sb.ToString());
            parser.ValueSeparator = valueSeparator;
            parser.ValueDelimiter = valueDelimiter;
            parser.PreserveLeadingWhiteSpace = true;
            parser.PreserveTrailingWhiteSpace = true;

            while (parser.HasMoreRecords)
            {
                Assert.Equal(1, await parser.ParseRecordsAsync(null, records, 0, records.Length));

                for (var i = 0; i < records[0].Count; ++i)
                {
                    Assert.Equal("value" + i, records[0][i].Trim());
                }
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
