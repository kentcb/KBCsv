using System;
using System.Data;
using System.IO;
using System.Text;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
    /// <summary>
    /// Unit tests the <see cref="CsvWriter"/> class.
    /// </summary>
    public sealed class CsvWriterTest
    {
        private CsvWriter _csvWriter;
        private StringWriter _stringWriter;
        private readonly string[] HEADERRECORD = new string[] { "Name", "Age", "Gender" };
        private readonly string[] RECORD1 = new string[] { "Kent", "25", "M" };
        private readonly string[] RECORD2 = new string[] { "Belinda", "26", "F" };
        private readonly string[] RECORD3 = new string[] { "Tempany", "0", "F" };

        public CsvWriterTest()
        {
            _stringWriter = new StringWriter();
            _csvWriter = new CsvWriter(_stringWriter);
        }

        [Fact]
        public void TestEncoding()
        {
            _csvWriter = new CsvWriter(new MemoryStream(), Encoding.UTF32);
            Assert.Equal(Encoding.UTF32, _csvWriter.Encoding);
        }

        [Fact]
        public void TestAlwaysDelimit()
        {
            Assert.False(_csvWriter.AlwaysDelimit);
            _csvWriter.WriteDataRecord("value1", " value2", "value3 ");
            AssertContents("value1,\" value2\",\"value3 \"{0}");
            _csvWriter.AlwaysDelimit = true;
            _csvWriter.WriteDataRecord("value1", " value2", "value3 ");
            AssertContents("value1,\" value2\",\"value3 \"{0}\"value1\",\" value2\",\"value3 \"{0}");
        }

        [Fact]
        public void TestValueDelimiterSameAsSeparator_DelimiterFirst()
        {
            _csvWriter.ValueDelimiter = '-';
            var ex = Assert.Throws<ArgumentException>(() => _csvWriter.ValueSeparator = '-');
            Assert.Equal("The value separator and value delimiter must be different.", ex.Message);
        }

        [Fact]
        public void TestValueSeparatorSpace()
        {
            var ex = Assert.Throws<ArgumentException>(() => _csvWriter.ValueSeparator = ' ');
            Assert.Equal("Space is not a valid value separator or delimiter.", ex.Message);
        }

        [Fact]
        public void TestValueSeparator()
        {
            _csvWriter.WriteDataRecord("value1", "value2", "value3");
            AssertContents("value1,value2,value3{0}");
            _csvWriter.ValueSeparator = '-';
            Assert.Equal('-', _csvWriter.ValueSeparator);
            _csvWriter.WriteDataRecord("value1", "value2", "value3");
            AssertContents("value1,value2,value3{0}value1-value2-value3{0}");
        }

        [Fact]
        public void TestValueDelimiterSameAsSeparator_SeparatorFirst()
        {
            _csvWriter.ValueSeparator = '-';
            var ex = Assert.Throws<ArgumentException>(() => _csvWriter.ValueDelimiter = '-');
            Assert.Equal("The value separator and value delimiter must be different.", ex.Message);
        }

        [Fact]
        public void TestValueDelimiterSpace()
        {
            var ex = Assert.Throws<ArgumentException>(() => _csvWriter.ValueDelimiter = ' ');
            Assert.Equal("Space is not a valid value separator or delimiter.", ex.Message);
        }

        [Fact]
        public void TestValueDelimiter()
        {
            _csvWriter.WriteDataRecord("value1", " value2 ", "value3");
            AssertContents("value1,\" value2 \",value3{0}");
            _csvWriter.ValueDelimiter = ':';
            Assert.Equal(':', _csvWriter.ValueDelimiter);
            _csvWriter.WriteDataRecord("value1", " value2 ", "value3");
            AssertContents("value1,\" value2 \",value3{0}value1,: value2 :,value3{0}");
        }

        [Fact]
        public void TestNewLine()
        {
            Assert.Equal(Environment.NewLine, _csvWriter.NewLine);
            _csvWriter.WriteDataRecord("value1", "value2");
            AssertContents("value1,value2{0}");
            _csvWriter.NewLine = "--whatever--";
            _csvWriter.WriteDataRecord("value1", "value2");
            AssertContents("value1,value2{0}value1,value2--whatever--");
        }

        [Fact]
        public void TestHeaderRecord()
        {
            Assert.Null(_csvWriter.HeaderRecord);
            _csvWriter.WriteHeaderRecord("column1", "column2");
            Assert.NotNull(_csvWriter.HeaderRecord);
            Assert.Equal("column1", _csvWriter.HeaderRecord.Values[0]);
            Assert.Equal("column2", _csvWriter.HeaderRecord.Values[1]);
        }

        [Fact]
        public void TestRecordNumber()
        {
            Assert.Equal(0, _csvWriter.RecordNumber);

            _csvWriter.WriteHeaderRecord("column1", "column2");
            // header record doesn't count
            Assert.Equal(0, _csvWriter.RecordNumber);

            _csvWriter.WriteDataRecord("value1", "value2");
            Assert.Equal(1, _csvWriter.RecordNumber);

            _csvWriter.WriteDataRecord("value3", "value4", "value5", "value6");
            Assert.Equal(2, _csvWriter.RecordNumber);

            _csvWriter.WriteDataRecord(new string[] { "value7", "value8" });
            Assert.Equal(3, _csvWriter.RecordNumber);

            _csvWriter.WriteDataRecord(new DataRecord(_csvWriter.HeaderRecord, new string[] { "value9", "value10" }));
            Assert.Equal(4, _csvWriter.RecordNumber);

            _csvWriter.WriteDataRecords(new string[][]
                {
                    new string[] { "value11", "value12" },
                    new string[] { "value13", "value14" }
                });
            Assert.Equal(6, _csvWriter.RecordNumber);

            _csvWriter.WriteDataRecords(new DataRecord[]
                {
                    new DataRecord(_csvWriter.HeaderRecord, new string[] { "value15", "value16" }),
                    new DataRecord(_csvWriter.HeaderRecord, new string[] { "value17", "value18" })
                });
            Assert.Equal(8, _csvWriter.RecordNumber);

            var dataSet = new DataSet();
            var table = dataSet.Tables.Add("Table");
            table.Columns.Add("First", typeof(string));
            table.Columns.Add("Second", typeof(string));
            table.Rows.Add((object[])new string[] { "value19", "value20" });
            table.Rows.Add((object[])new string[] { "value21", "value22" });
            _csvWriter.WriteAll(dataSet, false);
            Assert.Equal(10, _csvWriter.RecordNumber);

            _csvWriter.WriteAll(table, false);
            Assert.Equal(12, _csvWriter.RecordNumber);
        }

        [Fact]
        public void TestStreamConstructor()
        {
            MemoryStream memStream = new MemoryStream();
            _csvWriter = new CsvWriter(memStream);
            Assert.False(memStream.Length > 0);
            _csvWriter.WriteDataRecord("test");
            _csvWriter.Flush();
            Assert.True(memStream.Length > 0);
        }

        [Fact]
        public void TestStreamEncodingConstructor()
        {
            MemoryStream memStream = new MemoryStream();
            _csvWriter = new CsvWriter(memStream, Encoding.UTF32);
            Assert.Equal(Encoding.UTF32, _csvWriter.Encoding);
            Assert.False(memStream.Length > 0);
            _csvWriter.WriteDataRecord("test");
            _csvWriter.Flush();
            Assert.True(memStream.Length > 0);
        }

        [Fact]
        public void TestStringConstructor()
        {
            FileInfo file = new FileInfo(Path.GetTempFileName());
            _csvWriter = new CsvWriter(file.FullName);
            Assert.False(file.Length > 0);
            _csvWriter.WriteDataRecord("test");
            _csvWriter.Flush();
            _csvWriter.Close();
            file.Refresh();
            Assert.True(file.Length > 0);
        }

        [Fact]
        public void TestStringEncodingConstructor()
        {
            FileInfo file = new FileInfo(Path.GetTempFileName());
            _csvWriter = new CsvWriter(file.FullName, Encoding.UTF32);
            Assert.Equal(Encoding.UTF32, _csvWriter.Encoding);
            Assert.False(file.Length > 0);
            _csvWriter.WriteDataRecord("test");
            _csvWriter.Flush();
            _csvWriter.Close();
            file.Refresh();
            Assert.True(file.Length > 0);
        }

        [Fact]
        public void TestStringBoolConstructor()
        {
            FileInfo file = new FileInfo(Path.GetTempFileName());
            _csvWriter = new CsvWriter(file.FullName, false);
            Assert.False(file.Length > 0);
            _csvWriter.WriteDataRecord("test");
            _csvWriter.Flush();
            _csvWriter.Close();
            file.Refresh();
            long length = file.Length;
            Assert.True(length > 0);

            //now append
            _csvWriter = new CsvWriter(file.FullName, true);
            _csvWriter.WriteDataRecord("second record");
            _csvWriter.Flush();
            _csvWriter.Close();
            file.Refresh();
            Assert.True(file.Length > length);
        }

        [Fact]
        public void TestStringBoolEncodingConstructor()
        {
            FileInfo file = new FileInfo(Path.GetTempFileName());
            _csvWriter = new CsvWriter(file.FullName, false, Encoding.UTF32);
            Assert.Equal(Encoding.UTF32, _csvWriter.Encoding);
            Assert.False(file.Length > 0);
            _csvWriter.WriteDataRecord("test");
            _csvWriter.Flush();
            _csvWriter.Close();
            file.Refresh();
            long length = file.Length;
            Assert.True(length > 0);

            //now append
            _csvWriter = new CsvWriter(file.FullName, true);
            _csvWriter.WriteDataRecord("second record");
            _csvWriter.Flush();
            _csvWriter.Close();
            file.Refresh();
            Assert.True(file.Length > length);
        }

        [Fact]
        public void TestTextWriterConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter = new CsvWriter((TextWriter)null));
        }

        [Fact]
        public void TestTextWriterConstructor()
        {
            StringWriter stringWriter = new StringWriter();
            _csvWriter = new CsvWriter(stringWriter);
            Assert.False(stringWriter.ToString().Length > 0);
            _csvWriter.WriteDataRecord("value1", "value2");
            Assert.True(stringWriter.ToString().Length > 0);
        }

        [Fact]
        public void TestDispose()
        {
            _csvWriter = new CsvWriter(new MemoryStream());
            (_csvWriter as IDisposable).Dispose();
            char v;
            Assert.Throws<ObjectDisposedException>(() => v = _csvWriter.ValueDelimiter);
        }

        [Fact]
        public void TestClose()
        {
            _csvWriter = new CsvWriter(new MemoryStream());
            _csvWriter.Close();
            char v;
            Assert.Throws<ObjectDisposedException>(() => v = _csvWriter.ValueDelimiter);
        }

        [Fact]
        public void TestFlush()
        {
            MemoryStream memStream = new MemoryStream();
            BufferedStream bufStream = new BufferedStream(memStream, 2048);
            _csvWriter = new CsvWriter(bufStream);
            _csvWriter.WriteDataRecord("value1", "value2", "value3");
            Assert.Equal(0, memStream.Length);
            _csvWriter.Flush();
            Assert.True(memStream.Length > 0);
        }

        [Fact]
        public void TestWriteHeaderRecordAlreadyWritten()
        {
            _csvWriter.WriteDataRecord(RECORD1);
            var ex = Assert.Throws<InvalidOperationException>(() => _csvWriter.WriteHeaderRecord(HEADERRECORD));
            Assert.Equal("The first record has already been passed - cannot write header record.", ex.Message);
        }

        [Fact]
        public void TestWriteHeaderRecordStringsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteHeaderRecord((string[])null));
        }

        [Fact]
        public void TestWriteHeaderRecordStrings()
        {
            _csvWriter.WriteHeaderRecord(HEADERRECORD);
            AssertContents("Name,Age,Gender{0}");
        }

        [Fact]
        public void TestWriteHeaderRecordParamsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteHeaderRecord((object[])null));
        }

        [Fact]
        public void TestWriteHeaderRecordParams()
        {
            _csvWriter.WriteHeaderRecord("Name", 25, null, "Age");
            AssertContents("Name,25,,Age{0}");
        }

        [Fact]
        public void TestWriteHeaderRecordNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteHeaderRecord((HeaderRecord)null));
        }

        [Fact]
        public void TestWriteHeaderRecord()
        {
            _csvWriter.WriteHeaderRecord(new HeaderRecord(new string[] { "column1", "column2" }));
            AssertContents("column1,column2{0}");
        }

        [Fact]
        public void TestWriteDataRecordStringsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteDataRecord((string[])null));
        }

        [Fact]
        public void TestWriteDataRecordStringsNullEntry()
        {
            DataRecord record = new DataRecord(null, new string[] { "Kent", null, "M" });
            _csvWriter.WriteDataRecord(record);
            AssertContents("Kent,,M{0}");
        }

        [Fact]
        public void TestWriteDataRecordStrings()
        {
            _csvWriter.WriteDataRecord(RECORD1);
            AssertContents("Kent,25,M{0}");
            _csvWriter.WriteDataRecord(RECORD2);
            AssertContents("Kent,25,M{0}Belinda,26,F{0}");
            _csvWriter.WriteDataRecord(RECORD3);
            AssertContents("Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}");
        }

        [Fact]
        public void TestWriteDataRecordParamsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteDataRecord((object[])null));
        }

        [Fact]
        public void TestWriteDataRecordParams()
        {
            _csvWriter.WriteDataRecord("Kent", 25, null, "M");
            AssertContents("Kent,25,,M{0}");
            _csvWriter.WriteDataRecord("Belinda", 26, "test", "F");
            AssertContents("Kent,25,,M{0}Belinda,26,test,F{0}");
        }

        [Fact]
        public void TestWriteDataRecordNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteDataRecord((DataRecord)null));
        }

        [Fact]
        public void TestWriteDataRecord()
        {
            DataRecord dataRecord = new DataRecord(null, new string[] { "Kent", "25", "M" });
            _csvWriter.WriteDataRecord(dataRecord);
            dataRecord = new DataRecord(null, new string[] { "Belinda", "26", "F" });
            _csvWriter.WriteDataRecord(dataRecord);
            AssertContents("Kent,25,M{0}Belinda,26,F{0}");
        }

        [Fact]
        public void TestWriteDataRecordsStringsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteDataRecords((string[][])null));
        }

        [Fact]
        public void TestWriteDataRecordsStrings()
        {
            _csvWriter.WriteDataRecords(new string[][] { RECORD1, RECORD2, RECORD3 });
            AssertContents("Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}");
        }

        [Fact]
        public void TestWriteDataRecordsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteDataRecords((DataRecord[])null));
        }

        [Fact]
        public void TestWriteDataRecords()
        {
            DataRecord record1 = new DataRecord(null, new string[] { "Kent", "26", "M" });
            DataRecord record2 = new DataRecord(null, new string[] { "Belinda", "26", "F" });
            DataRecord record3 = new DataRecord(null, new string[] { "Tempany", "1", "F" });
            _csvWriter.WriteDataRecords(new DataRecord[] { record1, record2, record3 });
            AssertContents("Kent,26,M{0}Belinda,26,F{0}Tempany,1,F{0}");
        }

        [Fact]
        public void TestWriteAllNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteAll((DataSet)null));
        }

        [Fact]
        public void TestWriteAllDataSetNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteAll((DataSet)null));
        }

        [Fact]
        public void TestWriteAllDataSetNoTable()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _csvWriter.WriteAll(new DataSet()));
            Assert.Equal("The specified DataSet does not contain a table to write.", ex.Message);
        }

        [Fact]
        public void TestWriteAllDataSet()
        {
            DataSet dataSet = new DataSet();
            DataTable table = dataSet.Tables.Add("Table");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(string));
            table.Columns.Add("Gender", typeof(string));
            table.Rows.Add((object[])RECORD1);
            table.Rows.Add((object[])RECORD2);
            table.Rows.Add((object[])RECORD3);

            _csvWriter.WriteAll(dataSet, false);
            AssertContents("Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}");
        }

        [Fact]
        public void TestWriteAllDataSetWithHeader()
        {
            DataSet dataSet = new DataSet();
            DataTable table = dataSet.Tables.Add("Table");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(string));
            table.Columns.Add("Gender", typeof(string));
            table.Rows.Add((object[])RECORD1);
            table.Rows.Add((object[])RECORD2);
            table.Rows.Add((object[])RECORD3);
            _csvWriter.WriteAll(dataSet);
            AssertContents("Name,Age,Gender{0}Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}");
        }

        [Fact]
        public void WriteAllDataTableNull()
        {
            Assert.Throws<ArgumentNullException>(() => _csvWriter.WriteAll((DataTable)null));
        }

        [Fact]
        public void TestWriteAllDataTable()
        {
            DataTable table = new DataTable("Table");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(string));
            table.Columns.Add("Gender", typeof(string));
            table.Rows.Add((object[])RECORD1);
            table.Rows.Add((object[])RECORD2);
            table.Rows.Add((object[])RECORD3);
            _csvWriter.WriteAll(table, false);
            AssertContents("Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}");
        }

        [Fact]
        public void TestWriteAllDataTableWithHeader()
        {
            DataTable table = new DataTable("Table");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(string));
            table.Columns.Add("Gender", typeof(string));
            table.Rows.Add((object[])RECORD1);
            table.Rows.Add((object[])RECORD2);
            table.Rows.Add((object[])RECORD3);
            _csvWriter.WriteAll(table);
            AssertContents("Name,Age,Gender{0}Kent,25,M{0}Belinda,26,F{0}Tempany,0,F{0}");
        }

        [Fact]
        public void TestDelimitLeadingWhiteSpace()
        {
            _csvWriter.WriteDataRecord(" Kent", "25");
            AssertContents("\" Kent\",25{0}");
        }

        [Fact]
        public void TestDelimitTrailingWhiteSpace()
        {
            _csvWriter.WriteDataRecord("Kent ", "25");
            AssertContents("\"Kent \",25{0}");
        }

        [Fact]
        public void TestDelimitSeparator()
        {
            _csvWriter.WriteDataRecord("Kent", "October, 1979");
            AssertContents("Kent,\"October, 1979\"{0}");
        }

        [Fact]
        public void TestDelimitDelimiter()
        {
            _csvWriter.WriteDataRecord("Kent", "Some \"thing\"");
            AssertContents("Kent,\"Some \"\"thing\"\"\"{0}");
        }

        [Fact]
        public void TestDelimitNewLine()
        {
            _csvWriter.WriteDataRecord("Kent", string.Format("Some {0}thing", Environment.NewLine));
            AssertContents("Kent,\"Some {0}thing\"{0}");
        }

        [Fact]
        public void TestLargeComplexWrite()
        {
            //this test is mainly to get better code coverage in the area of buffer overflows. the assertions at the end are pretty straightforward

            StringWriter stringWriter = new StringWriter();
            _csvWriter = new CsvWriter(stringWriter);
            //fixed seed so repeatable
            Random random = new Random(1024);
            StringBuilder sb = new StringBuilder();

            for (int record = 0; record < 1500; ++record)
            {
                string[] values = new string[random.Next(1, 200)];

                for (int i = 0; i < values.Length; ++i)
                {
                    sb.Length = 0;

                    //random leading whitespace
                    sb.Append(' ', random.Next(0, 200));

                    sb.Append("value").Append(i);

                    //random trailing whitespace
                    sb.Append(' ', random.Next(0, 100));

                    values[i] = sb.ToString();
                }

                _csvWriter.WriteDataRecord(values);
            }

            using (CsvReader csvReader = CsvReader.FromCsvString(stringWriter.ToString()))
            {
                foreach (DataRecord record in csvReader.DataRecords)
                {
                    for (int i = 0; i < record.Values.Count; ++i)
                    {
                        Assert.Equal("value" + i, record.Values[i].Trim());
                    }
                }
            }
        }

        private void AssertContents(string expected)
        {
            AssertContents(expected, Environment.NewLine);
        }

        private void AssertContents(string expected, string lineBreak)
        {
            _csvWriter.Flush();
            Assert.Equal(string.Format(expected, lineBreak), _stringWriter.ToString());
        }
    }
}
