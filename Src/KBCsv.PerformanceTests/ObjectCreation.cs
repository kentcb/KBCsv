namespace KBCsv.PerformanceTests
{
    using System.IO;
    using KBCsv.PerformanceTests.Utility;
    using Xunit;

    public sealed class ObjectCreation
    {
        [PerformanceTest]
        public void reader_creation()
        {
            var repeatCount = 100000;
            var stringReader = new StringReader(string.Empty);
            var creationCount = 0L;

            for (var i = 0; i < repeatCount; ++i)
            {
                var reader = new CsvReader(stringReader);

                // ensure optimization doesn't remove the object creation
                creationCount += reader.RecordNumber;

                ++creationCount;
            }

            Assert.Equal(repeatCount, creationCount);
        }

        [PerformanceTest]
        public void writer_creation()
        {
            var repeatCount = 100000;
            var stringWriter = new StringWriter();
            var creationCount = 0L;

            for (var i = 0; i < repeatCount; ++i)
            {
                var writer = new CsvWriter(stringWriter);

                // ensure optimization doesn't remove the object creation
                creationCount += writer.RecordNumber;

                ++creationCount;
            }

            Assert.Equal(repeatCount, creationCount);
        }

        [PerformanceTest]
        public void header_record_creation()
        {
            var repeatCount = 1000000;
            var creationCount = 0L;

            for (var i = 0; i < repeatCount; ++i)
            {
                var headerRecord = new HeaderRecord();

                // ensure optimization doesn't remove the object creation
                creationCount += headerRecord.Count;

                ++creationCount;
            }

            Assert.Equal(repeatCount, creationCount);
        }

        [PerformanceTest]
        public void data_record_creation()
        {
            var repeatCount = 1000000;
            var creationCount = 0L;

            for (var i = 0; i < repeatCount; ++i)
            {
                var dataRecord = new DataRecord();

                // ensure optimization doesn't remove the object creation
                creationCount += dataRecord.Count;

                ++creationCount;
            }

            Assert.Equal(repeatCount, creationCount);
        }
    }
}
