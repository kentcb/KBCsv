using System;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest
{
	/// <summary>
	/// Unit tests the <see cref="RecordBase"/> class.
	/// </summary>
	public sealed class RecordBaseTest
	{
		private RecordBase _record;

		public RecordBaseTest()
		{
			_record = new ConcreteRecordBase();
		}

		[Fact]
		public void TestDefaultConstructor()
		{
			Assert.NotNull(_record.Values);
			Assert.Equal(0, _record.Values.Count);
		}

		[Fact]
		public void TestValuesConstructor()
		{
			_record = new ConcreteRecordBase(new string[] {"value1", "value2", "value3", "value4"});
			Assert.NotNull(_record.Values);
			Assert.Equal(4, _record.Values.Count);
			Assert.Equal("value1", _record.Values[0]);
			Assert.Equal("value2", _record.Values[1]);
			Assert.Equal("value3", _record.Values[2]);
			Assert.Equal("value4", _record.Values[3]);
			Assert.False(_record.Values.IsReadOnly);
		}

		[Fact]
		public void TestValuesBoolConstructor()
		{
			_record = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" }, true);
			Assert.NotNull(_record.Values);
			Assert.Equal(4, _record.Values.Count);
			Assert.Equal("value1", _record.Values[0]);
			Assert.Equal("value2", _record.Values[1]);
			Assert.Equal("value3", _record.Values[2]);
			Assert.Equal("value4", _record.Values[3]);
			Assert.True(_record.Values.IsReadOnly);
		}

		[Fact]
		public void TestValuesContructorNull()
		{
			Assert.Throws<ArgumentNullException>(() => _record = new ConcreteRecordBase(null));
		}

		[Fact]
		public void TestIndexerNegativeIndex()
		{
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _record[-1] = "test");
			Assert.Equal("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", ex.Message);
		}

		[Fact]
		public void TestIndexerInvalidIndex()
		{
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _record[12] = "test");
			Assert.Equal("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", ex.Message);
		}

		[Fact]
		public void TestIndexer()
		{
			_record = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" });
			Assert.Equal("value2", _record.Values[1]);
			_record.Values[1] = "new value";
			Assert.Equal("new value", _record.Values[1]);
			Assert.Equal("value4", _record.Values[3]);
			_record.Values[3] = "another new value";
			Assert.Equal("another new value", _record.Values[3]);
		}

		[Fact]
		public void TestEquals()
		{
			RecordBase record1 = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" });
			RecordBase record2 = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" });
			Assert.True(record1.Equals(record2));

			//slightly different value in one record
			record2[1] += " ";
			Assert.False(record1.Equals(record2));

			//same change made to first record
			record1[1] += " ";
			Assert.True(record1.Equals(record2));

			//extra value in 1 record
			record1 = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" });
			record2 = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4", "value5" });
			Assert.False(record1.Equals(record2));

			//one record is readonly
			record1 = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" });
			record2 = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" }, true);
			Assert.True(record1.Equals(record2));

			//both records empty
			record1 = new ConcreteRecordBase();
			record2 = new ConcreteRecordBase();
			Assert.True(record1.Equals(record2));

			//null argument
			Assert.False(record1.Equals(null));
			Assert.False(record2.Equals(null));
		}

		[Fact]
		public void TestGetHashCode()
		{
			_record = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" });
			int originalHash = _record.GetHashCode();
			_record[1] += "changed";
			//hash code shouldn't change if a value is changed
//			Assert.Equal(originalHash, _record.GetHashCode());

			//two objects with same value must have equal hash codes
			RecordBase record2 = new ConcreteRecordBase(new string[] { "value1", "value2changed", "value3", "value4" });
			Assert.Equal(_record.GetHashCode(), record2.GetHashCode());
		}

		[Fact]
		public void TestToString()
		{
			_record = new ConcreteRecordBase(new string[] { "value1", "value2", "value3", "value4" });
			Assert.Equal(string.Format("value1{0}value2{0}value3{0}value4{0}", RecordBase.ValueSeparator), _record.ToString());
			_record[1] += " ";
			Assert.Equal(string.Format("value1{0}value2 {0}value3{0}value4{0}", RecordBase.ValueSeparator), _record.ToString());
			_record.Values.Add("a new value");
			Assert.Equal(string.Format("value1{0}value2 {0}value3{0}value4{0}a new value{0}", RecordBase.ValueSeparator), _record.ToString());
		}

		#region Supporting Types

		private sealed class ConcreteRecordBase : RecordBase
		{
			public ConcreteRecordBase()
			{
			}

			public ConcreteRecordBase(string[] values)
				: base(values)
			{
			}

			public ConcreteRecordBase(string[] values, bool readOnly)
				: base(values, readOnly)
			{
			}
		}

		#endregion
	}
}
