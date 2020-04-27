namespace KBCsv.UnitTests.Issues
{
    using System.IO;
    using System.Text;
    using Xunit;

    public sealed class Issue18
    {
        [Fact]
        public void issue18_repro()
        {
            var memoryStream = new MemoryStream();

            using (var csvWriter = new CsvWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
            {
                csvWriter.WriteRecord("foo", "bar");
            }

            Assert.NotEmpty(memoryStream.ToArray());
        }
    }
}
