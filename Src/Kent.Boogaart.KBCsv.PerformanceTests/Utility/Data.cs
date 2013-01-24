namespace Kent.Boogaart.KBCsv.PerformanceTests.Utility
{
    using System.Collections.Generic;
    using System.IO;

    internal static class Data
    {
        public static readonly IEnumerable<string> PlainDataLines = GetLinesFromCsv(DataResources.PlainData);
        public static readonly IEnumerable<IEnumerable<string>> PlainDataValues = GetValuesFromCsv(DataResources.PlainData);

        public static readonly IEnumerable<string> CopiousWhiteSpaceLines = GetLinesFromCsv(DataResources.CopiousWhiteSpace);
        public static readonly IEnumerable<IEnumerable<string>> CopiousWhiteSpaceValues = GetValuesFromCsv(DataResources.CopiousWhiteSpace);

        public static readonly IEnumerable<string> CopiousEscapedDelimitersLines = GetLinesFromCsv(DataResources.CopiousEscapedDelimiters);
        public static readonly IEnumerable<IEnumerable<string>> CopiousEscapedDelimitersValues = GetValuesFromCsv(DataResources.CopiousEscapedDelimiters);

        public static readonly IEnumerable<string> DelimitedLines = GetLinesFromCsv(DataResources.Delimited);
        public static readonly IEnumerable<IEnumerable<string>> DelimitedValues = GetValuesFromCsv(DataResources.Delimited);

        public static readonly IEnumerable<string> UnnecessarilyDelimitedLines = GetLinesFromCsv(DataResources.UnnecessarilyDelimited);
        public static readonly IEnumerable<IEnumerable<string>> UnnecessarilyDelimitedValues = GetValuesFromCsv(DataResources.UnnecessarilyDelimited);

        public static readonly IEnumerable<string> StackoverflowLines = GetLinesFromCsv(DataResources.Stackoverflow);
        public static readonly IEnumerable<IEnumerable<string>> StackoverflowValues = GetValuesFromCsv(DataResources.Stackoverflow);

        // extract lines from CSV
        private static IList<string> GetLinesFromCsv(string csv)
        {
            var lines = new List<string>();

            using (var csvReader = CsvReader.FromCsvString(csv))
            {
                while (csvReader.HasMoreRecords)
                {
                    var record = csvReader.ReadDataRecord();

                    using (var stringWriter = new StringWriter())
                    using (var csvWriter = new CsvWriter(stringWriter))
                    {
                        csvWriter.WriteRecord(record);
                        csvWriter.Flush();

                        lines.Add(stringWriter.ToString());
                    }
                }
            }

            return lines;
        }

        // extract values (by record) from CSV
        private static IList<IList<string>> GetValuesFromCsv(string csv)
        {
            var values = new List<IList<string>>();

            using (var csvReader = CsvReader.FromCsvString(csv))
            {
                while (csvReader.HasMoreRecords)
                {
                    var record = csvReader.ReadDataRecord();
                    var valuesForLine = new List<string>();
                    valuesForLine.AddRange(record);
                    values.Add(valuesForLine);
                }
            }

            return values;
        }
    }
}