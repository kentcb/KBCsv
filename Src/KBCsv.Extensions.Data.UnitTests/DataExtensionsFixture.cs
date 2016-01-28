namespace KBCsv.Extensions.UnitTests
{
    using KBCsv;
    using KBCsv.Extensions.Data;
    using System;
    using System.Data;
    using System.IO;
    using Xunit;
    using System.Threading.Tasks;

    public sealed class DataExtensionsFixture
    {
        [Fact]
        public void fill_data_set_throws_if_data_set_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ((DataSet)null).Fill(CsvReader.FromCsvString(string.Empty)));
        }

        [Fact]
        public void fill_data_set_throws_if_table_name_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new DataSet().Fill(CsvReader.FromCsvString(string.Empty), null));
        }

        [Fact]
        public void fill_data_set_throws_if_csv_reader_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new DataSet().Fill(null));
        }

        [Fact]
        public void fill_data_set_throws_if_csv_reader_is_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => new DataSet().Fill(reader));
        }

        [Fact]
        public void fill_data_set_uses_default_table_name_if_unspecified()
        {
            var dataSet = new DataSet();

            using (var reader = CsvReader.FromCsvString("First,Second"))
            {
                reader.ReadHeaderRecord();
                dataSet.Fill(reader);
            }

            Assert.Equal(1, dataSet.Tables.Count);
            Assert.Equal(DataExtensions.DefaultTableName, dataSet.Tables[0].TableName);
        }

        [Fact]
        public void fill_data_table_throws_if_data_table_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ((DataTable)null).Fill(CsvReader.FromCsvString(string.Empty)));
        }

        [Fact]
        public void fill_data_table_throws_if_csv_reader_is_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => new DataTable().Fill(reader));
        }

        [Fact]
        public void fill_data_table_throws_if_maximum_records_is_negative()
        {
            var ex = Assert.Throws<ArgumentException>(() => new DataTable().Fill(CsvReader.FromCsvString(string.Empty), -1));
            Assert.Equal("maximumRecords cannot be negative.", ex.Message);
        }

        [Fact]
        public void fill_data_table_throws_if_table_has_no_columns_and_csv_reader_has_no_header_record()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new DataTable().Fill(CsvReader.FromCsvString(string.Empty)));
            Assert.Equal("If the DataTable has no columns, the CsvReader must have a HeaderRecord from which columns will be constructed.", ex.Message);
        }

        [Fact]
        public void fill_data_table_throws_if_the_number_of_values_in_a_record_exceeds_the_number_of_columns_in_the_data_table()
        {
            var table = new DataTable();
            table.Columns.Add("First");
            table.Columns.Add("Second");

            using (var reader = CsvReader.FromCsvString("first,second,third"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => table.Fill(reader));
                Assert.Equal("DataTable has 2 columns, but a DataRecord had 3. The number of columns in the DataTable must match or exceed the number of values in each DataRecord.", ex.Message);
            }
        }

        [Fact]
        public void fill_data_table_uses_table_columns_if_specified()
        {
            var table = new DataTable();
            table.Columns.Add("First");
            table.Columns.Add("Second");
            var csv = @"Header1,Header2
1,2";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(1, table.Fill(reader));
                Assert.Equal(1, table.Rows.Count);
                Assert.Equal("First", table.Columns[0].ColumnName);
                Assert.Equal("Second", table.Columns[1].ColumnName);
                Assert.Equal("1", table.Rows[0][0]);
                Assert.Equal("2", table.Rows[0][1]);
            }
        }

        [Fact]
        public void fill_data_table_uses_header_record_if_columns_are_not_specified()
        {
            var table = new DataTable();
            var csv = @"Header1,Header2
1,2";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(1, table.Fill(reader));
                Assert.Equal(1, table.Rows.Count);
                Assert.Equal("Header1", table.Columns[0].ColumnName);
                Assert.Equal("Header2", table.Columns[1].ColumnName);
                Assert.Equal("1", table.Rows[0][0]);
                Assert.Equal("2", table.Rows[0][1]);
            }
        }

        [Fact]
        public void fill_data_table_stops_short_of_maximum_records_if_it_runs_out_of_data()
        {
            var table = new DataTable();
            var csv = @"Header1,Header2
1,2
3,4";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(2, table.Fill(reader, 10));
                Assert.Equal(2, table.Rows.Count);
            }
        }

        [Fact]
        public void fill_data_table_stops_if_it_reaches_maximum_records()
        {
            var table = new DataTable();
            var csv = @"Header1,Header2
1,2
3,4
5,6
7,8";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(3, table.Fill(reader, 3));
                Assert.Equal(3, table.Rows.Count);
                Assert.True(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void fill_data_table_works_with_large_csv_input()
        {
            var csv = string.Empty;

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.WriteRecord("Header1", "Header2");

                for (var i = 0; i < 1000; ++i)
                {
                    writer.WriteRecord("value0_" + i, "value1_" + i);
                }

                writer.Flush();
                csv = stringWriter.ToString();
            }

            // read less than all available records
            using (var reader = CsvReader.FromCsvString(csv))
            {
                var table = new DataTable();
                reader.ReadHeaderRecord();

                Assert.Equal(913, table.Fill(reader, 913));
                Assert.Equal(913, table.Rows.Count);
                Assert.True(reader.HasMoreRecords);
            }

            // read exactly available records
            using (var reader = CsvReader.FromCsvString(csv))
            {
                var table = new DataTable();
                reader.ReadHeaderRecord();

                Assert.Equal(1000, table.Fill(reader, 1000));
                Assert.Equal(1000, table.Rows.Count);
                Assert.False(reader.HasMoreRecords);
            }

            // attempt to read more than available records
            using (var reader = CsvReader.FromCsvString(csv))
            {
                var table = new DataTable();
                reader.ReadHeaderRecord();

                Assert.Equal(1000, table.Fill(reader, 1500));
                Assert.Equal(1000, table.Rows.Count);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task fill_data_set_async_throws_if_data_set_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((DataSet)null).FillAsync(CsvReader.FromCsvString(string.Empty)));
        }

        [Fact]
        public async Task fill_data_set_async_throws_if_table_name_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => new DataSet().FillAsync(CsvReader.FromCsvString(string.Empty), null));
        }

        [Fact]
        public async Task fill_data_set_async_throws_if_csv_reader_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => new DataSet().FillAsync(null));
        }

        [Fact]
        public async Task fill_data_set_async_throws_if_csv_reader_is_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => new DataSet().FillAsync(reader));
        }

        [Fact]
        public async Task fill_data_set_async_uses_default_table_name_if_unspecified()
        {
            var dataSet = new DataSet();

            using (var reader = CsvReader.FromCsvString("First,Second"))
            {
                reader.ReadHeaderRecord();
                await dataSet.FillAsync(reader);
            }

            Assert.Equal(1, dataSet.Tables.Count);
            Assert.Equal(DataExtensions.DefaultTableName, dataSet.Tables[0].TableName);
        }

        [Fact]
        public async Task fill_data_table_async_throws_if_data_table_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((DataTable)null).FillAsync(CsvReader.FromCsvString(string.Empty)));
        }

        [Fact]
        public async Task fill_data_table_async_throws_if_csv_reader_is_disposed()
        {
            var reader = CsvReader.FromCsvString(string.Empty);
            reader.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => new DataTable().FillAsync(reader));
        }

        [Fact]
        public async Task fill_data_table_async_throws_if_maximum_records_is_negative()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => new DataTable().FillAsync(CsvReader.FromCsvString(string.Empty), -1));
            Assert.Equal("maximumRecords cannot be negative.", ex.Message);
        }

        [Fact]
        public async Task fill_data_table_async_throws_if_table_has_no_columns_and_csv_reader_has_no_header_record()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => new DataTable().FillAsync(CsvReader.FromCsvString(string.Empty)));
            Assert.Equal("If the DataTable has no columns, the CsvReader must have a HeaderRecord from which columns will be constructed.", ex.Message);
        }

        [Fact]
        public async Task fill_data_table_async_throws_if_the_number_of_values_in_a_record_exceeds_the_number_of_columns_in_the_data_table()
        {
            var table = new DataTable();
            table.Columns.Add("First");
            table.Columns.Add("Second");

            using (var reader = CsvReader.FromCsvString("first,second,third"))
            {
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => table.FillAsync(reader));
                Assert.Equal("DataTable has 2 columns, but a DataRecord had 3. The number of columns in the DataTable must match or exceed the number of values in each DataRecord.", ex.Message);
            }
        }

        [Fact]
        public async Task fill_data_table_async_uses_table_columns_if_specified()
        {
            var table = new DataTable();
            table.Columns.Add("First");
            table.Columns.Add("Second");
            var csv = @"Header1,Header2
1,2";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(1, await table.FillAsync(reader));
                Assert.Equal(1, table.Rows.Count);
                Assert.Equal("First", table.Columns[0].ColumnName);
                Assert.Equal("Second", table.Columns[1].ColumnName);
                Assert.Equal("1", table.Rows[0][0]);
                Assert.Equal("2", table.Rows[0][1]);
            }
        }

        [Fact]
        public async Task fill_data_table_async_uses_header_record_if_columns_are_not_specified()
        {
            var table = new DataTable();
            var csv = @"Header1,Header2
1,2";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(1, await table.FillAsync(reader));
                Assert.Equal(1, table.Rows.Count);
                Assert.Equal("Header1", table.Columns[0].ColumnName);
                Assert.Equal("Header2", table.Columns[1].ColumnName);
                Assert.Equal("1", table.Rows[0][0]);
                Assert.Equal("2", table.Rows[0][1]);
            }
        }

        [Fact]
        public async Task fill_data_table_async_stops_short_of_maximum_records_if_it_runs_out_of_data()
        {
            var table = new DataTable();
            var csv = @"Header1,Header2
1,2
3,4";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(2, await table.FillAsync(reader, 10));
                Assert.Equal(2, table.Rows.Count);
            }
        }

        [Fact]
        public async Task fill_data_table_async_stops_if_it_reaches_maximum_records()
        {
            var table = new DataTable();
            var csv = @"Header1,Header2
1,2
3,4
5,6
7,8";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();

                Assert.Equal(3, await table.FillAsync(reader, 3));
                Assert.Equal(3, table.Rows.Count);
                Assert.True(reader.HasMoreRecords);
            }
        }

        [Fact]
        public async Task fill_data_table_async_works_with_large_csv_input()
        {
            var csv = string.Empty;

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.WriteRecord("Header1", "Header2");

                for (var i = 0; i < 1000; ++i)
                {
                    writer.WriteRecord("value0_" + i, "value1_" + i);
                }

                writer.Flush();
                csv = stringWriter.ToString();
            }

            // read less than all available records
            using (var reader = CsvReader.FromCsvString(csv))
            {
                var table = new DataTable();
                reader.ReadHeaderRecord();

                Assert.Equal(913, await table.FillAsync(reader, 913));
                Assert.Equal(913, table.Rows.Count);
                Assert.True(reader.HasMoreRecords);
            }

            // read exactly available records
            using (var reader = CsvReader.FromCsvString(csv))
            {
                var table = new DataTable();
                reader.ReadHeaderRecord();

                Assert.Equal(1000, await table.FillAsync(reader, 1000));
                Assert.Equal(1000, table.Rows.Count);
                Assert.False(reader.HasMoreRecords);
            }

            // attempt to read more than available records
            using (var reader = CsvReader.FromCsvString(csv))
            {
                var table = new DataTable();
                reader.ReadHeaderRecord();

                Assert.Equal(1000, await table.FillAsync(reader, 1500));
                Assert.Equal(1000, table.Rows.Count);
                Assert.False(reader.HasMoreRecords);
            }
        }

        [Fact]
        public void write_csv_throws_if_data_table_is_null()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Throws<ArgumentNullException>(() => ((DataTable)null).WriteCsv(writer));
            }
        }

        [Fact]
        public void write_csv_throws_if_csv_writer_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new DataTable().WriteCsv(null));
        }

        [Fact]
        public void write_csv_throws_if_object_to_string_converter_is_null()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Throws<ArgumentNullException>(() => new DataTable().WriteCsv(writer, false, null, null));
            }
        }

        [Fact]
        public void write_csv_throws_if_csv_reader_is_disposed()
        {
            var writer = new CsvWriter(new StringWriter());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => new DataTable().WriteCsv(writer));
        }

        [Fact]
        public void write_csv_writes_a_header_record_based_on_data_table_column_names_if_requested()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");

                Assert.Equal(0, dataTable.WriteCsv(writer, true));
                Assert.Equal("First,Second<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_writes_all_rows_as_data_records()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", "2");
                dataTable.Rows.Add("3", "4");
                dataTable.Rows.Add("5", "6");

                Assert.Equal(3, dataTable.WriteCsv(writer, true));
                Assert.Equal("First,Second<EOL>1,2<EOL>3,4<EOL>5,6<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_stops_if_it_reaches_maximum_rows()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", "2");
                dataTable.Rows.Add("3", "4");
                dataTable.Rows.Add("5", "6");

                Assert.Equal(2, dataTable.WriteCsv(writer, true, 2));
                Assert.Equal("First,Second<EOL>1,2<EOL>3,4<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_uses_object_to_string_converter_to_convert_objects_in_data_row_to_string()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", 2);
                dataTable.Rows.Add(3, 4d);
                dataTable.Rows.Add(5m, 6f);

                Assert.Equal(3, dataTable.WriteCsv(writer, true, null, o => o.ToString() + "_SUFFIX"));
                Assert.Equal("First,Second<EOL>1_SUFFIX,2_SUFFIX<EOL>3_SUFFIX,4_SUFFIX<EOL>5_SUFFIX,6_SUFFIX<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_object_to_string_converter_converts_nulls_to_empty_string()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", 2);
                dataTable.Rows.Add(null, 4d);
                dataTable.Rows.Add(5m, null);

                Assert.Equal(3, dataTable.WriteCsv(writer));
                Assert.Equal("First,Second<EOL>1,2<EOL>,4<EOL>5,<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_returns_number_of_records_written()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                writer.WriteRecord("some", "record");

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", "2");
                dataTable.Rows.Add("3", "4");
                dataTable.Rows.Add("5", "6");

                Assert.Equal(3, dataTable.WriteCsv(writer, false));
                Assert.Equal(3, dataTable.WriteCsv(writer, true));
            }
        }

        [Fact]
        public void write_csv_works_with_large_data_table_input()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("First");
            dataTable.Columns.Add("Second");

            for (var i = 0; i < 1000; ++i)
            {
                dataTable.Rows.Add("value0_" + i, "value1_" + i);
            }

            // write less than all available records
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(913, dataTable.WriteCsv(writer, true, 913));
            }

            // write exactly available records
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(1000, dataTable.WriteCsv(writer, true, 1000));
            }

            // attempt to write more than available records
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(1000, dataTable.WriteCsv(writer, true, 1500));
            }
        }

        [Fact]
        public async Task write_csv_async_throws_if_data_table_is_null()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => ((DataTable)null).WriteCsvAsync(writer));
            }
        }

        [Fact]
        public async Task write_csv_async_throws_if_csv_writer_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => new DataTable().WriteCsvAsync(null));
        }

        [Fact]
        public async Task write_csv_async_throws_if_object_to_string_converter_is_null()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => new DataTable().WriteCsvAsync(writer, false, null, null));
            }
        }

        [Fact]
        public async Task write_csv_async_throws_if_csv_reader_is_disposed()
        {
            var writer = new CsvWriter(new StringWriter());
            writer.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => new DataTable().WriteCsvAsync(writer));
        }

        [Fact]
        public async Task write_csv_async_writes_a_header_record_based_on_data_table_column_names_if_requested()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");

                Assert.Equal(0, await dataTable.WriteCsvAsync(writer, true));
                Assert.Equal("First,Second<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_csv_async_writes_all_rows_as_data_records()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", "2");
                dataTable.Rows.Add("3", "4");
                dataTable.Rows.Add("5", "6");

                Assert.Equal(3, await dataTable.WriteCsvAsync(writer, true));
                Assert.Equal("First,Second<EOL>1,2<EOL>3,4<EOL>5,6<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_csv_async_stops_if_it_reaches_maximum_rows()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", "2");
                dataTable.Rows.Add("3", "4");
                dataTable.Rows.Add("5", "6");

                Assert.Equal(2, await dataTable.WriteCsvAsync(writer, true, 2));
                Assert.Equal("First,Second<EOL>1,2<EOL>3,4<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_csv_async_uses_object_to_string_converter_to_convert_objects_in_data_row_to_string()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", 2);
                dataTable.Rows.Add(3, 4d);
                dataTable.Rows.Add(5m, 6f);

                Assert.Equal(3, await dataTable.WriteCsvAsync(writer, true, null, o => o.ToString() + "_SUFFIX"));
                Assert.Equal("First,Second<EOL>1_SUFFIX,2_SUFFIX<EOL>3_SUFFIX,4_SUFFIX<EOL>5_SUFFIX,6_SUFFIX<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_csv_async_object_to_string_converter_converts_nulls_to_empty_string()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", 2);
                dataTable.Rows.Add(null, 4d);
                dataTable.Rows.Add(5m, null);

                Assert.Equal(3, await dataTable.WriteCsvAsync(writer));
                Assert.Equal("First,Second<EOL>1,2<EOL>,4<EOL>5,<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async Task write_csv_async_returns_number_of_records_written()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                writer.WriteRecord("some", "record");

                var dataTable = new DataTable();
                dataTable.Columns.Add("First");
                dataTable.Columns.Add("Second");
                dataTable.Rows.Add("1", "2");
                dataTable.Rows.Add("3", "4");
                dataTable.Rows.Add("5", "6");

                Assert.Equal(3, await dataTable.WriteCsvAsync(writer, false));
                Assert.Equal(3, await dataTable.WriteCsvAsync(writer, true));
            }
        }

        [Fact]
        public async Task write_csv_async_works_with_large_data_table_input()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("First");
            dataTable.Columns.Add("Second");

            for (var i = 0; i < 1000; ++i)
            {
                dataTable.Rows.Add("value0_" + i, "value1_" + i);
            }

            // write less than all available records
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(913, await dataTable.WriteCsvAsync(writer, true, 913));
            }

            // write exactly available records
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(1000, await dataTable.WriteCsvAsync(writer, true, 1000));
            }

            // attempt to write more than available records
            using (var writer = new CsvWriter(new StringWriter()))
            {
                Assert.Equal(1000, await dataTable.WriteCsvAsync(writer, true, 1500));
            }
        }
    }
}
