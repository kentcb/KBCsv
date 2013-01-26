namespace Kent.Boogaart.KBCsv.Extensions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Kent.Boogaart.KBCsv;
    using Xunit;

    public sealed class EnumerableExtensionsFixture
    {
        [Fact]
        public void write_csv_throws_if_enumerable_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<DateTime>)null).WriteCsv(writer));
        }

        [Fact]
        public void write_csv_throws_if_csv_writer_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new List<DateTime>().WriteCsv(null));
        }

        [Fact]
        public void write_csv_throws_if_property_names_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            Assert.Throws<ArgumentNullException>(() => new List<DateTime>().WriteCsv(writer, true, null));
        }

        [Fact]
        public void write_csv_throws_if_any_property_name_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            var ex = Assert.Throws<ArgumentException>(() => new List<DateTime>().WriteCsv(writer, true, new string[] { "Date", null }));
            Assert.Equal("A property name is null.", ex.Message);
        }

        [Fact]
        public void write_csv_throws_if_any_property_cannot_be_resolved()
        {
            var writer = new CsvWriter(new StringWriter());
            var ex = Assert.Throws<InvalidOperationException>(() => new List<DateTime>().WriteCsv(writer, true, new string[] { "Date", "Foo" }));
            Assert.Equal("Property 'Foo' was not found on type 'System.DateTime'.", ex.Message);
        }

        [Fact]
        public void write_csv_throws_if_object_to_string_converter_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            Assert.Throws<ArgumentNullException>(() => new List<DateTime>().WriteCsv(writer, true, new string[] { "Date" }, null));
        }

        [Fact]
        public void write_csv_throws_if_csv_writer_is_disposed()
        {
            var writer = new CsvWriter(new StringWriter());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(() => new DateTime[0].WriteCsv(writer));
        }

        [Fact]
        public void write_csv_writes_header_record_by_default()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                new TestType1[0].WriteCsv(writer);
                var result = stringWriter.ToString();

                // can't assert exact contents because order of properties is undefined (and changes)
                Assert.Contains("Property1", result);
                Assert.Contains("Property2", result);
            }
        }

        [Fact]
        public void write_csv_writes_all_public_properties_by_default()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType1[]
                {
                    new TestType1
                    {
                        Property1 = "1",
                        Property2 = "2",
                        Property3 = "3"
                    },
                    new TestType1
                    {
                        Property1 = "4",
                        Property2 = "5",
                        Property3 = "6"
                    }
                };

                items.WriteCsv(writer);
                var result = stringWriter.ToString();

                // can't assert exact contents because order of properties is undefined (and changes)
                Assert.Contains("Property1", result);
                Assert.Contains("Property2", result);
                Assert.Contains("1", result);
                Assert.Contains("2", result);
                Assert.Contains("4", result);
                Assert.Contains("5", result);
            }
        }

        [Fact]
        public void write_csv_writes_only_requested_properties_if_specified()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType1[]
                {
                    new TestType1
                    {
                        Property1 = "1",
                        Property2 = "2",
                        Property3 = "3"
                    },
                    new TestType1
                    {
                        Property1 = "4",
                        Property2 = "5",
                        Property3 = "6"
                    }
                };

                items.WriteCsv(writer, true, new string[] { "Property2" });
                Assert.Equal("Property2<EOL>2<EOL>5<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_writes_converts_property_values_to_strings()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType2[]
                {
                    new TestType2
                    {
                        Property1 = "1",
                        Property2 = 2d,
                        Property3 = 3,
                        Property4 = 4m
                    },
                    new TestType2
                    {
                        Property1 = "5",
                        Property2 = 6d,
                        Property3 = 7,
                        Property4 = 8m
                    }
                };

                items.WriteCsv(writer);
                Assert.Equal("Property1,Property2,Property3,Property4<EOL>1,2,3,4<EOL>5,6,7,8<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_writes_converts_null_property_values_to_empty_strings()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType2[]
                {
                    new TestType2
                    {
                        Property1 = null,
                        Property2 = 2d,
                        Property3 = 3,
                        Property4 = 4m
                    },
                    new TestType2
                    {
                        Property1 = null,
                        Property2 = 6d,
                        Property3 = 7,
                        Property4 = null
                    }
                };

                items.WriteCsv(writer);
                Assert.Equal("Property1,Property2,Property3,Property4<EOL>,2,3,4<EOL>,6,7,<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_uses_object_to_string_converter_if_specified()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType2[]
                {
                    new TestType2
                    {
                        Property1 = "1",
                        Property2 = 2d,
                        Property3 = 3,
                        Property4 = 4m
                    },
                    new TestType2
                    {
                        Property1 = "5",
                        Property2 = 6d,
                        Property3 = 7,
                        Property4 = 8m
                    }
                };

                items.WriteCsv(writer, true, new string[] { "Property1", "Property2" }, o => o.ToString() + "_SUFFIX");
                Assert.Equal("Property1,Property2<EOL>1_SUFFIX,2_SUFFIX<EOL>5_SUFFIX,6_SUFFIX<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_returns_number_of_items_written()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                writer.WriteRecord("some", "record");

                var items = new TestType1[]
                {
                    new TestType1
                    {
                        Property1 = "1",
                        Property2 = "2",
                        Property3 = "3"
                    },
                    new TestType1
                    {
                        Property1 = "4",
                        Property2 = "5",
                        Property3 = "6"
                    }
                };

                Assert.Equal(2, items.WriteCsv(writer));
                Assert.Equal(2, items.WriteCsv(writer, false));
            }
        }

        [Fact]
        public void write_csv_non_reflection_throws_if_object_to_record_converter_is_null()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                var header = new string[] { "The Year", "Even Year?" };
                Assert.Throws<ArgumentNullException>(() => new DateTime[0].WriteCsv(writer, header, null));
            }
        }

        [Fact]
        public void write_csv_non_reflection_overload_allows_arbitrary_conversion_of_objects_to_csv()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new DateTime[]
                {
                    new DateTime(2004, 12, 31),
                    new DateTime(1978, 12, 4),
                    new DateTime(1979, 10, 26)
                };

                var header = new string[] { "The Year", "Even Year?" };
                items.WriteCsv(writer, header, dt => new string[] { dt.Year.ToString(CultureInfo.InvariantCulture), (dt.Year % 2 == 0).ToString(CultureInfo.InvariantCulture) });
                Assert.Equal("The Year,Even Year?<EOL>2004,True<EOL>1978,True<EOL>1979,False<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public void write_csv_async_throws_if_enumerable_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            Assert.Throws<ArgumentNullException>(((IEnumerable<DateTime>)null).WriteCsvAsync(writer));
        }

        [Fact]
        public void write_csv_async_throws_if_csv_writer_is_null()
        {
            Assert.Throws<ArgumentNullException>(new List<DateTime>().WriteCsvAsync(null));
        }

        [Fact]
        public void write_csv_async_throws_if_property_names_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            Assert.Throws<ArgumentNullException>(new List<DateTime>().WriteCsvAsync(writer, true, null));
        }

        [Fact]
        public void write_csv_async_throws_if_any_property_name_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            var ex = Assert.Throws<ArgumentException>(new List<DateTime>().WriteCsvAsync(writer, true, new string[] { "Date", null }));
            Assert.Equal("A property name is null.", ex.Message);
        }

        [Fact]
        public void write_csv_async_throws_if_any_property_cannot_be_resolved()
        {
            var writer = new CsvWriter(new StringWriter());
            var ex = Assert.Throws<InvalidOperationException>(new List<DateTime>().WriteCsvAsync(writer, true, new string[] { "Date", "Foo" }));
            Assert.Equal("Property 'Foo' was not found on type 'System.DateTime'.", ex.Message);
        }

        [Fact]
        public void write_csv_async_throws_if_object_to_string_converter_is_null()
        {
            var writer = new CsvWriter(new StringWriter());
            Assert.Throws<ArgumentNullException>(new List<DateTime>().WriteCsvAsync(writer, true, new string[] { "Date" }, null));
        }

        [Fact]
        public void write_csv_async_throws_if_csv_writer_is_disposed()
        {
            var writer = new CsvWriter(new StringWriter());
            writer.Dispose();
            Assert.Throws<ObjectDisposedException>(new DateTime[0].WriteCsvAsync(writer));
        }

        [Fact]
        public async void write_csv_async_writes_header_record_by_default()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                await new TestType1[0].WriteCsvAsync(writer);
                var result = stringWriter.ToString();

                // can't assert exact contents because order of properties is undefined (and changes)
                Assert.Contains("Property1", result);
                Assert.Contains("Property2", result);
            }
        }

        [Fact]
        public async void write_csv_async_writes_all_public_properties_by_default()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType1[]
                {
                    new TestType1
                    {
                        Property1 = "1",
                        Property2 = "2",
                        Property3 = "3"
                    },
                    new TestType1
                    {
                        Property1 = "4",
                        Property2 = "5",
                        Property3 = "6"
                    }
                };

                await items.WriteCsvAsync(writer);
                var result = stringWriter.ToString();

                // can't assert exact contents because order of properties is undefined (and changes)
                Assert.Contains("Property1", result);
                Assert.Contains("Property2", result);
                Assert.Contains("1", result);
                Assert.Contains("2", result);
                Assert.Contains("4", result);
                Assert.Contains("5", result);
            }
        }

        [Fact]
        public async void write_csv_async_writes_only_requested_properties_if_specified()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType1[]
                {
                    new TestType1
                    {
                        Property1 = "1",
                        Property2 = "2",
                        Property3 = "3"
                    },
                    new TestType1
                    {
                        Property1 = "4",
                        Property2 = "5",
                        Property3 = "6"
                    }
                };

                await items.WriteCsvAsync(writer, true, new string[] { "Property2" });
                Assert.Equal("Property2<EOL>2<EOL>5<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async void write_csv_async_writes_converts_property_values_to_strings()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType2[]
                {
                    new TestType2
                    {
                        Property1 = "1",
                        Property2 = 2d,
                        Property3 = 3,
                        Property4 = 4m
                    },
                    new TestType2
                    {
                        Property1 = "5",
                        Property2 = 6d,
                        Property3 = 7,
                        Property4 = 8m
                    }
                };

                await items.WriteCsvAsync(writer);
                Assert.Equal("Property1,Property2,Property3,Property4<EOL>1,2,3,4<EOL>5,6,7,8<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async void write_csv_async_writes_converts_null_property_values_to_empty_strings()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType2[]
                {
                    new TestType2
                    {
                        Property1 = null,
                        Property2 = 2d,
                        Property3 = 3,
                        Property4 = 4m
                    },
                    new TestType2
                    {
                        Property1 = null,
                        Property2 = 6d,
                        Property3 = 7,
                        Property4 = null
                    }
                };

                await items.WriteCsvAsync(writer);
                Assert.Equal("Property1,Property2,Property3,Property4<EOL>,2,3,4<EOL>,6,7,<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async void write_csv_async_uses_object_to_string_converter_if_specified()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new TestType2[]
                {
                    new TestType2
                    {
                        Property1 = "1",
                        Property2 = 2d,
                        Property3 = 3,
                        Property4 = 4m
                    },
                    new TestType2
                    {
                        Property1 = "5",
                        Property2 = 6d,
                        Property3 = 7,
                        Property4 = 8m
                    }
                };

                await items.WriteCsvAsync(writer, true, new string[] { "Property1", "Property2" }, o => o.ToString() + "_SUFFIX");
                Assert.Equal("Property1,Property2<EOL>1_SUFFIX,2_SUFFIX<EOL>5_SUFFIX,6_SUFFIX<EOL>", stringWriter.ToString());
            }
        }

        [Fact]
        public async void write_csv_async_returns_number_of_items_written()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                writer.WriteRecord("some", "record");

                var items = new TestType1[]
                {
                    new TestType1
                    {
                        Property1 = "1",
                        Property2 = "2",
                        Property3 = "3"
                    },
                    new TestType1
                    {
                        Property1 = "4",
                        Property2 = "5",
                        Property3 = "6"
                    }
                };

                Assert.Equal(2, await items.WriteCsvAsync(writer));
                Assert.Equal(2, await items.WriteCsvAsync(writer, false));
            }
        }

        [Fact]
        public void write_csv_async_non_reflection_throws_if_object_to_record_converter_is_null()
        {
            using (var writer = new CsvWriter(new StringWriter()))
            {
                var header = new string[] { "The Year", "Even Year?" };
                Assert.Throws<ArgumentNullException>(new DateTime[0].WriteCsvAsync(writer, header, null));
            }
        }

        [Fact]
        public async void write_csv_async_non_reflection_overload_allows_arbitrary_conversion_of_objects_to_csv()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                writer.NewLine = "<EOL>";

                var items = new DateTime[]
                {
                    new DateTime(2004, 12, 31),
                    new DateTime(1978, 12, 4),
                    new DateTime(1979, 10, 26)
                };

                var header = new string[] { "The Year", "Even Year?" };
                await items.WriteCsvAsync(writer, header, dt => new string[] { dt.Year.ToString(CultureInfo.InvariantCulture), (dt.Year % 2 == 0).ToString(CultureInfo.InvariantCulture) });
                Assert.Equal("The Year,Even Year?<EOL>2004,True<EOL>1978,True<EOL>1979,False<EOL>", stringWriter.ToString());
            }
        }

        #region Supporting Types

        public sealed class TestType1
        {
            public string Property1
            {
                get;
                set;
            }

            public string Property2
            {
                get;
                set;
            }

            internal string Property3
            {
                get;
                set;
            }

            private string Property4
            {
                get;
                set;
            }
        }

        public sealed class TestType2
        {
            public string Property1
            {
                get;
                set;
            }

            public double Property2
            {
                get;
                set;
            }

            public int Property3
            {
                get;
                set;
            }

            public decimal? Property4
            {
                get;
                set;
            }
        }

        #endregion
    }
}
