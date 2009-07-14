using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
	/// <summary>
	/// Unit tests the <see cref="HeaderRecord"/> class.
	/// </summary>
	public sealed class HeaderRecordTest
	{
		private HeaderRecord _header;

		[Fact]
		public void TestDefaultConstructor()
		{
			_header = new HeaderRecord();
			Assert.NotNull(_header.Values);
			Assert.Equal(0, _header.Values.Count);
		}

		[Fact]
		public void TestDuplicateColumnName()
		{
			var ex  = Assert.Throws<InvalidOperationException>(() => new HeaderRecord(new string[] {"column1", "column2", "column3", "column2", "column5"}));
			Assert.Equal("A column named 'column2' appears more than once in the header record.", ex.Message);
		}

		[Fact]
		public void TestSerializable()
		{
			_header = new HeaderRecord(new string[] {"Name", "Age", "Gender"});
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(new MemoryStream(), _header);
		}

		[Fact]
		public void TestNull()
		{
			Assert.Throws<ArgumentNullException>(() => new HeaderRecord(null));
		}

		[Fact]
		public void TestEmpty()
		{
			_header = new HeaderRecord(new string[0]);
			Assert.Equal(0, _header.Values.Count);
		}

		[Fact]
		public void TestEquals()
		{
			HeaderRecord record1;
			HeaderRecord record2;

			//wrong type
			record1 = new HeaderRecord(new string[] {});
			Assert.False(record1.Equals(1));

			//empty values
			record1 = new HeaderRecord(new string[] {});
			record2 = new HeaderRecord(new string[] {});
			Assert.Equal(record1, record2);

			//non-equal values
			record1 = new HeaderRecord(new string[] {"Name", "Age", "Gender"});
			record2 = new HeaderRecord(new string[] {"Name", "Age", "Sex"});
			Assert.False(record1.Equals(record2));

			//equal values
			record1 = new HeaderRecord(new string[] {"Name", "Age", "Gender"});
			record2 = new HeaderRecord(new string[] {"Name", "Age", "Gender"});
			Assert.Equal(record1, record2);
		}

		[Fact]
		public void TestWithValues()
		{
			_header = new HeaderRecord(new string[] {"Name", "Age", "Gender"});
			Assert.Equal(3, _header.Values.Count);
			Assert.Equal("Name", _header[0]);
			Assert.Equal("Age", _header[1]);
			Assert.Equal("Gender", _header[2]);
		}

		[Fact]
		public void TestStringIndexer()
		{
			_header = new HeaderRecord(new string[] {"Name", "Age", "Gender"});
			Assert.Equal(3, _header.Values.Count);
			Assert.Equal(0, _header["Name"]);
			Assert.Equal(1, _header["Age"]);
			Assert.Equal(2, _header["Gender"]);
		}

		[Fact]
		public void TestStringIndexerInvalid()
		{
			_header = new HeaderRecord(new string[] {"Name", "Age", "Gender"});
			int i = 0;
			Assert.Throws<ArgumentException>(() => i = _header["foobar"]);
		}

		[Fact]
		public void TestValuesReadOnly()
		{
			_header = new HeaderRecord(new string[] {"Name", "Age", "Gender"}, true);
			var ex = Assert.Throws<NotSupportedException>(() => _header.Values[0] = "NEW VALUE");
			Assert.Equal("Collection is read-only.", ex.Message);
		}

		[Fact]
		public void TestFromParserNull()
		{
			//FromTextReader is internal so no check is made against the argument
			Assert.Throws<NullReferenceException>(() => _header = HeaderRecord.FromParser(null));
		}

		[Fact]
		public void TestFromParserNoRecord()
		{
			CsvParser parser = new CsvParser(new StringReader("column1,column2"));
			parser.ValueDelimiter = '\'';
			parser.SkipRecord();
			_header = HeaderRecord.FromParser(parser);
			Assert.Null(_header);
		}

		[Fact]
		public void TestFromParser()
		{
			CsvParser parser = new CsvParser(new StringReader("column1,column2,'  column3  '"));
			parser.ValueDelimiter = '\'';
			_header = HeaderRecord.FromParser(parser);
			Assert.Equal(3, _header.Values.Count);
			Assert.Equal("column1", _header.Values[0]);
			Assert.Equal("column2", _header.Values[1]);
			Assert.Equal("  column3  ", _header.Values[2]);
		}
	}
}
