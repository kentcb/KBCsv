namespace Kent.Boogaart.KBCsv.UnitTest
{
    using System;
    using Xunit;

    public sealed class DataRecordFixture
    {
        [Fact]
        public void public_constructor_assigns_given_header_record()
        {
            var header = new HeaderRecord();
            var data = new DataRecord(header);
            Assert.NotNull(data.HeaderRecord);
            Assert.Same(header, data.HeaderRecord);
        }

        [Fact]
        public void indexer_get_throws_if_column_not_found()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }), new string[] { "Kent", "25", "M" });
            string s;

            Assert.Throws<ArgumentException>(() => s = data["foo"]);
            Assert.Throws<ArgumentException>(() => s = data["name"]);
            var ex = Assert.Throws<ArgumentException>(() => s = data["GENDER"]);
            Assert.Equal("No column named 'GENDER' was found in the header record.", ex.Message);
        }

        [Fact]
        public void indexer_get_throws_if_there_is_no_header()
        {
            var data = new DataRecord(null);
            var ex = Assert.Throws<InvalidOperationException>(() => data["anything"]);
            Assert.Equal("No header record is associated with this DataRecord.", ex.Message);
        }

        [Fact]
        public void indexer_get_throws_if_column_name_is_null()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }));
            var ex = Assert.Throws<ArgumentNullException>(() => data[(string)null]);
        }

        [Fact]
        public void indexer_get_returns_corresponding_value_for_column()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }), new string[] { "Kent", "25", "M" });

            Assert.Equal("Kent", data["Name"]);
            Assert.Equal("25", data["Age"]);
            Assert.Equal("M", data["Gender"]);
        }

        [Fact]
        public void indexer_set_throws_if_column_not_found()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }), new string[] { "Kent", "25", "M" });
            var ex = Assert.Throws<ArgumentException>(() => data["Whatever"] = "whatever");
            Assert.Equal("No column named 'Whatever' was found in the header record.", ex.Message);
        }

        [Fact]
        public void indexer_set_throws_if_column_name_is_null()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }));
            var ex = Assert.Throws<ArgumentNullException>(() => data[(string)null] = "whatever");
        }

        [Fact]
        public void indexer_set_throws_if_there_is_no_header()
        {
            var data = new DataRecord(null);
            var ex = Assert.Throws<InvalidOperationException>(() => data["anything"] = "whatever");
            Assert.Equal("No header record is associated with this DataRecord.", ex.Message);
        }

        [Fact]
        public void get_value_or_null_throws_if_there_is_no_header()
        {
            var data = new DataRecord(null);
            var ex = Assert.Throws<InvalidOperationException>(() => data.GetValueOrNull("anything"));
            Assert.Equal("No header record is associated with this DataRecord.", ex.Message);
        }

        [Fact]
        public void get_value_or_null_throws_if_column_name_is_null()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }));
            var ex = Assert.Throws<ArgumentNullException>(() => data.GetValueOrNull(null));
        }

        [Fact]
        public void get_value_or_null_returns_null_if_column_is_not_found()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender", "Relationship Status" }), new string[] { "Kent", "25", "M" }, true);

            Assert.Null(data.GetValueOrNull("foo"));
            Assert.Null(data.GetValueOrNull("name"));
            Assert.Null(data.GetValueOrNull("GENDER"));
            Assert.Null(data.GetValueOrNull("Relationship Status"));
        }

        [Fact]
        public void get_value_or_default_returns_value_in_column()
        {
            var data = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender", "Relationship Status" }), new string[] { "Kent", "25", "M" }, true);

            Assert.Equal("Kent", data.GetValueOrNull("Name"));
            Assert.Equal("25", data.GetValueOrNull("Age"));
            Assert.Equal("M", data.GetValueOrNull("Gender"));
        }

        [Fact]
        public void equals_returns_false_if_header_values_differ()
        {
            var data1 = new DataRecord(new HeaderRecord(new string[] { "Name" }), new string[] { "Kent" });
            var data2 = new DataRecord(new HeaderRecord(new string[] { "name" }), new string[] { "Kent" });

            Assert.False(data1.Equals(data2));
            Assert.False(data2.Equals(data1));
        }

        [Fact]
        public void equals_returns_false_if_one_record_has_header_and_other_does_not()
        {
            var data1 = new DataRecord(new HeaderRecord(new string[] { "Name" }), new string[] { "Kent" });
            var data2 = new DataRecord(null, new string[] { "Kent" });

            Assert.False(data1.Equals(data2));
            Assert.False(data2.Equals(data1));
        }

        [Fact]
        public void equals_returns_true_if_headers_and_values_match()
        {
            var data1 = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }), new string[] { "Kent", "M", "25" });
            var data2 = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }), new string[] { "Kent", "M", "25" });

            Assert.True(data1.Equals(data2));
            Assert.True(data2.Equals(data1));
        }

        [Fact]
        public void get_hash_code_returns_same_hash_for_equal_records()
        {
            var data1 = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }), new string[] { "Kent", "M", "25" });
            var data2 = new DataRecord(new HeaderRecord(new string[] { "Name", "Age", "Gender" }), new string[] { "Kent", "M", "25" });

            Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
        }
    }
}
