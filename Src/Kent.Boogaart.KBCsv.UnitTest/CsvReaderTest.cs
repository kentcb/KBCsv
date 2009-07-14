using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
	/// <summary>
	/// Unit tests the <see cref="CsvReader"/> class.
	/// </summary>
	public sealed class CsvReaderTest : IDisposable
	{
		private CsvReader _csvReader;

		public void Dispose()
		{
			if (_csvReader != null)
			{
				_csvReader.Close();
			}
		}

		[Fact]
		public void TestPreserveLeadingWhiteSpace()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1,   value2,  value3  {0}value1,   value2,  value3  {0}", Environment.NewLine));
			Assert.False(_csvReader.PreserveLeadingWhiteSpace);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("value1", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3", record[2]);
			_csvReader.PreserveLeadingWhiteSpace = true;
			Assert.True(_csvReader.PreserveLeadingWhiteSpace);
			record = _csvReader.ReadDataRecord();
			Assert.Equal("value1", record[0]);
			Assert.Equal("   value2", record[1]);
			Assert.Equal("  value3", record[2]);
		}

		[Fact]
		public void TestTrailingLeadingWhiteSpace()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1 ,   value2,  value3  {0}value1 ,   value2,  value3  {0}", Environment.NewLine));
			Assert.False(_csvReader.PreserveTrailingWhiteSpace);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("value1", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3", record[2]);
			_csvReader.PreserveTrailingWhiteSpace = true;
			Assert.True(_csvReader.PreserveTrailingWhiteSpace);
			record = _csvReader.ReadDataRecord();
			Assert.Equal("value1 ", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3  ", record[2]);
		}

		[Fact]
		public void TestPreserveAllWhiteSpace()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1 ,   value2,  value3  {0}value1 ,   value2,  value3  {0}", Environment.NewLine));
			Assert.False(_csvReader.PreserveLeadingWhiteSpace);
			Assert.False(_csvReader.PreserveTrailingWhiteSpace);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("value1", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3", record[2]);
			_csvReader.PreserveLeadingWhiteSpace = true;
			_csvReader.PreserveTrailingWhiteSpace = true;
			Assert.True(_csvReader.PreserveLeadingWhiteSpace);
			Assert.True(_csvReader.PreserveTrailingWhiteSpace);
			record = _csvReader.ReadDataRecord();
			Assert.Equal("value1 ", record[0]);
			Assert.Equal("   value2", record[1]);
			Assert.Equal("  value3  ", record[2]);
		}

		[Fact]
		public void TestValueSeparator()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1,value2,value3{0}value1-value2-value3{0}", Environment.NewLine));
			Assert.Equal(CsvParser.DefaultValueSeparator, _csvReader.ValueSeparator);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("value1", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3", record[2]);
			_csvReader.ValueSeparator = '-';
			Assert.Equal('-', _csvReader.ValueSeparator);
			record = _csvReader.ReadDataRecord();
			Assert.Equal("value1", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3", record[2]);
		}

		[Fact]
		public void TestValueDelimiter()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("\" value1 \",\"value2\",value3{0}: value1 :,:value2:,value3{0}", Environment.NewLine));
			Assert.Equal(CsvParser.DefaultValueDelimiter, _csvReader.ValueDelimiter);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal(" value1 ", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3", record[2]);
			_csvReader.ValueDelimiter = ':';
			Assert.Equal(':', _csvReader.ValueDelimiter);
			record = _csvReader.ReadDataRecord();
			Assert.Equal(" value1 ", record[0]);
			Assert.Equal("value2", record[1]);
			Assert.Equal("value3", record[2]);
		}

		[Fact]
		public void TestHeaderRecordNull()
		{
			_csvReader = CsvReader.FromCsvString(string.Empty);
			Assert.Throws<ArgumentNullException>(() => _csvReader.HeaderRecord = null);
		}

		[Fact]
		public void TestHeaderRecordPassedFirst()
		{
			_csvReader = CsvReader.FromCsvString("value1,value2,value3");
			_csvReader.ReadDataRecord();
			var ex = Assert.Throws<InvalidOperationException>(() => _csvReader.HeaderRecord = new HeaderRecord());
			Assert.Equal("The first record has already been passed - cannot set header record.", ex.Message);
		}

		[Fact]
		public void TestHeaderRecordAlreadySet()
		{
			_csvReader = CsvReader.FromCsvString(string.Empty);
			_csvReader.HeaderRecord = new HeaderRecord();
			var ex = Assert.Throws<InvalidOperationException>(() => _csvReader.HeaderRecord = new HeaderRecord());
			Assert.Equal("A header record already exists.", ex.Message);
		}

		[Fact]
		public void TestHeaderRecord()
		{
			_csvReader = CsvReader.FromCsvString(string.Empty);
			_csvReader.HeaderRecord = new HeaderRecord(new string[] { "column1", "column2" });
			Assert.NotNull(_csvReader.HeaderRecord);
			Assert.Equal("column1", _csvReader.HeaderRecord.Values[0]);
			Assert.Equal("column2", _csvReader.HeaderRecord.Values[1]);
		}

		[Fact]
		public void TestRecordNumber()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1,value2,value3{0}value1,value2,value3{0}", Environment.NewLine));
			Assert.Equal(0, _csvReader.RecordNumber);
			_csvReader.ReadDataRecord();
			Assert.Equal(1, _csvReader.RecordNumber);
			_csvReader.SkipRecord();
			Assert.Equal(2, _csvReader.RecordNumber);
		}

		[Fact]
		public void TestHasMoreRecords()
		{
			Init("csv.records");
			Assert.True(_csvReader.HasMoreRecords);
			_csvReader.ReadDataRecord();
			Assert.True(_csvReader.HasMoreRecords);
			_csvReader.SkipRecord();
			Assert.True(_csvReader.HasMoreRecords);
			_csvReader.ReadDataRecord();
			Assert.True(_csvReader.HasMoreRecords);
			_csvReader.SkipRecord();
			Assert.False(_csvReader.HasMoreRecords);
		}

		[Fact]
		public void TestDataRecords()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1,value2,value3{0}value1,value2,value3{0}value1,value2,value3{0}", Environment.NewLine));

			foreach (DataRecord record in _csvReader.DataRecords)
			{
				Assert.NotNull(record);
				Assert.Equal("value1", record.Values[0]);
				Assert.Equal("value2", record.Values[1]);
				Assert.Equal("value3", record.Values[2]);
			}

			Assert.Equal(3, _csvReader.RecordNumber);
		}

		[Fact]
		public void TestDataRecordsAsStrings()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1,value2,value3{0}value1,value2,value3{0}value1,value2,value3{0}", Environment.NewLine));

			foreach (string[] record in _csvReader.DataRecordsAsStrings)
			{
				Assert.NotNull(record);
				Assert.Equal("value1", record[0]);
				Assert.Equal("value2", record[1]);
				Assert.Equal("value3", record[2]);
			}

			Assert.Equal(3, _csvReader.RecordNumber);
		}

		[Fact]
		public void TestFromCsvStringNull()
		{
			Assert.Throws<ArgumentNullException>(() => _csvReader = CsvReader.FromCsvString(null));
		}

		[Fact]
		public void TestFromCsvString()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("value1,value2,value3{0}value1,value2,value3{0}", Environment.NewLine));
			Assert.Equal(2, _csvReader.ReadDataRecords().Count);
		}

		[Fact]
		public void TestStreamConstructor()
		{
			MemoryStream memStream = new MemoryStream();
			byte[] bytes = Encoding.Default.GetBytes("value1,value2,value3\rvalue1,value2,value3\n");
			memStream.Write(bytes, 0, bytes.Length);
			memStream.Position = 0;
			_csvReader = new CsvReader(memStream);
			Assert.Equal(2, _csvReader.ReadDataRecords().Count);
		}

		[Fact]
		public void TestStreamEncodingConstructor()
		{
			MemoryStream memStream = new MemoryStream();
			byte[] bytes = Encoding.UTF32.GetBytes("value1,value2,value3\rvalue1,value2,value3\n");
			memStream.Write(bytes, 0, bytes.Length);
			memStream.Position = 0;
			_csvReader = new CsvReader(memStream, Encoding.UTF32);
			Assert.Equal(2, _csvReader.ReadDataRecords().Count);
		}

		[Fact]
		public void TestStringConstructor()
		{
			string path = Path.GetTempFileName();
			File.WriteAllLines(path, new string[] {"value1,value2,value3", "value1,value2,value3" });
			_csvReader = new CsvReader(path);
			Assert.Equal(2, _csvReader.ReadDataRecords().Count);
		}

		[Fact]
		public void TestStringEncodingConstructor()
		{
			string path = Path.GetTempFileName();
			File.WriteAllLines(path, new string[] { "value1,value2,value3", "value1,value2,value3" }, Encoding.UTF32);
			_csvReader = new CsvReader(path, Encoding.UTF32);
			Assert.Equal(2, _csvReader.ReadDataRecords().Count);
		}

		[Fact]
		public void TestTextReaderConstructorNull()
		{
			Assert.Throws<ArgumentNullException>(() => _csvReader = new CsvReader((TextReader)null));
		}

		[Fact]
		public void TestTextReaderConstructor()
		{
			StringReader stringReader = new StringReader("value1,value2,value3\rvalue1,value2,value3\n");
			_csvReader = new CsvReader(stringReader);
			Assert.Equal(2, _csvReader.ReadDataRecords().Count);
		}

		[Fact]
		public void TestDispose()
		{
			_csvReader = CsvReader.FromCsvString("value1,value2");
			(_csvReader as IDisposable).Dispose();
			Assert.Throws<ObjectDisposedException>(() => _csvReader.ValueDelimiter = ':');
		}

		[Fact]
		public void TestClose()
		{
			_csvReader = CsvReader.FromCsvString("value1,value2");
			_csvReader.Close();
			Assert.Throws<ObjectDisposedException>(() => _csvReader.ValueDelimiter = ':');
		}

		[Fact]
		public void TestReadHeaderTwice()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Tempany,0{0}Kent,25", Environment.NewLine));
			_csvReader.ReadHeaderRecord();
			Assert.Throws<InvalidOperationException>(() => _csvReader.ReadHeaderRecord());
		}

		[Fact]
		public void TestReadHeaderAfterRecord()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Tempany,0{0}Kent,25", Environment.NewLine));
			_csvReader.ReadDataRecord();
			Assert.Throws<InvalidOperationException>(() => _csvReader.ReadHeaderRecord());
		}


		[Fact]
		public void TestEmpty()
		{
			_csvReader = CsvReader.FromCsvString(string.Empty);
			Assert.Null(_csvReader.ReadHeaderRecord());
			Assert.Null(_csvReader.ReadDataRecord());
		}

		[Fact]
		public void TestReadBlank()
		{
			_csvReader = CsvReader.FromCsvString("  ");
			_csvReader.PreserveLeadingWhiteSpace = true;
			_csvReader.PreserveTrailingWhiteSpace = true;
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.NotNull(record);
			Assert.Equal("  ", record[0]);
		}

		[Fact]
		public void TestHeaders()
		{
			Init("csv.headers");
			_csvReader.ReadHeaderRecord();
			Assert.NotNull(_csvReader.HeaderRecord);
			Assert.Equal(3, _csvReader.HeaderRecord.Values.Count);
			Assert.Equal("Name", _csvReader.HeaderRecord[0]);
			Assert.Equal("Gender", _csvReader.HeaderRecord[1]);
			Assert.Equal("Age", _csvReader.HeaderRecord[2]);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.NotNull(record);
			Assert.NotNull(record.HeaderRecord);
			Assert.Equal(_csvReader.HeaderRecord, record.HeaderRecord);
			Assert.Equal(3, record.Values.Count);
			Assert.Equal("25", record["Age"]);
		}

		[Fact]
		public void TestRecords()
		{
			Init("csv.records");
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.NotNull(record);
			Assert.Equal(3, record.Values.Count);
			Assert.Equal("Kent", record[0]);
			Assert.Equal("M", record[1]);
			Assert.Equal("25", record[2]);

			record = _csvReader.ReadDataRecord();
			Assert.NotNull(record);
			Assert.Equal(3, record.Values.Count);
			Assert.Equal("Belinda", record[0]);
			Assert.Equal("F", record[1]);
			Assert.Equal("26", record[2]);

			record = _csvReader.ReadDataRecord();
			Assert.NotNull(record);
			Assert.Equal(3, record.Values.Count);
			Assert.Equal("Tempany", record[0]);
			Assert.Equal("F", record[1]);
			Assert.Equal("0", record[2]);

			record = _csvReader.ReadDataRecord();
			Assert.NotNull(record);
			Assert.Equal(4, record.Values.Count);
			Assert.Equal("Jennifer LH", record[0]);
			Assert.Equal("F", record[1]);
			Assert.Equal("23", record[2]);
			Assert.Equal("Extraneous", record[3]);
		}

		[Fact]
		public void TestReadOnly()
		{
			Init("csv.records");
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.NotNull(record);
			Assert.Equal(3, record.Values.Count);
			var ex = Assert.Throws<NotSupportedException>(() => record[0] = "foobar");
			Assert.Equal("Collection is read-only.", ex.Message);
		}

		[Fact]
		public void TestEscapedData()
		{
			Init("csv.escaped-data");
			_csvReader.ReadHeaderRecord();
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("MUSE", record["Band"]);
			Assert.Equal("February, 2004", record["Date"]);
			Assert.Equal("BDO Adelaide", record["Venue, City"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal("MUSE", record["Band"]);
			Assert.Equal("October, 2004", record["Date"]);
			Assert.Equal("Thebby Theatre, \"Adelaide\"", record["Venue, City"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal("Gomez", record["Band"]);
			Assert.Equal("March, 2004", record["Date"]);
			Assert.Equal("Thebby Theatre, Adelaide", record["Venue, City"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal("Pearl Jam", record["Band"]);
			Assert.Equal("March, 2002", record["Date"]);
			Assert.Equal(string.Format("Entertainment Centre{0}Adelaide", Environment.NewLine), record["Venue, City"]);
			Assert.Equal(string.Empty, record[3]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal("Powderfinger", record["Band"]);
			Assert.Equal("Multiple \"dates\"", record["Date"]);
			Assert.Equal("Multiple Venues", record["Venue, City"]);
		}

		[Fact]
		public void TestNonStandardSeparator()
		{
			Init("csv.non-standard-separator", '-');
			_csvReader.ReadHeaderRecord();
			Assert.Equal(4, _csvReader.HeaderRecord.Values.Count);
			Assert.Equal("Surname", _csvReader.HeaderRecord[0]);
			Assert.Equal("Given Name", _csvReader.HeaderRecord[1]);
			Assert.Equal("Era", _csvReader.HeaderRecord[2]);
			Assert.Equal("Status", _csvReader.HeaderRecord[3]);

			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal(4, record.Values.Count);
			Assert.Equal("Heche", record["Surname"]);
			Assert.Equal("Anne", record["Given Name"]);
			Assert.Equal("1995", record["Era"]);
			Assert.Equal("hot-as", record["Status"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal(4, record.Values.Count);
			Assert.Equal("Love-Hewitt", record["Surname"]);
			Assert.Equal("Jennifer", record["Given Name"]);
			Assert.Equal("1997-now", record["Era"]);
			Assert.Equal("Hel\"lo!!", record["Status"]);

			Assert.Null(_csvReader.ReadDataRecord());
		}

		[Fact]
		public void TestNonStandardDelimiter()
		{
			Init("csv.non-standard-delimiter", '-', '~');
			_csvReader.ReadHeaderRecord();
			Assert.Equal(4, _csvReader.HeaderRecord.Values.Count);
			Assert.Equal("Surname", _csvReader.HeaderRecord[0]);
			Assert.Equal("Given Name", _csvReader.HeaderRecord[1]);
			Assert.Equal("Era", _csvReader.HeaderRecord[2]);
			Assert.Equal("Status", _csvReader.HeaderRecord[3]);

			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal(4, record.Values.Count);
			Assert.Equal("Heche", record["Surname"]);
			Assert.Equal("Anne", record["Given Name"]);
			Assert.Equal("1995", record["Era"]);
			Assert.Equal("hot-as", record["Status"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal(4, record.Values.Count);
			Assert.Equal("Love-Hewitt", record["Surname"]);
			Assert.Equal("Jennifer", record["Given Name"]);
			Assert.Equal("1997-now", record["Era"]);
			Assert.Equal("Hel~lo!!", record["Status"]);

			Assert.Null(_csvReader.ReadDataRecord());
		}

		[Fact]
		public void TestLFOnly()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Name,Age{0}Kent,25{0}Belinda,26{0}Tempany,0", (char)0x0a));
			_csvReader.ReadHeaderRecord();
			Assert.Equal(2, _csvReader.HeaderRecord.Values.Count);
			Assert.Equal("Name", _csvReader.HeaderRecord[0]);
			Assert.Equal("Age", _csvReader.HeaderRecord[1]);

			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal(2, record.Values.Count);
			Assert.Equal("Kent", record["Name"]);
			Assert.Equal("25", record["Age"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal(2, record.Values.Count);
			Assert.Equal("Belinda", record["Name"]);
			Assert.Equal("26", record["Age"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal(2, record.Values.Count);
			Assert.Equal("Tempany", record["Name"]);
			Assert.Equal("0", record["Age"]);

			Assert.Null(_csvReader.ReadDataRecord());
		}

		[Fact]
		public void TestCROnly()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Name,Age{0}Kent,25{0}Belinda,26{0}Tempany,0", (char)0x0d));
			_csvReader.ReadHeaderRecord();
			Assert.Equal(2, _csvReader.HeaderRecord.Values.Count);
			Assert.Equal("Name", _csvReader.HeaderRecord[0]);
			Assert.Equal("Age", _csvReader.HeaderRecord[1]);

			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal(2, record.Values.Count);
			Assert.Equal("Kent", record["Name"]);
			Assert.Equal("25", record["Age"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal(2, record.Values.Count);
			Assert.Equal("Belinda", record["Name"]);
			Assert.Equal("26", record["Age"]);

			record = _csvReader.ReadDataRecord();
			Assert.Equal(2, record.Values.Count);
			Assert.Equal("Tempany", record["Name"]);
			Assert.Equal("0", record["Age"]);

			Assert.Null(_csvReader.ReadDataRecord());
		}

		[Fact]
		public void TestReadDataRecords()
		{
			Init("csv.escaped-data");
			ICollection<DataRecord> records = _csvReader.ReadDataRecords();
			Assert.Equal(6, records.Count);
		}

		[Fact]
		public void TestReadDataRecordsInvalidMaximum()
		{
			Init("csv.escaped-data");
			var ex = Assert.Throws<ArgumentException>(() => _csvReader.ReadDataRecords(-1));
			Assert.Equal("The maximumRecords parameter cannot be less than zero.", ex.Message);
		}

		[Fact]
		public void TestReadDataRecordsToMaximum()
		{
			Init("csv.escaped-data");
			ICollection<DataRecord> records = _csvReader.ReadDataRecords(2);
			Assert.Equal(2, records.Count);
			records = _csvReader.ReadDataRecords(3);
			Assert.Equal(3, records.Count);
			records = _csvReader.ReadDataRecords(10);
			Assert.Equal(1, records.Count);
			Init("csv.escaped-data");
			records = _csvReader.ReadDataRecords(100);
			Assert.Equal(6, records.Count);
		}

		[Fact]
		public void TestReadDataRecordsAsStrings()
		{
			Init("csv.escaped-data");
			ICollection<string[]> records = _csvReader.ReadDataRecordsAsStrings();
			Assert.Equal(6, records.Count);
		}

		[Fact]
		public void TestReadDataRecordsAsStringsInvalidMaximum()
		{
			Init("csv.escaped-data");
			var ex = Assert.Throws<ArgumentException>(() => _csvReader.ReadDataRecordsAsStrings(-1));
			Assert.Equal("The maximumRecords parameter cannot be less than zero.", ex.Message);
		}

		[Fact]
		public void TestReadDataRecordsAsStringsToMaximum()
		{
			Init("csv.escaped-data");
			ICollection<string[]> records = _csvReader.ReadDataRecordsAsStrings(2);
			Assert.Equal(2, records.Count);
			records = _csvReader.ReadDataRecordsAsStrings(3);
			Assert.Equal(3, records.Count);
			records = _csvReader.ReadDataRecordsAsStrings(10);
			Assert.Equal(1, records.Count);
			Init("csv.escaped-data");
			records = _csvReader.ReadDataRecordsAsStrings(100);
			Assert.Equal(6, records.Count);
		}

		[Fact]
		public void TestSkip()
		{
			Init("csv.escaped-data");
			Assert.True(_csvReader.SkipRecord());
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("MUSE", record[0]);
			Assert.True(_csvReader.SkipRecord());
			record = _csvReader.ReadDataRecord();
			Assert.Equal("Gomez", record[0]);
			Assert.True(_csvReader.SkipRecord());
			record = _csvReader.ReadDataRecord();
			Assert.Equal("Powderfinger", record[0]);
			Assert.False(_csvReader.SkipRecord());
		}

		[Fact]
		public void TestSkipBlank()
		{
			_csvReader = CsvReader.FromCsvString("  ");
			_csvReader.PreserveLeadingWhiteSpace = true;
			_csvReader.PreserveTrailingWhiteSpace = true;
			Assert.True(_csvReader.SkipRecord());
		}

		[Fact]
		public void TestSkipRecordCount()
		{
			Init("csv.escaped-data");
			Assert.True(_csvReader.SkipRecord(true));
			Assert.Equal(1, _csvReader.RecordNumber);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal(2, _csvReader.RecordNumber);
			Assert.Equal("MUSE", record[0]);
			Assert.True(_csvReader.SkipRecord(false));
			Assert.Equal(2, _csvReader.RecordNumber);
			record = _csvReader.ReadDataRecord();
			Assert.Equal(3, _csvReader.RecordNumber);
			Assert.Equal("Gomez", record[0]);
			Assert.True(_csvReader.SkipRecord(false));
			Assert.Equal(3, _csvReader.RecordNumber);
			record = _csvReader.ReadDataRecord();
			Assert.Equal(4, _csvReader.RecordNumber);
			Assert.Equal("Powderfinger", record[0]);
			//try various permutations of SkipRecord, ensuring each returns false now that there is no data left
			Assert.False(_csvReader.SkipRecord());
			Assert.False(_csvReader.SkipRecord(true));
			Assert.False(_csvReader.SkipRecord(false));
		}

		[Fact]
		public void TestSkipRecordsInvalidNumber()
		{
			Init("csv.escaped-data");
			var ex = Assert.Throws<ArgumentException>(() => _csvReader.SkipRecords(-1));
			Assert.Equal("The number parameter cannot be less than zero.", ex.Message);
		}

		[Fact]
		public void TestSkipRecordsWithBoolInvalidNumber()
		{
			Init("csv.escaped-data");
			var ex = Assert.Throws<ArgumentException>(() => _csvReader.SkipRecords(-1, true));
			Assert.Equal("The number parameter cannot be less than zero.", ex.Message);
		}

		[Fact]
		public void TestSkipRecords()
		{
			Init("csv.escaped-data");
			Assert.Equal(6, _csvReader.SkipRecords(100));
			Assert.Equal(6, _csvReader.RecordNumber);
			Init("csv.escaped-data");
			Assert.Equal(6, _csvReader.SkipRecords(100, false));
			Assert.Equal(0, _csvReader.RecordNumber);
			Init("csv.escaped-data");
			Assert.Equal(4, _csvReader.SkipRecords(4, false));
			Assert.Equal(2, _csvReader.SkipRecords(3, true));
			Assert.Equal(2, _csvReader.RecordNumber);
		}

		[Fact]
		public void TestFill()
		{
			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			DataSet dataSet = new DataSet();
			Assert.Equal(5, _csvReader.Fill(dataSet));
			Assert.Equal("Kent", dataSet.Tables[0].Rows[0].ItemArray[0]);
			Assert.Equal("F", dataSet.Tables[0].Rows[4].ItemArray[2]);

			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			dataSet = new DataSet();
			Assert.Equal(5, _csvReader.Fill(dataSet, "my-table"));
			Assert.NotNull(dataSet.Tables["my-table"]);

			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			dataSet = new DataSet();
			Assert.Equal(2, _csvReader.Fill(dataSet, 2));

			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			dataSet = new DataSet();
			Assert.Equal(3, _csvReader.Fill(dataSet, "my-table", 3));
			Assert.NotNull(dataSet.Tables["my-table"]);
		}

		[Fact]
		public void TestFillNullDataSet()
		{
			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			Assert.Throws<ArgumentNullException>(() => _csvReader.Fill(null));
		}

		[Fact]
		public void TestFillNullTableName()
		{
			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			Assert.Throws<ArgumentNullException>(() => _csvReader.Fill(new DataSet(), null));
		}

		[Fact]
		public void TestFillInvalidMaximum()
		{
			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			Assert.Throws<ArgumentException>(() => _csvReader.Fill(new DataSet(), -10));
		}

		[Fact]
		public void TestFillNoHeaderRecord()
		{
			Init("csv.fill");
			Assert.Throws<InvalidOperationException>(() => _csvReader.Fill(new DataSet()));
		}

		[Fact]
		public void TestFillTableAlreadyExists()
		{
			Init("csv.fill");
			_csvReader.ReadHeaderRecord();
			DataSet dataSet = new DataSet();
			dataSet.Tables.Add("my-table");
			Assert.Throws<DuplicateNameException>(() => _csvReader.Fill(dataSet, "my-table"));
		}

		[Fact]
		public void TestFillTooManyColumns()
		{
			Init("csv.fill.too-many-columns");
			_csvReader.ReadHeaderRecord();
			var ex = Assert.Throws<InvalidOperationException>(() => _csvReader.Fill(new DataSet(), "my-table"));
			Assert.Equal("A record has 4 columns in it, but the header only defines 3.", ex.Message);
		}




		
		/*

		[Fact]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void TestReadAfterClose()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Tempany,0{0}Kent,25", Environment.NewLine));
			_csvReader.ReadDataRecord();
			_csvReader.Close();
			_csvReader.ReadDataRecord();
		}


		[Fact]
		public void TestSetHeader()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Tempany,0{0}Kent,25", Environment.NewLine));
			_csvReader.HeaderRecord = new HeaderRecord(new string[] {"Name","Age"});
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("Tempany", record["Name"]);
			Assert.Equal("0", record["Age"]);
			record = _csvReader.ReadDataRecord();
			Assert.Equal("Kent", record["Name"]);
			Assert.Equal("25", record["Age"]);
			Assert.Null(_csvReader.ReadDataRecord());
		}

		[Fact]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestSetHeaderNull()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Tempany,0{0}Kent,25", Environment.NewLine));
			_csvReader.HeaderRecord = null;
		}

		[Fact]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestSetHeaderTwice()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Tempany,0{0}Kent,25", Environment.NewLine));
			_csvReader.HeaderRecord = new HeaderRecord(new string[] {"Name","Age"});
			_csvReader.HeaderRecord = new HeaderRecord(new string[] {"Name","Age"});
		}

		[Fact]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestSetHeaderAfterRecord()
		{
			_csvReader = CsvReader.FromCsvString(string.Format("Tempany,0{0}Kent,25", Environment.NewLine));
			_csvReader.ReadDataRecord();
			_csvReader.HeaderRecord = new HeaderRecord(new string[] {"Name","Age"});
		}



		[Fact]
		[ExpectedException(typeof(ArgumentException))]
		public void TestSameSeparatorAndDelimiter()
		{
			Init("csv.escaped-data", '-', '-');
		}

		[Fact]
		public void TestWhitespacePreserveNone()
		{
			_csvReader = CsvReader.FromCsvString("  Kent  ,  Belinda  , \" Tempany   \"  ,  ,  a  ,  b", CsvReaderOptions.None);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("Kent", record[0]);
			Assert.Equal("Belinda", record[1]);
			Assert.Equal(" Tempany   ", record[2]);
			Assert.Equal("", record[3]);
			Assert.Equal("a", record[4]);
			Assert.Equal("b", record[5]);
		}

		[Fact]
		public void TestWhitespacePreserveLeading()
		{
			_csvReader = CsvReader.FromCsvString("  Kent  ,  Belinda  , \" Tempany   \"  ,  ,  a  ,  b", CsvReaderOptions.PreserveLeadingWhiteSpace);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("  Kent", record[0]);
			Assert.Equal("  Belinda", record[1]);
			Assert.Equal("  Tempany   ", record[2]);
			Assert.Equal("", record[3]);
			Assert.Equal("  a", record[4]);
			Assert.Equal("  b", record[5]);
		}

		[Fact]
		public void TestWhitespacePreserveTrailing()
		{
			_csvReader = CsvReader.FromCsvString("  Kent  ,  Belinda  , \" Tempany   \"  ,  ,  a  ,  b", CsvReaderOptions.PreserveTrailingWhiteSpace);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("Kent  ", record[0]);
			Assert.Equal("Belinda  ", record[1]);
			Assert.Equal(" Tempany     ", record[2]);
			Assert.Equal("", record[3]);
			Assert.Equal("a  ", record[4]);
			Assert.Equal("b", record[5]);
		}

		[Fact]
		public void TestWhitespacePreserveAll()
		{
			_csvReader = CsvReader.FromCsvString("  Kent  ,  Belinda  , \" Tempany   \"  ,  ,  a  ,  b", CsvReaderOptions.PreserveLeadingWhiteSpace | CsvReaderOptions.PreserveTrailingWhiteSpace);
			DataRecord record = _csvReader.ReadDataRecord();
			Assert.Equal("  Kent  ", record[0]);
			Assert.Equal("  Belinda  ", record[1]);
			Assert.Equal("  Tempany     ", record[2]);
			Assert.Equal("  ", record[3]);
			Assert.Equal("  a  ", record[4]);
			Assert.Equal("  b", record[5]);
		}
		*/

		private void Init(string key)
		{
			if (_csvReader != null)
			{
				_csvReader.Close();
			}

			_csvReader = new CsvReader(GetConfigurationStream(key));
		}

		private void Init(string key, char separator)
		{
			_csvReader = new CsvReader(GetConfigurationStream(key));
			_csvReader.ValueSeparator = separator;
		}

		private void Init(string key, char separator, char delimiter)
		{
			_csvReader = new CsvReader(GetConfigurationStream(key));
			_csvReader.ValueSeparator = separator;
			_csvReader.ValueDelimiter = delimiter;
		}

		private Stream GetConfigurationStream(string key)
		{
			ResourceManager resourceManager = new ResourceManager("Kent.Boogaart.KBCsv.UnitTest.Resources", Assembly.GetExecutingAssembly());
			string xml = resourceManager.GetString(key);

			if (xml == null)
			{
				throw new NullReferenceException(string.Format("No resource found under key '{0}'", key));
			}

			byte[] xmlBytes = new UTF8Encoding().GetBytes(xml);
			MemoryStream retVal = new MemoryStream(xmlBytes);
			return retVal;
		}
	}
}
