using System.Linq;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest.Issues
{
    public sealed class Issue12271
    {
        [Fact]
        public void Issue12271_Repro()
        {
            var csv = "first," + new string(' ', CsvParser.BufferSize) + "second";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                while (reader.HasMoreRecords)
                {
                    var record = reader.ReadDataRecord();

                    // fails if there is a space at the rear of the buffer when it needs to be refilled prior to completing the current value
                    Assert.False(record.Values.Any(x => x.StartsWith(" ")), "Record number " + reader.RecordNumber + " has a field that starts with a space. Record: " + record);
                }
            }
        }
    }
}
