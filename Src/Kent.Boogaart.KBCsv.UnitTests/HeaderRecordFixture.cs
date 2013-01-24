namespace Kent.Boogaart.KBCsv.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public sealed class HeaderRecordFixture
    {
        [Fact]
        public void public_constructor_throws_if_multiple_columns_have_the_same_name()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new HeaderRecord(new string[] { "column1", "column2", "column3", "column2", "column5" }));
            Assert.Equal("A column named 'column2' appears more than once in the header record.", ex.Message);
        }

        [Fact]
        public void public_constructor_throws_if_values_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new HeaderRecord(false, (string[])null));
        }

        [Fact]
        public void public_constructor_throws_if_any_value_is_null()
        {
            Assert.Throws<ArgumentException>(() => new HeaderRecord("column1", null, "column3"));
        }

        [Fact]
        public void public_constructor_assigns_empty_values_if_columns_array_is_empty()
        {
            var header = new HeaderRecord();
            Assert.Equal(0, header.Count);
        }

        [Fact]
        public void public_constructor_assigns_columns_as_values()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            Assert.Equal(3, header.Count);
            Assert.Equal("Name", header[0]);
            Assert.Equal("Age", header[1]);
            Assert.Equal("Gender", header[2]);
        }

        [Fact]
        public void internal_constructor_with_array_results_in_read_only_record()
        {
            var header = new HeaderRecord((IList<string>)new string[] { "Name", "Age", "Gender" });
            Assert.True(header.IsReadOnly);
        }

        [Fact]
        public void indexer_throws_if_column_is_not_found()
        {
            var header = new HeaderRecord(new string[] { "Name", "Age", "Gender" });
            var i = 0;
            var ex = Assert.Throws<ArgumentException>(() => i = header["foobar"]);
            Assert.Equal("No column named 'foobar' was found in the header record.", ex.Message);
        }

        [Fact]
        public void indexer_returns_column_index()
        {
            var header = new HeaderRecord(new string[] { "Name", "Age", "Gender" });
            Assert.Equal(0, header["Name"]);
            Assert.Equal(1, header["Age"]);
            Assert.Equal(2, header["Gender"]);
        }

        [Fact]
        public void get_column_index_or_null_returns_null_if_column_is_not_found()
        {
            var header = new HeaderRecord(new string[] { "Name", "Age", "Gender" });
            Assert.Null(header.GetColumnIndexOrNull("Foo"));
            Assert.Null(header.GetColumnIndexOrNull("name"));
            Assert.Null(header.GetColumnIndexOrNull("Age "));
            Assert.Null(header.GetColumnIndexOrNull("GENDER"));
        }

        [Fact]
        public void get_column_index_or_null_returns_column_index_if_present()
        {
            var header = new HeaderRecord(new string[] { "Name", "Age", "Gender" });
            Assert.Equal(0, header.GetColumnIndexOrNull("Name"));
            Assert.Equal(1, header.GetColumnIndexOrNull("Age"));
            Assert.Equal(2, header.GetColumnIndexOrNull("Gender"));
        }

        [Fact]
        public void add_throws_if_there_is_already_a_column_with_the_same_name()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            var ex = Assert.Throws<InvalidOperationException>(() => header.Add("Age"));
            Assert.Equal("A column named 'Age' appears more than once in the header record.", ex.Message);
        }

        [Fact]
        public void insert_throws_if_there_is_already_a_column_with_the_same_name()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            var ex = Assert.Throws<InvalidOperationException>(() => header.Insert(0, "Age"));
            Assert.Equal("A column named 'Age' appears more than once in the header record.", ex.Message);
        }

        [Fact]
        public void indexer_set_updates_indexes_appropriately()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            Assert.Equal(1, header["Age"]);
            header[1] = "New";
            Assert.Equal(1, header["New"]);
            Assert.Null(header.GetColumnIndexOrNull("Age"));
        }

        [Fact]
        public void clear_updates_indexes_appropriately()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            Assert.Equal(0, header["Name"]);
            Assert.Equal(1, header["Age"]);
            Assert.Equal(2, header["Gender"]);
            header.Clear();
            Assert.Null(header.GetColumnIndexOrNull("Name"));
            Assert.Null(header.GetColumnIndexOrNull("Age"));
            Assert.Null(header.GetColumnIndexOrNull("Gender"));
        }

        [Fact]
        public void add_updates_indexes_appropriately()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            header.Add("Address");
            Assert.Equal(0, header["Name"]);
            Assert.Equal(1, header["Age"]);
            Assert.Equal(2, header["Gender"]);
            Assert.Equal(3, header["Address"]);
        }

        [Fact]
        public void insert_updates_indexes_appropriately()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            header.Insert(1, "Address");
            Assert.Equal(0, header["Name"]);
            Assert.Equal(1, header["Address"]);
            Assert.Equal(2, header["Age"]);
            Assert.Equal(3, header["Gender"]);
        }

        [Fact]
        public void remove_updates_indexes_appropriately()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            header.Remove("Age");
            Assert.Equal(0, header["Name"]);
            Assert.Equal(1, header["Gender"]);
        }

        [Fact]
        public void remove_at_updates_indexes_appropriately()
        {
            var header = new HeaderRecord("Name", "Age", "Gender");
            header.RemoveAt(1);
            Assert.Equal(0, header["Name"]);
            Assert.Equal(1, header["Gender"]);
        }
    }
}