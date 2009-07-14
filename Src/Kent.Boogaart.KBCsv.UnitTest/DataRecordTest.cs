using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
	/// <summary>
	/// Unit tests the <see cref="DataRecord"/> class.
	/// </summary>
	public sealed class DataRecordTest
	{
		private DataRecord _data;

		[Fact]
		public void TestHeaderConstructorNull()
		{
			_data = new DataRecord(null);
			Assert.NotNull(_data.Values);
			Assert.Equal(0, _data.Values.Count);
		}

		[Fact]
		public void TestHeaderConstructor()
		{
			HeaderRecord headerRecord = new HeaderRecord(new string[] { "column1", "column2" });
			_data = new DataRecord(headerRecord);
			Assert.Same(headerRecord, _data.HeaderRecord);
			_data.Values.Add("value1");
			_data.Values.Add("value2");
			Assert.Equal("value1", _data["column1"]);
			Assert.Equal("value2", _data["column2"]);
		}

		[Fact]
		public void TestSerializable()
		{
			_data = new DataRecord(null, new string[] {"Kent", "25", "M"});
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(new MemoryStream(), _data);
		}

		[Fact]
		public void TestNull()
		{
			Assert.Throws<ArgumentNullException>(() => _data = new DataRecord(null, null));
		}

		[Fact]
		public void TestEmpty()
		{
			_data = new DataRecord(null, new string[0]);
			Assert.Equal(0, _data.Values.Count);
		}

		[Fact]
		public void TestWithValues()
		{
			_data = new DataRecord(null, new string[] {"Kent", "25", "M"});
			Assert.Equal(3, _data.Values.Count);
			Assert.Equal("Kent", _data[0]);
			Assert.Equal("25", _data[1]);
			Assert.Equal("M", _data[2]);
		}

		[Fact]
		public void TestEquals()
		{
			DataRecord record1;
			DataRecord record2;

			//wrong type
			record1 = new DataRecord(null, new string[] {});
			Assert.False(record1.Equals(1));

			//no header, empty values
			record1 = new DataRecord(null, new string[] {});
			record2 = new DataRecord(null, new string[] {});
			Assert.Equal(record1, record2);

			//empty header, empty values
			record1 = new DataRecord(new HeaderRecord(new string[] {}), new string[] {});
			record2 = new DataRecord(new HeaderRecord(new string[] {}), new string[] {});
			Assert.Equal(record1, record2);

			//no header, non-equal values
			record1 = new DataRecord(null, new string[] {"Kent", "25", "M"});
			record2 = new DataRecord(null, new string[] {"Kent", "26", "M"});
			Assert.False(record1.Equals(record2));

			//equal header, non-equal values
			record1 = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "25", "M"});
			record2 = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "26", "M"});
			Assert.False(record1.Equals(record2));

			//non-equal header, equal values
			record1 = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "25", "M"});
			record2 = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Sex"}), new string[] {"Kent", "25", "M"});
			Assert.False(record1.Equals(record2));

			//equal header, equal values
			record1 = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "25", "M"});
			record2 = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "25", "M"});
			Assert.Equal(record1, record2);
		}

		[Fact]
		public void TestGetHashCode()
		{
			DataRecord record1 = new DataRecord(null, new string[] {"value1", "value2", "value3"});
			DataRecord record2 = new DataRecord(null, new string[] { "value1", "value2", "value3" });
			Assert.True(record1.GetHashCode() == record2.GetHashCode());

			record1[2] += "changed";
			Assert.False(record1.GetHashCode() == record2.GetHashCode());

			record1 = new DataRecord(null, new string[] { "value1", "value2", "value3" }, true);
			Assert.True(record1.GetHashCode() == record2.GetHashCode());

			HeaderRecord headerRecord = new HeaderRecord(new string[] { "column1", "column2", "column3" });
			record2 = new DataRecord(headerRecord, new string[] { "value1", "value2", "value3" });
			Assert.False(record1.GetHashCode() == record2.GetHashCode());

			record1 = new DataRecord(headerRecord, new string[] { "value1", "value2", "value3" });
			Assert.True(record1.GetHashCode() == record2.GetHashCode());
		}

		[Fact]
		public void TestStringIndexer()
		{
			_data = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "25", "M"});
			Assert.Equal(3, _data.Values.Count);
			Assert.Equal("Kent", _data["Name"]);
			Assert.Equal("25", _data["Age"]);
			Assert.Equal("M", _data["Gender"]);
		}

		[Fact]
		public void TestStringIndexerInvalid()
		{
			_data = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "25", "M"});
			string s;
			Assert.Throws<ArgumentException>(() => s = _data["foobar"]);
		}

		[Fact]
		public void TestValuesReadOnly()
		{
			_data = new DataRecord(new HeaderRecord(new string[] {"Name", "Age", "Gender"}), new string[] {"Kent", "25", "M"}, true);
			var ex = Assert.Throws<NotSupportedException>(() => _data.Values[0] = "NEW VALUE");
			Assert.Equal("Collection is read-only.", ex.Message);
		}

		[Fact]
		public void TestFromParserNull()
		{
			//FromTextReader is internal so no check is made against the argument
			Assert.Throws<NullReferenceException>(() => _data = DataRecord.FromParser(null, null));
		}

		[Fact]
		public void TestFromParserNoRecord()
		{
			CsvParser parser = new CsvParser(new StringReader("field1,field2"));
			parser.ValueDelimiter = '\'';
			parser.SkipRecord();
			_data = DataRecord.FromParser(null, parser);
			Assert.Null(_data);
		}

		[Fact]
		public void TestFromParser()
		{
			CsvParser parser = new CsvParser(new StringReader("field1,field2,'  field3  '"));
			parser.ValueDelimiter = '\'';
			_data = DataRecord.FromParser(null, parser);
			Assert.Equal(3, _data.Values.Count);
			Assert.Equal("field1", _data.Values[0]);
			Assert.Equal("field2", _data.Values[1]);
			Assert.Equal("  field3  ", _data.Values[2]);
		}
	}
}
