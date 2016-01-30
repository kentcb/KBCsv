namespace KBCsv.Extensions.UnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using KBCsv.Extensions;
    using Xunit;

    public sealed class CsvReaderExtensionsFixture
    {
        [Fact]
        public void copy_to_throws_if_csv_reader_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ((CsvReader)null).CopyTo(new CsvWriter(new StringWriter())));
        }

        [Fact]
        public void copy_to_throws_if_csv_writer_is_null()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                Assert.Throws<ArgumentNullException>(() => reader.CopyTo(null));
            }
        }

        [Fact]
        public void copy_to_returns_number_of_records_copied()
        {
            var csv = @"First,Second
1,2
3,4
5,6";

            using (var reader = CsvReader.FromCsvString(csv))
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(4, reader.CopyTo(writer));
            }
        }

        [Fact]
        public void copy_to_copies_records()
        {
            var csv = @"First,Second
1,2
3,4
5,6";

            using (var reader = CsvReader.FromCsvString(csv))
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                Assert.Equal(4, reader.CopyTo(writer));
                writer.Flush();

                Assert.Equal("First,Second<EOL>1,2<EOL>3,4<EOL>5,6<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void copy_to_works_with_large_input()
        {
            var csv = string.Empty;

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                for (var i = 0; i < 1000; ++i)
                {
                    writer.WriteRecord("value0" + i, "value1" + i, "value2" + i);
                }

                writer.Flush();
                csv = stringWriter.ToString();
            }

            using (var reader = CsvReader.FromCsvString(csv))
            using (var writer = new CsvWriter(new StringWriter()))
            {
                writer.NewLine = "<EOL>";

                Assert.Equal(1000, reader.CopyTo(writer));
            }
        }
        
        [Fact]
        public async Task copy_to_async_throws_if_csv_reader_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((CsvReader)null).CopyToAsync(new CsvWriter(new StringWriter())));
        }

        [Fact]
        public async Task copy_to_async_throws_if_csv_writer_is_null()
        {
            using (var reader = CsvReader.FromCsvString(string.Empty))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => reader.CopyToAsync(null));
            }
        }

        [Fact]
        public async Task copy_to_async_returns_number_of_records_copied()
        {
            var csv = @"First,Second
1,2
3,4
5,6";

            using (var reader = CsvReader.FromCsvString(csv))
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(4, await reader.CopyToAsync(writer));
            }
        }

        [Fact]
        public async Task copy_to_async_copies_records()
        {
            var csv = @"First,Second
1,2
3,4
5,6";

            using (var reader = CsvReader.FromCsvString(csv))
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                Assert.Equal(4, await reader.CopyToAsync(writer));
                writer.Flush();

                Assert.Equal("First,Second<EOL>1,2<EOL>3,4<EOL>5,6<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async Task copy_to_async_works_with_large_input()
        {
            var csv = string.Empty;

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                for (var i = 0; i < 1000; ++i)
                {
                    writer.WriteRecord("value0" + i, "value1" + i, "value2" + i);
                }

                writer.Flush();
                csv = stringWriter.ToString();
            }

            using (var reader = CsvReader.FromCsvString(csv))
            using (var writer = new CsvWriter(new StringWriter()))
            {
                writer.NewLine = "<EOL>";

                Assert.Equal(1000, await reader.CopyToAsync(writer));
            }
        }

        [Fact]
        public void to_enumerable_throws_if_csv_reader_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ((CsvReader)null).ToEnumerable().ToList());
        }

        [Fact]
        public void to_enumerable_does_not_read_header_if_instructed()
        {
            var csv = @"value1,value2
value3,value4";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var results = reader
                    .ToEnumerable(readHeader: false)
                    .Select(record => record[0] + " " + record[1])
                    .Aggregate((current, next) => current + "-" + next);

                Assert.Equal("value1 value2-value3 value4", results);
            }
        }

        [Fact]
        public void to_enumerable_reads_header_if_instructed_to()
        {
            var csv = @"field1,field2
value1,value2
value3,value4";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                var results = reader
                    .ToEnumerable(readHeader: true)
                    .Select(record => record["field1"] + " " + record["field2"])
                    .Aggregate((current, next) => current + "-" + next);

                Assert.Equal("value1 value2-value3 value4", results);
            }
        }
    }
}
