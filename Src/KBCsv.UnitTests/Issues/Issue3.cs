namespace KBCsv.UnitTests.Issues
{
    using System.Threading.Tasks;
    using Xunit;

    public sealed class Issue3
    {
        [Fact]
        public async Task issue3_repro_async()
        {
            var csv = @"Col1,Col2,Col3
val1,val2,val3
val1,val2,";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                await reader.ReadHeaderRecordAsync();
                var buffer = new DataRecord[100];
                var read = await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length);

                Assert.Equal(2, read);
            }
        }

        [Fact]
        public void issue3_repro()
        {
            var csv = @"Col1,Col2,Col3
val1,val2,val3
val1,val2,";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.ReadHeaderRecord();
                var buffer = new DataRecord[100];
                var read = reader.ReadDataRecords(buffer, 0, buffer.Length);

                Assert.Equal(2, read);
            }
        }
    }
}