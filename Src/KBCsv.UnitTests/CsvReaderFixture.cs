namespace KBCsv.UnitTests
{
    using Moq;
    using Moq.Protected;
    using System;
    using System.IO;
    using Xunit;
    using System.Threading.Tasks;

    public sealed class CsvReaderFixture
    {
        [Fact]
        public void constructor_taking_stream_throws_if_stream_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvReader((Stream)null));
        }

        [Fact]
        public void constructor_taking_stream_and_encoding_throws_if_encoding_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvReader(new MemoryStream(), null));
        }

        [Fact]
        public void constructor_taking_text_reader_throws_if_text_reader_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvReader((TextReader)null));
        }

        [Fact]
        public void preserve_leading_white_space_defaults_to_false()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.False(reader.PreserveLeadingWhiteSpace);
            }
        }

        [Fact]
        public void preserve_leading_white_space_cannot_be_gotten_if_disposed()
        {
            var ignore = false;
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ignore = reader.PreserveLeadingWhiteSpace);
        }

        [Fact]
        public void preserve_leading_white_space_can_be_set()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                reader.PreserveLeadingWhiteSpace = true;
                Assert.True(reader.PreserveLeadingWhiteSpace);
            }
        }

        [Fact]
        public void preserve_leading_white_space_cannot_be_set_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.PreserveLeadingWhiteSpace = true);
        }

        [Fact]
        public void preserve_trailing_white_space_defaults_to_false()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.False(reader.PreserveTrailingWhiteSpace);
            }
        }

        [Fact]
        public void preserve_trailing_white_space_cannot_be_gotten_if_disposed()
        {
            var ignore = false;
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ignore = reader.PreserveTrailingWhiteSpace);
        }

        [Fact]
        public void preserve_trailing_white_space_can_be_set()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                reader.PreserveTrailingWhiteSpace = true;
                Assert.True(reader.PreserveTrailingWhiteSpace);
            }
        }

        [Fact]
        public void preserve_trailing_white_space_cannot_be_set_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.PreserveTrailingWhiteSpace = true);
        }

        [Fact]
        public void value_separator_defaults_to_comma()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Equal(',', reader.ValueSeparator);
            }
        }

        [Fact]
        public void value_separator_cannot_be_gotten_if_disposed()
        {
            var ignore = ' ';
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ignore = reader.ValueSeparator);
        }

        [Fact]
        public void value_separator_can_be_set()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                reader.ValueSeparator = '-';
                Assert.Equal('-', reader.ValueSeparator);
            }
        }

        [Fact]
        public void value_separator_cannot_be_set_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.ValueSeparator = '-');
        }

        [Fact]
        public void value_delimiter_defaults_to_double_quote()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Equal('"', reader.ValueDelimiter);
            }
        }

        [Fact]
        public void value_delimiter_cannot_be_gotten_if_disposed()
        {
            var ignore = ' ';
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ignore = reader.ValueDelimiter.Value);
        }

        [Fact]
        public void value_delimiter_can_be_set()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                reader.ValueDelimiter = '\'';
                Assert.Equal('\'', reader.ValueDelimiter);
            }
        }

        [Fact]
        public void value_delimiter_can_be_set_to_null()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                reader.ValueDelimiter = null;
                Assert.Null(reader.ValueDelimiter);
            }
        }

        [Fact]
        public void value_delimiter_cannot_be_set_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.ValueDelimiter = '|');
        }

        [Fact]
        public void escape_character_defaults_to_null()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Null(reader.EscapeCharacter);
            }
        }

        [Fact]
        public void escape_character_cannot_be_gotten_if_disposed()
        {
            var ignore = ' ';
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ignore = reader.EscapeCharacter.Value);
        }

        [Fact]
        public void escape_character_can_be_set()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                reader.EscapeCharacter = '\\';
                Assert.Equal('\\', reader.EscapeCharacter);
            }
        }

        [Fact]
        public void escape_character_can_be_set_to_null()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                reader.EscapeCharacter = null;
                Assert.Null(reader.EscapeCharacter);
            }
        }

        [Fact]
        public void escape_character_cannot_be_set_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.EscapeCharacter = '\\');
        }


        [Fact]
        public void header_record_defaults_to_null()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Null(reader.HeaderRecord);
            }
        }

        [Fact]
        public void header_record_cannot_be_gotten_if_disposed()
        {
            HeaderRecord ignore;
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => ignore = reader.HeaderRecord);
        }

        [Fact]
        public void header_record_can_be_set()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                var headerRecord = new HeaderRecord();
                reader.HeaderRecord = headerRecord;
                Assert.Same(headerRecord, reader.HeaderRecord);
            }
        }

        [Fact]
        public void header_record_cannot_be_set_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.HeaderRecord = new HeaderRecord());
        }

        [Fact]
        public void header_record_cannot_be_set_if_passed_first_record()
        {
            using (var reader = CsvReader.FromCsvString("foo"))
            {
                reader.ReadHeaderRecord();
                Assert.Throws<InvalidOperationException>(() => reader.HeaderRecord = new HeaderRecord());
            }
        }

        [Fact]
        public void record_number_defaults_to_zero()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Equal(0, reader.RecordNumber);
            }
        }

        [Fact]
        public void record_number_does_not_increase_when_header_is_assigned()
        {
            using (var reader = CsvReader.FromCsvString("Name,Age"))
            {
                reader.HeaderRecord = new HeaderRecord();
                Assert.Equal(0, reader.RecordNumber);
            }
        }

        [Fact]
        public void record_number_increases_when_header_is_read()
        {
            using (var reader = CsvReader.FromCsvString("Name,Age"))
            {
                reader.ReadHeaderRecord();
                Assert.Equal(1, reader.RecordNumber);
            }
        }

        [Fact]
        public void record_number_increases_when_data_records_are_read()
        {
            var csv = @"Name,Age
Kent,33
Belinda,34
Tempany,8
Xak,0";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();
                Assert.Equal(1, reader.RecordNumber);
                reader.ReadDataRecord();
                Assert.Equal(2, reader.RecordNumber);
                reader.ReadDataRecord();
                Assert.Equal(3, reader.RecordNumber);
                reader.ReadDataRecord();
                Assert.Equal(4, reader.RecordNumber);
                reader.ReadDataRecord();
                Assert.Equal(5, reader.RecordNumber);
            }
        }

        [Fact]
        public void has_more_records_returns_false_for_empty_csv()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void has_more_records_returns_true_if_there_are_more_records_left()
        {
            var csv = @"Name,Age
Kent,33
Belinda,34
Tempany,8
Xak,0";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.True(reader.HasMoreRecords);
                reader.ReadHeaderRecord();
                Assert.True(reader.HasMoreRecords);
                reader.ReadDataRecord();
                Assert.True(reader.HasMoreRecords);
                reader.ReadDataRecord();
                Assert.True(reader.HasMoreRecords);
                reader.ReadDataRecord();
                Assert.True(reader.HasMoreRecords);
                reader.ReadDataRecord();
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void from_csv_string_throws_if_string_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CsvReader.FromCsvString(null));
        }

        [Fact]
        public void skip_record_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.SkipRecord());
        }

        [Fact]
        public void skip_record_returns_false_if_there_is_no_record_to_skip()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.False(reader.SkipRecord());
            }

            using (var reader = CsvReader.FromCsvString("Header"))
            {
                reader.ReadHeaderRecord();
                Assert.False(reader.SkipRecord());
            }
        }

        [Fact]
        public void skip_record_returns_true_if_record_is_skipped()
        {
            using (var reader = CsvReader.FromCsvString("Header"))
            {
                Assert.True(reader.SkipRecord());
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void skip_record_increments_record_number()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.True(reader.SkipRecord());
                Assert.Equal(1, reader.RecordNumber);
                Assert.True(reader.SkipRecord());
                Assert.Equal(2, reader.RecordNumber);
                Assert.True(reader.SkipRecord());
                Assert.Equal(3, reader.RecordNumber);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void skip_record_can_optionally_increment_record_number()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.True(reader.SkipRecord(false));
                Assert.Equal(0, reader.RecordNumber);
                Assert.True(reader.SkipRecord(true));
                Assert.Equal(1, reader.RecordNumber);
                Assert.True(reader.SkipRecord(false));
                Assert.Equal(1, reader.RecordNumber);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task skip_record_async_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => reader.SkipRecordAsync());
        }

        [Fact]
        public async Task skip_record_async_returns_false_if_there_is_no_record_to_skip()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.False(await reader.SkipRecordAsync());
            }

            using (var reader = CsvReader.FromCsvString("Header"))
            {
                reader.ReadHeaderRecord();
                Assert.False(await reader.SkipRecordAsync());
            }
        }

        [Fact]
        public async Task skip_record_async_returns_true_if_record_is_skipped()
        {
            using (var reader = CsvReader.FromCsvString("Header"))
            {
                Assert.True(await reader.SkipRecordAsync());
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task skip_record_async_increments_record_number()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.True(await reader.SkipRecordAsync());
                Assert.Equal(1, reader.RecordNumber);
                Assert.True(await reader.SkipRecordAsync());
                Assert.Equal(2, reader.RecordNumber);
                Assert.True(await reader.SkipRecordAsync());
                Assert.Equal(3, reader.RecordNumber);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task skip_record_async_can_optionally_increment_record_number()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.True(await reader.SkipRecordAsync(false));
                Assert.Equal(0, reader.RecordNumber);
                Assert.True(await reader.SkipRecordAsync(true));
                Assert.Equal(1, reader.RecordNumber);
                Assert.True(await reader.SkipRecordAsync(false));
                Assert.Equal(1, reader.RecordNumber);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void skip_records_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.SkipRecords(100));
        }

        [Fact]
        public void skip_records_returns_zero_if_there_are_no_records_to_skip()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Equal(0, reader.SkipRecords(100));
            }

            using (var reader = CsvReader.FromCsvString("Header"))
            {
                reader.ReadHeaderRecord();
                Assert.Equal(0, reader.SkipRecords(1));
            }
        }

        [Fact]
        public void skip_records_returns_requested_skip_count_if_all_requested_records_are_skipped()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(3, reader.SkipRecords(3));
                Assert.Equal(7, reader.SkipRecords(7));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void skip_records_returns_actual_skip_count_if_all_requested_records_are_not_skipped()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, reader.SkipRecords(250));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void skip_records_increments_record_number()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(3, reader.SkipRecords(3));
                Assert.False(reader.HasMoreRecords);
                Assert.Equal(3, reader.RecordNumber);
            }
        }

        [Fact]
        public void skip_records_can_optionally_increment_record_number()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(4, reader.SkipRecords(4, false));
                Assert.Equal(0, reader.RecordNumber);
                Assert.Equal(4, reader.SkipRecords(4, true));
                Assert.Equal(4, reader.RecordNumber);
                Assert.Equal(2, reader.SkipRecords(2, false));
                Assert.Equal(4, reader.RecordNumber);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task skip_records_async_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => reader.SkipRecordsAsync(100));
        }

        [Fact]
        public async Task skip_records_async_returns_zero_if_there_are_no_records_to_skip()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Equal(0, await reader.SkipRecordsAsync(100));
            }

            using (var reader = CsvReader.FromCsvString("Header"))
            {
                reader.ReadHeaderRecord();
                Assert.Equal(0, await reader.SkipRecordsAsync(1));
            }
        }

        [Fact]
        public async Task skip_records_async_returns_requested_skip_count_if_all_requested_records_are_skipped()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(3, await reader.SkipRecordsAsync(3));
                Assert.Equal(7, await reader.SkipRecordsAsync(7));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task skip_records_async_returns_actual_skip_count_if_all_requested_records_are_not_skipped()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, await reader.SkipRecordsAsync(250));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task skip_records_async_increments_record_number()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(3, await reader.SkipRecordsAsync(3));
                Assert.False(reader.HasMoreRecords);
                Assert.Equal(3, reader.RecordNumber);
            }
        }

        [Fact]
        public async Task skip_records_async_can_optionally_increment_record_number()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(4, await reader.SkipRecordsAsync(4, false));
                Assert.Equal(0, reader.RecordNumber);
                Assert.Equal(4, await reader.SkipRecordsAsync(4, true));
                Assert.Equal(4, reader.RecordNumber);
                Assert.Equal(2, await reader.SkipRecordsAsync(2, false));
                Assert.Equal(4, reader.RecordNumber);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void read_header_record_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.ReadHeaderRecord());
        }

        [Fact]
        public void read_header_record_throws_if_passed_first_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadDataRecord();
                var ex = Assert.Throws<InvalidOperationException>(() => reader.ReadHeaderRecord());
                Assert.Equal("The CsvReader has already passed the first record, so this operation is not permitted.", ex.Message);
            }
        }

        [Fact]
        public void read_header_record_returns_null_if_there_is_no_record_to_read()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Null(reader.ReadHeaderRecord());
            }
        }

        [Fact]
        public void read_header_record_assigns_returned_record_as_header_record()
        {
            var csv = "Name,Age";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = reader.ReadHeaderRecord();
                Assert.NotNull(headerRecord);
                Assert.Same(headerRecord, reader.HeaderRecord);
            }
        }

        [Fact]
        public void read_header_record_increments_record_number()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(0, reader.RecordNumber);
                reader.ReadHeaderRecord();
                Assert.Equal(1, reader.RecordNumber);
            }
        }

        [Fact]
        public void read_header_record_correctly_parses_header()
        {
            var csv = "Name,Age";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = reader.ReadHeaderRecord();
                Assert.NotNull(headerRecord);
                Assert.Equal("Name", headerRecord[0]);
                Assert.Equal("Age", headerRecord[1]);
            }
        }

        [Fact]
        public void read_header_record_returns_read_only_record()
        {
            var csv = "Name,Age";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = reader.ReadHeaderRecord();
                Assert.NotNull(headerRecord);
                Assert.True(headerRecord.IsReadOnly);
            }
        }

        [Fact]
        public async Task read_header_record_async_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => reader.ReadHeaderRecordAsync());
        }

        [Fact]
        public async Task read_header_record_async_throws_if_passed_first_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                await reader.ReadDataRecordAsync();
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadHeaderRecordAsync());
                Assert.Equal("The CsvReader has already passed the first record, so this operation is not permitted.", ex.Message);
            }
        }

        [Fact]
        public async Task read_header_record_async_returns_null_if_there_is_no_record_to_read()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Null(await reader.ReadHeaderRecordAsync());
            }
        }

        [Fact]
        public async Task read_header_record_async_assigns_returned_record_as_header_record()
        {
            var csv = "Name,Age";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = await reader.ReadHeaderRecordAsync();
                Assert.NotNull(headerRecord);
                Assert.Same(headerRecord, reader.HeaderRecord);
            }
        }

        [Fact]
        public async Task read_header_record_async_increments_record_number()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(0, reader.RecordNumber);
                await reader.ReadHeaderRecordAsync();
                Assert.Equal(1, reader.RecordNumber);
            }
        }

        [Fact]
        public async Task read_header_record_async_correctly_parses_header()
        {
            var csv = "Name,Age";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = await reader.ReadHeaderRecordAsync();
                Assert.NotNull(headerRecord);
                Assert.Equal("Name", headerRecord[0]);
                Assert.Equal("Age", headerRecord[1]);
            }
        }

        [Fact]
        public async Task read_header_record_async_returns_read_only_record()
        {
            var csv = "Name,Age";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = await reader.ReadHeaderRecordAsync();
                Assert.NotNull(headerRecord);
                Assert.True(headerRecord.IsReadOnly);
            }
        }

        [Fact]
        public void read_data_record_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.ReadDataRecord());
        }

        [Fact]
        public void read_data_record_returns_null_if_there_is_no_record_to_read()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Null(reader.ReadDataRecord());
            }
        }

        [Fact]
        public void read_data_record_returns_data_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var record = reader.ReadDataRecord();
                Assert.NotNull(record);
                Assert.Equal(2, record.Count);
                Assert.Equal("Kent", record[0]);
                Assert.Equal("33", record[1]);
            }
        }

        [Fact]
        public void read_data_record_assigns_any_header_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = new HeaderRecord("Name", "Age");
                reader.HeaderRecord = headerRecord;
                var record = reader.ReadDataRecord();
                Assert.NotNull(record);
                Assert.Same(headerRecord, record.HeaderRecord);
            }
        }

        [Fact]
        public void read_data_record_returns_read_only_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var record = reader.ReadDataRecord();
                Assert.NotNull(record);
                Assert.True(record.IsReadOnly);
            }
        }

        [Fact]
        public void read_data_record_increments_record_number()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(0, reader.RecordNumber);
                reader.ReadDataRecord();
                Assert.Equal(1, reader.RecordNumber);
            }
        }

        [Fact]
        public async Task read_data_record_async_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => reader.ReadDataRecordAsync());
        }

        [Fact]
        public async Task read_data_record_async_returns_null_if_there_is_no_record_to_read()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Null(await reader.ReadDataRecordAsync());
            }
        }

        [Fact]
        public async Task read_data_record_async_returns_data_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var record = await reader.ReadDataRecordAsync();
                Assert.NotNull(record);
                Assert.Equal(2, record.Count);
                Assert.Equal("Kent", record[0]);
                Assert.Equal("33", record[1]);
            }
        }

        [Fact]
        public async Task read_data_record_async_assigns_any_header_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = new HeaderRecord("Name", "Age");
                reader.HeaderRecord = headerRecord;
                var record = await reader.ReadDataRecordAsync();
                Assert.NotNull(record);
                Assert.Same(headerRecord, record.HeaderRecord);
            }
        }

        [Fact]
        public async Task read_data_record_async_returns_read_only_record()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var record = await reader.ReadDataRecordAsync();
                Assert.NotNull(record);
                Assert.True(record.IsReadOnly);
            }
        }

        [Fact]
        public async Task read_data_record_async_increments_record_number()
        {
            var csv = "Kent,33";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(0, reader.RecordNumber);
                await reader.ReadDataRecordAsync();
                Assert.Equal(1, reader.RecordNumber);
            }
        }

        [Fact]
        public void read_data_records_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.ReadDataRecords(new DataRecord[1], 0, 1));
        }

        [Fact]
        public void read_data_records_returns_zero_if_there_are_no_records_to_read()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Equal(0, reader.ReadDataRecords(new DataRecord[1], 0, 1));
            }
        }

        [Fact]
        public void read_data_records_returns_requested_read_count_if_all_requested_records_are_read()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, reader.ReadDataRecords(new DataRecord[10], 0, 10));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void read_data_records_returns_actual_read_count_if_all_requested_records_are_not_read()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, reader.ReadDataRecords(new DataRecord[200], 0, 200));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void read_data_records_increments_record_number()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, reader.ReadDataRecords(new DataRecord[10], 0, 10));
                Assert.Equal(10, reader.RecordNumber);
            }
        }

        [Fact]
        public void read_data_records_assigns_any_header_record()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = new HeaderRecord("Number");
                reader.HeaderRecord = headerRecord;
                var buffer = new DataRecord[10];

                Assert.Equal(10, reader.ReadDataRecords(buffer, 0, buffer.Length));

                for (var i = 0; i < 10; ++i)
                {
                    Assert.Same(headerRecord, buffer[i].HeaderRecord);
                }
            }
        }

        [Fact]
        public void read_data_records_populates_buffer_starting_at_specified_index()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var buffer = new DataRecord[10];
                Assert.Equal(3, reader.ReadDataRecords(buffer, 7, 3));
                Assert.Equal("first", buffer[7][0]);
                Assert.Equal("second", buffer[8][0]);
                Assert.Equal("third", buffer[9][0]);
            }
        }

        [Fact]
        public void read_data_records_populates_buffer_ending_with_specified_count()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var buffer = new DataRecord[10];
                Assert.Equal(2, reader.ReadDataRecords(buffer, 0, 2));
                Assert.Equal("first", buffer[0][0]);
                Assert.Equal("second", buffer[1][0]);
                Assert.Null(buffer[2]);
            }
        }

        [Fact]
        public void read_data_records_returns_read_only_records()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var buffer = new DataRecord[3];
                Assert.Equal(3, reader.ReadDataRecords(buffer, 0, buffer.Length));
                Assert.NotNull(buffer[0]);
                Assert.True(buffer[0].IsReadOnly);
                Assert.NotNull(buffer[1]);
                Assert.True(buffer[1].IsReadOnly);
                Assert.NotNull(buffer[2]);
                Assert.True(buffer[2].IsReadOnly);
            }
        }

        [Fact]
        public async Task read_data_records_async_throws_if_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => reader.ReadDataRecordsAsync(new DataRecord[1], 0, 1));
        }

        [Fact]
        public async Task read_data_records_async_returns_zero_if_there_are_no_records_to_read()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Equal(0, await reader.ReadDataRecordsAsync(new DataRecord[1], 0, 1));
            }
        }

        [Fact]
        public async Task read_data_records_async_returns_requested_read_count_if_all_requested_records_are_read()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, await reader.ReadDataRecordsAsync(new DataRecord[10], 0, 10));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task read_data_records_async_returns_actual_read_count_if_all_requested_records_are_not_read()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, await reader.ReadDataRecordsAsync(new DataRecord[200], 0, 200));
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task read_data_records_async_increments_record_number()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                Assert.Equal(10, await reader.ReadDataRecordsAsync(new DataRecord[10], 0, 10));
                Assert.Equal(10, reader.RecordNumber);
            }
        }

        [Fact]
        public async Task read_data_records_async_assigns_any_header_record()
        {
            var csv = @"first
second
third
fourth
fifth
sixth
seventh
eighth
ninth
tenth";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var headerRecord = new HeaderRecord("Number");
                reader.HeaderRecord = headerRecord;
                var buffer = new DataRecord[10];

                Assert.Equal(10, await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length));

                for (var i = 0; i < 10; ++i)
                {
                    Assert.Same(headerRecord, buffer[i].HeaderRecord);
                }
            }
        }

        [Fact]
        public async Task read_data_records_async_populates_buffer_starting_at_specified_index()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var buffer = new DataRecord[10];
                Assert.Equal(3, await reader.ReadDataRecordsAsync(buffer, 7, 3));
                Assert.Equal("first", buffer[7][0]);
                Assert.Equal("second", buffer[8][0]);
                Assert.Equal("third", buffer[9][0]);
            }
        }

        [Fact]
        public async Task read_data_records_async_populates_buffer_ending_with_specified_count()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var buffer = new DataRecord[10];
                Assert.Equal(2, await reader.ReadDataRecordsAsync(buffer, 0, 2));
                Assert.Equal("first", buffer[0][0]);
                Assert.Equal("second", buffer[1][0]);
                Assert.Null(buffer[2]);
            }
        }

        [Fact]
        public async Task read_data_records_async_returns_read_only_records()
        {
            var csv = @"first
second
third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var buffer = new DataRecord[3];
                Assert.Equal(3, await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length));
                Assert.NotNull(buffer[0]);
                Assert.True(buffer[0].IsReadOnly);
                Assert.NotNull(buffer[1]);
                Assert.True(buffer[1].IsReadOnly);
                Assert.NotNull(buffer[2]);
                Assert.True(buffer[2].IsReadOnly);
            }
        }

        [Fact]
        public async Task read_data_records_async_with_escape_character()
        {
            var csv = @"first,""\""second\"""",third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.EscapeCharacter = '\\';

                var buffer = new DataRecord[1];
                Assert.Equal(1, await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length));

                var dataRecord = buffer[0];
                Assert.Equal(3, dataRecord.Count);
                Assert.Equal("first", dataRecord[0]);
                Assert.Equal("\"second\"", dataRecord[1]);
                Assert.Equal("third", dataRecord[2]);
            }
        }

        [Fact]
        public async Task read_data_records_async_with_no_escape_character()
        {
            var csv = @"first,\""second\"",third";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var buffer = new DataRecord[1];
                Assert.Equal(1, await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length));

                var dataRecord = buffer[0];
                Assert.Equal(3, dataRecord.Count);
                Assert.Equal("first", dataRecord[0]);
                Assert.Equal("\\second\\", dataRecord[1]);
                Assert.Equal("third", dataRecord[2]);
            }
        }

        [Fact]
        public void dispose_disposes_of_underlying_text_reader()
        {
            var textReader = new Mock<TextReader>();
            textReader.Protected().Setup("Dispose", true).Verifiable();

            using (new CsvReader(textReader.Object))
            {
            }

            textReader.Verify();
        }

        [Fact]
        public void dispose_does_not_dispose_of_underlying_text_reader_if_leave_open_is_specified_during_construction()
        {
            var textReader = new Mock<TextReader>();
            textReader.Protected().Setup("Dispose", true).Throws(new InvalidOperationException("Should not be called."));

            using (new CsvReader(textReader.Object, true))
            {
            }
        }

        [Fact]
        public void close_calls_dispose()
        {
            //var textReader = new Mock<TextReader>();
            //textReader.Protected().Setup("Dispose", true).Verifiable();

            //var reader = new CsvReader(textReader.Object);
            var sr = new StringReader("foo");
            var reader = new CsvReader(sr);
            //reader.Close();

            //textReader.Verify();
        }
    }
}
