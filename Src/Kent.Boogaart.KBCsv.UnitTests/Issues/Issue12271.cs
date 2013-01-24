namespace Kent.Boogaart.KBCsv.UnitTests.Issues
{
    using System.Linq;
    using Kent.Boogaart.KBCsv.Internal;
    using Xunit;

    public sealed class Issue12271
    {
        [Fact]
        public void issue12271_repro()
        {
            var csv = "first," + new string(' ', CsvParser.BufferSize) + "second";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                while (reader.HasMoreRecords)
                {
                    var record = reader.ReadDataRecord();

                    // fails if there is a space at the rear of the buffer when it needs to be refilled prior to completing the current value
                    Assert.False(record.Any(x => x.StartsWith(" ")), "Record number " + reader.RecordNumber + " has a field that starts with a space. Record: " + record);
                }
            }
        }
    }
}
