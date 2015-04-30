namespace Kent.Boogaart.KBCsv.UnitTests
{
    using Moq;
    using Moq.Protected;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class CsvWriterFixture
    {
        [Fact]
        public void constructor_stream_throws_if_stream_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvWriter((Stream)null));
        }

        [Fact]
        public void constructor_stream_throws_if_encoding_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvWriter(new MemoryStream(), null));
        }

        [Fact]
        public void constructor_text_writer_throws_if_text_writer_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvWriter((TextWriter)null));
        }

        [Fact]
        public void encoding_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.Encoding);
        }

        [Fact]
        public void encoding_gets_encoding()
        {
            using (var writer = new CsvWriter(new MemoryStream(), Encoding.ASCII))
            {
                Assert.Same(Encoding.ASCII, writer.Encoding);
            }
        }

        [Fact]
        public void record_number_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.RecordNumber);
        }

        [Fact]
        public void record_number_is_zero_by_default()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Equal(0, writer.RecordNumber);
            }
        }

        [Fact]
        public void force_delimit_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.ForceDelimit);
        }

        [Fact]
        public void force_delimit_throws_if_set_to_true_when_value_delimiter_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<ArgumentException>(() => writer.ForceDelimit = true);
                Assert.Equal("Value delimiter cannot be null when ForceDelimit is true.", ex.Message);
            }
        }

        [Fact]
        public void force_delimit_is_false_by_default()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.False(writer.ForceDelimit);
            }
        }

        [Fact]
        public void value_separator_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.ValueSeparator);
        }

        [Fact]
        public void value_separator_throws_if_same_as_delimiter()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                var ex = Assert.Throws<ArgumentException>(() => writer.ValueSeparator = writer.ValueDelimiter.Value);
                Assert.Equal("Value separator and delimiter cannot be the same.", ex.Message);
            }
        }

        [Fact]
        public void value_separator_is_comma_by_default()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Equal(',', writer.ValueSeparator);
            }
        }

        [Fact]
        public void value_separator_can_get_and_set()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueSeparator = ';';
                Assert.Equal(';', writer.ValueSeparator);
            }
        }

        [Fact]
        public void value_delimiter_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.ValueDelimiter);
        }

        [Fact]
        public void value_delimiter_throws_if_same_as_separator()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                var ex = Assert.Throws<ArgumentException>(() => writer.ValueDelimiter = writer.ValueSeparator);
                Assert.Equal("Value separator and delimiter cannot be the same.", ex.Message);
            }
        }

        [Fact]
        public void value_delimiter_throws_if_set_to_null_and_force_delimit_is_true()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ForceDelimit = true;
                var ex = Assert.Throws<ArgumentException>(() => writer.ValueDelimiter = null);
                Assert.Equal("Value delimiter cannot be null when ForceDelimit is true.", ex.Message);
            }
        }

        [Fact]
        public void value_delimiter_is_double_quote_by_default()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Equal('"', writer.ValueDelimiter);
            }
        }

        [Fact]
        public void value_delimiter_can_get_and_set()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = '\'';
                Assert.Equal('\'', writer.ValueDelimiter);
            }
        }

        [Fact]
        public void value_delimiter_can_be_set_to_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                Assert.Null(writer.ValueDelimiter);
            }
        }

        [Fact]
        public void new_line_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.NewLine);
        }

        [Fact]
        public void new_line_is_environment_new_line_by_default()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Equal(Environment.NewLine, writer.NewLine);
            }
        }

        [Fact]
        public void new_line_can_get_and_set()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.NewLine = "foo";
                Assert.Equal("foo", writer.NewLine);
            }
        }

        [Fact]
        public void write_record_record_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.WriteRecord(new HeaderRecord()));
        }

        [Fact]
        public void write_record_record_throws_if_record_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Throws<ArgumentNullException>(() => writer.WriteRecord((RecordBase)null));
            }
        }

        [Fact]
        public void write_record_record_throws_if_value_delimiter_is_null_and_value_contains_whitespace()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteRecord(new DataRecord(null, new string[] { " a value with leading and trailing whitespace " })));
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is:  a value with leading and trailing whitespace ", ex.Message);
            }
        }

        [Fact]
        public void write_record_record_throws_if_value_delimiter_is_null_and_value_contains_value_separator()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteRecord(new DataRecord(null, new string[] { "a value, with a comma" })));
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value, with a comma", ex.Message);
            }
        }

        [Fact]
        public void write_record_record_throws_if_value_delimiter_is_null_and_value_contains_carriage_return()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteRecord(new DataRecord(null, new string[] { "a value\r\nwith a carriage return" })));
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value\r\nwith a carriage return", ex.Message);
            }
        }

        [Fact]
        public void write_record_record_writes_values_in_the_record_to_the_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.WriteRecord(new HeaderRecord(new string[] { "one", "two", "three" }));
                }

                Assert.Equal("one,two,three" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_record_record_increments_the_record_number()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.WriteRecord(new HeaderRecord());
                Assert.Equal(1, writer.RecordNumber);
            }
        }

        [Fact]
        public void write_record_values_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.WriteRecord("one", "two", "three"));
        }

        [Fact]
        public void write_record_values_throws_if_values_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Throws<ArgumentNullException>(() => writer.WriteRecord((string[])null));
            }
        }

        [Fact]
        public void write_record_values_throws_if_value_delimiter_is_null_and_value_contains_whitespace()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteRecord(new string[] { " a value with leading and trailing whitespace " }));
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is:  a value with leading and trailing whitespace ", ex.Message);
            }
        }

        [Fact]
        public void write_record_values_throws_if_value_delimiter_is_null_and_value_contains_value_separator()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteRecord(new string[] { "a value, with a comma" }));
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value, with a comma", ex.Message);
            }
        }

        [Fact]
        public void write_record_values_throws_if_value_delimiter_is_null_and_value_contains_carriage_return()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteRecord(new string[] { "a value\r\nwith a carriage return" }));
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value\r\nwith a carriage return", ex.Message);
            }
        }

        [Fact]
        public void write_record_values_writes_values_to_the_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.WriteRecord("one", "two", "three");
                }

                Assert.Equal("one,two,three" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_record_values_null_values_are_encoded_as_empty_strings()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.WriteRecord("one", null, "three", null, "five", null);
                }

                Assert.Equal("one,,three,,five," + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_record_values_increments_the_record_number()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.WriteRecord("one", "two", "three");
                Assert.Equal(1, writer.RecordNumber);
            }
        }

        [Fact]
        public void write_record_uses_value_separator()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.ValueSeparator = ';';
                    writer.WriteRecord("one", "two", "three");
                }

                Assert.Equal("one;two;three" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_record_uses_value_delimiter_where_necessary()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.ValueDelimiter = '\'';
                    writer.WriteRecord("can", "can't", "will", "won't", " white space\t");
                }

                Assert.Equal("can,'can''t',will,'won''t',' white space\t'" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_record_uses_value_delimiter_always_if_force_is_true()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.ValueDelimiter = '\'';
                    writer.ForceDelimit = true;
                    writer.WriteRecord("can", "can't", "will", "won't");
                }

                Assert.Equal("'can','can''t','will','won''t'" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_record_uses_new_line()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.NewLine = "<<NL>>";
                    writer.WriteRecord("one", "two", "three");
                    writer.WriteRecord("four", "five", "six");
                }

                Assert.Equal("one,two,three<<NL>>four,five,six<<NL>>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_record_values_can_be_an_enumerable()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.WriteRecord(new List<string>(new string[] { "one", null, "three", null, "five", null }));
                }

                Assert.Equal("one,,three,,five," + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_record_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => writer.WriteRecordAsync(new HeaderRecord()));
        }

        [Fact]
        public async Task write_record_async_record_throws_if_record_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => writer.WriteRecordAsync((RecordBase)null));
            }
        }

        [Fact]
        public void write_record_async_record_throws_if_value_delimiter_is_null_and_value_contains_whitespace()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<AggregateException>(() => writer.WriteRecordAsync(new DataRecord(null, new string[] { " a value with leading and trailing whitespace " })).Wait());
                Assert.Equal(1, ex.InnerExceptions.Count);
                Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is:  a value with leading and trailing whitespace ", ex.InnerExceptions[0].Message);
            }
        }

        [Fact]
        public void write_record_async_record_throws_if_value_delimiter_is_null_and_value_contains_value_separator()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<AggregateException>(() => writer.WriteRecordAsync(new DataRecord(null, new string[] { "a value, with a comma" })).Wait());
                Assert.Equal(1, ex.InnerExceptions.Count);
                Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value, with a comma", ex.InnerExceptions[0].Message);
            }
        }

        [Fact]
        public void write_record_async_record_throws_if_value_delimiter_is_null_and_value_contains_carriage_return()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<AggregateException>(() => writer.WriteRecordAsync(new DataRecord(null, new string[] { "a value\r\nwith a carriage return" })).Wait());
                Assert.Equal(1, ex.InnerExceptions.Count);
                Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value\r\nwith a carriage return", ex.InnerExceptions[0].Message);
            }
        }

        [Fact]
        public async Task write_record_async_record_writes_values_in_the_record_to_the_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    await writer.WriteRecordAsync(new HeaderRecord(new string[] { "one", "two", "three" }));
                }

                Assert.Equal("one,two,three" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_record_increments_the_record_number()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                await writer.WriteRecordAsync(new HeaderRecord());
                Assert.Equal(1, writer.RecordNumber);
            }
        }

        [Fact]
        public async Task write_record_async_values_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => writer.WriteRecordAsync("one", "two", "three"));
        }

        [Fact]
        public async Task write_record_async_values_throws_if_values_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => writer.WriteRecordAsync((string[])null));
            }
        }

        [Fact]
        public void write_record_async_values_throws_if_value_delimiter_is_null_and_value_contains_whitespace()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<AggregateException>(() => writer.WriteRecordAsync(new string[] { " a value with leading and trailing whitespace " }).Wait());
                Assert.Equal(1, ex.InnerExceptions.Count);
                Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is:  a value with leading and trailing whitespace ", ex.InnerExceptions[0].Message);
            }
        }

        [Fact]
        public void write_record_async_values_throws_if_value_delimiter_is_null_and_value_contains_value_separator()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<AggregateException>(() => writer.WriteRecordAsync(new string[] { "a value, with a comma" }).Wait());
                Assert.Equal(1, ex.InnerExceptions.Count);
                Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value, with a comma", ex.InnerExceptions[0].Message);
            }
        }

        [Fact]
        public void write_record_async_values_throws_if_value_delimiter_is_null_and_value_contains_carriage_return()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                writer.ValueDelimiter = null;
                var ex = Assert.Throws<AggregateException>(() => writer.WriteRecordAsync(new string[] { "a value\r\nwith a carriage return" }).Wait());
                Assert.Equal(1, ex.InnerExceptions.Count);
                Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
                Assert.Equal("A value requires delimiting in order to be valid CSV, but no value delimiter has been set. The value is: a value\r\nwith a carriage return", ex.InnerExceptions[0].Message);
            }
        }

        [Fact]
        public async Task write_record_async_values_writes_values_to_the_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    await writer.WriteRecordAsync("one", "two", "three");
                }

                Assert.Equal("one,two,three" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_values_null_values_are_encoded_as_empty_strings()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    await writer.WriteRecordAsync("one", null, "three", null, "five", null);
                }

                Assert.Equal("one,,three,,five," + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_values_increments_the_record_number()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                await writer.WriteRecordAsync("one", "two", "three");
                Assert.Equal(1, writer.RecordNumber);
            }
        }

        [Fact]
        public async Task write_record_async_uses_value_separator()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.ValueSeparator = ';';
                    await writer.WriteRecordAsync("one", "two", "three");
                }

                Assert.Equal("one;two;three" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_uses_value_delimiter_where_necessary()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.ValueDelimiter = '\'';
                    await writer.WriteRecordAsync("can", "can't", "will", "won't", " white space\t");
                }

                Assert.Equal("can,'can''t',will,'won''t',' white space\t'" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_uses_value_delimiter_always_if_force_is_true()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.ValueDelimiter = '\'';
                    writer.ForceDelimit = true;
                    await writer.WriteRecordAsync("can", "can't", "will", "won't");
                }

                Assert.Equal("'can','can''t','will','won''t'" + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_uses_new_line()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.NewLine = "<<NL>>";
                    await writer.WriteRecordAsync("one", "two", "three");
                    await writer.WriteRecordAsync("four", "five", "six");
                }

                Assert.Equal("one,two,three<<NL>>four,five,six<<NL>>", stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_record_async_values_can_be_an_enumerable()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    await writer.WriteRecordAsync(new List<string>(new string[] { "one", null, "three", null, "five", null }));
                }

                Assert.Equal("one,,three,,five," + Environment.NewLine, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_records_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.WriteRecords(new RecordBase[1], 0, 1));
        }

        [Fact]
        public void write_records_throws_if_buffer_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Throws<ArgumentNullException>(() => writer.WriteRecords(null, 0, 1));
            }
        }

        [Fact]
        public void write_records_throws_if_offset_is_invalid()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Throws<ArgumentException>(() => writer.WriteRecords(new RecordBase[100], -1, 1));
                Assert.Throws<ArgumentException>(() => writer.WriteRecords(new RecordBase[100], 100, 1));
            }
        }

        [Fact]
        public void write_records_throws_if_length_is_invalid()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                Assert.Throws<ArgumentException>(() => writer.WriteRecords(new RecordBase[100], 0, 101));
                Assert.Throws<ArgumentException>(() => writer.WriteRecords(new RecordBase[100], 90, 20));
            }
        }

        [Fact]
        public void write_records_throws_if_any_included_record_within_the_buffer_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                var buffer = new RecordBase[]
                {
                    new HeaderRecord(),
                    new HeaderRecord(),
                    new HeaderRecord(),
                    null,
                    new HeaderRecord()
                };

                writer.WriteRecords(buffer, 0, 3);

                Assert.Throws<ArgumentException>(() => writer.WriteRecords(buffer, 0, 4));
            }
        }

        [Fact]
        public void write_records_writes_records_to_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    var buffer = new RecordBase[]
                    {
                        new DataRecord(null, new string[] { "one", "two", "three" }),
                        new DataRecord(null, new string[] { "four", "five", "six" }),
                        new DataRecord(null, new string[] { "seven", "eight", "nine", "ten" })
                    };

                    writer.WriteRecords(buffer, 0, buffer.Length);
                }

                var expectedCsv = @"one,two,three
four,five,six
seven,eight,nine,ten
";

                Assert.Equal(expectedCsv, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_records_writes_only_specified_records_to_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    var buffer = new RecordBase[]
                    {
                        new DataRecord(null, new string[] { "one", "two", "three" }),
                        new DataRecord(null, new string[] { "four", "five", "six" }),
                        new DataRecord(null, new string[] { "seven", "eight", "nine" }),
                        new DataRecord(null, new string[] { "ten" })
                    };

                    writer.WriteRecords(buffer, 1, 2);
                }

                var expectedCsv = @"four,five,six
seven,eight,nine
";

                Assert.Equal(expectedCsv, stringWriter.ToString());
            }
        }

        [Fact]
        public void write_records_increments_record_number()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                var buffer = new RecordBase[]
                {
                    new DataRecord(null, new string[] { "one", "two", "three" }),
                    new DataRecord(null, new string[] { "four", "five", "six" }),
                    new DataRecord(null, new string[] { "seven", "eight", "nine", "ten" })
                };

                writer.WriteRecords(buffer, 0, buffer.Length);
                Assert.Equal(3, writer.RecordNumber);
            }
        }

        [Fact]
        public async Task write_records_async_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => writer.WriteRecordsAsync(new RecordBase[1], 0, 1));
        }

        [Fact]
        public async Task write_records_async_throws_if_buffer_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => writer.WriteRecordsAsync(null, 0, 1));
            }
        }

        [Fact]
        public async Task write_records_async_throws_if_offset_is_invalid()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => writer.WriteRecordsAsync(new RecordBase[100], -1, 1));
                await Assert.ThrowsAsync<ArgumentException>(() => writer.WriteRecordsAsync(new RecordBase[100], 100, 1));
            }
        }

        [Fact]
        public async Task write_records_async_throws_if_length_is_invalid()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => writer.WriteRecordsAsync(new RecordBase[100], 0, 101));
                await Assert.ThrowsAsync<ArgumentException>(() => writer.WriteRecordsAsync(new RecordBase[100], 90, 20));
            }
        }

        [Fact]
        public async Task write_records_async_throws_if_any_included_record_within_the_buffer_is_null()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                var buffer = new RecordBase[]
                {
                    new HeaderRecord(),
                    new HeaderRecord(),
                    new HeaderRecord(),
                    null,
                    new HeaderRecord()
                };

                writer.WriteRecords(buffer, 0, 3);

                await Assert.ThrowsAsync<ArgumentException>(() => writer.WriteRecordsAsync(buffer, 0, 4));
            }
        }

        [Fact]
        public async Task write_records_async_writes_records_to_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    var buffer = new RecordBase[]
                    {
                        new DataRecord(null, new string[] { "one", "two", "three" }),
                        new DataRecord(null, new string[] { "four", "five", "six" }),
                        new DataRecord(null, new string[] { "seven", "eight", "nine", "ten" })
                    };

                    await writer.WriteRecordsAsync(buffer, 0, buffer.Length);
                }

                var expectedCsv = @"one,two,three
four,five,six
seven,eight,nine,ten
";

                Assert.Equal(expectedCsv, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_records_async_writes_only_specified_records_to_text_writer()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    var buffer = new RecordBase[]
                    {
                        new DataRecord(null, new string[] { "one", "two", "three" }),
                        new DataRecord(null, new string[] { "four", "five", "six" }),
                        new DataRecord(null, new string[] { "seven", "eight", "nine" }),
                        new DataRecord(null, new string[] { "ten" })
                    };

                    await writer.WriteRecordsAsync(buffer, 1, 2);
                }

                var expectedCsv = @"four,five,six
seven,eight,nine
";

                Assert.Equal(expectedCsv, stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_records_async_increments_record_number()
        {
            using (var writer = new CsvWriter(new MemoryStream()))
            {
                var buffer = new RecordBase[]
                {
                    new DataRecord(null, new string[] { "one", "two", "three" }),
                    new DataRecord(null, new string[] { "four", "five", "six" }),
                    new DataRecord(null, new string[] { "seven", "eight", "nine", "ten" })
                };

                await writer.WriteRecordsAsync(buffer, 0, buffer.Length);
                Assert.Equal(3, writer.RecordNumber);
            }
        }

        [Fact]
        public void write_char_to_buffer_can_be_used_to_customize_the_way_values_are_written()
        {
            var stringWriter = new StringWriter();
            var writer = new CustomCsvWriter(stringWriter);

            writer.ValueDelimiter = '\'';
            writer.NewLine = string.Empty;
            writer.WriteRecord("foo", "bar", "foo-bar");
            writer.Flush();

            Assert.Equal("foo,bar,'foo-bar'", stringWriter.ToString());
        }

        [Fact]
        public void flush_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => writer.Flush());
        }

        [Fact]
        public void flush_calls_flush_on_underlying_text_writer()
        {
            var textWriter = new Mock<TextWriter>();
            textWriter.Setup(x => x.Flush()).Verifiable();

            using (var writer = new CsvWriter(textWriter.Object))
            {
                writer.Flush();
            }

            textWriter.Verify();
        }

        [Fact]
        public async Task flush_async_throws_if_disposed()
        {
            var writer = new CsvWriter(new MemoryStream());
            writer.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => writer.FlushAsync());
        }

        [Fact]
        public async Task flush_async_calls_flush_on_underlying_text_writer()
        {
            var textWriter = new Mock<TextWriter>();
            var successTask = new TaskCompletionSource<bool>();
            successTask.SetResult(true);
            textWriter.Setup(x => x.FlushAsync()).Returns(successTask.Task).Verifiable();

            using (var writer = new CsvWriter(textWriter.Object))
            {
                await writer.FlushAsync();
            }

            textWriter.Verify();
        }

        [Fact]
        public void dispose_disposes_of_underlying_text_writer()
        {
            var textWriter = new Mock<TextWriter>();
            textWriter.Protected().Setup("Dispose", true).Verifiable();

            using (new CsvWriter(textWriter.Object))
            {
            }

            textWriter.Verify();
        }

        [Fact]
        public void dispose_does_not_dispose_of_underlying_text_writer_if_leave_open_is_specified_during_construction()
        {
            var textWriter = new Mock<TextWriter>();
            textWriter.Protected().Setup("Dispose", true).Throws(new InvalidOperationException("Should not be called."));

            using (new CsvWriter(textWriter.Object, true))
            {
            }
        }

        [Fact]
        public void close_calls_dispose()
        {
            var textWriter = new Mock<TextWriter>();
            textWriter.Protected().Setup("Dispose", true).Verifiable();

            var writer = new CsvWriter(textWriter.Object);
            writer.Close();

            textWriter.Verify();
        }

        #region Supporting Types

        private sealed class CustomCsvWriter : CsvWriter
        {
            public CustomCsvWriter(TextWriter textWriter)
                : base(textWriter)
            {
            }

            protected override void WriteCharToBuffer(StringBuilder buffer, char ch, ref bool delimit)
            {
                // only values containing hyphens will be delimited
                delimit = delimit || ch == '-';
                buffer.Append(ch);
            }
        }

        #endregion
    }
}
