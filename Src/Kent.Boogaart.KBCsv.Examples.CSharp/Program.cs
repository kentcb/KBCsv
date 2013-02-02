namespace Kent.Boogaart.KBCsv.Examples.CSharp
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Kent.Boogaart.KBCsv.Extensions;

    class Program
    {
        static void Main(string[] args)
        {
            var examples = typeof(Program)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.Name != "Main" && x.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                .Select((m, i) => new { Method = m, Key = (char)('a' + i) })
                .ToDictionary(e => e.Key, e => e.Method);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choose an example to run:");

                foreach (var example in examples)
                {
                    Console.WriteLine("  {0}. {1}", example.Key, example.Value.Name);
                }

                var choice = Console.ReadKey();

                if (examples.ContainsKey(choice.KeyChar))
                {
                    Console.CursorLeft = 0;
                    Console.WriteLine(" ");

                    var result = examples[choice.KeyChar].Invoke(null, null) as Task;

                    while (result != null)
                    {
                        result.Wait();
                    }

                    Console.WriteLine();
                    Console.WriteLine("Done - press a key to choose another example");
                    Console.ReadKey();
                }
            }
        }

        private static void ReadCSVFromString()
        {
            #region ReadCSVFromString

            var csv = @"Kent,33
Belinda,34
Tempany,8";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                while (reader.HasMoreRecords)
                {
                    var dataRecord = reader.ReadDataRecord();
                    Console.WriteLine("{0} is {1} years old.", dataRecord[0], dataRecord[1]);
                }
            }

            #endregion
        }

        private static void ReadCSVFromStringPreservingWhiteSpace()
        {
            #region ReadCSVFromStringPreservingWhiteSpace

            var csv = @"Kent   ,33
Belinda,34
Tempany, 8";

            using (var reader = CsvReader.FromCsvString(csv))
            {
                reader.PreserveLeadingWhiteSpace = true;
                reader.PreserveTrailingWhiteSpace = true;

                while (reader.HasMoreRecords)
                {
                    var dataRecord = reader.ReadDataRecord();
                    Console.WriteLine("{0} is {1} years old.", dataRecord[0], dataRecord[1]);
                }
            }

            #endregion
        }

        private static void ReadTabDelimitedDataFromFile()
        {
            #region ReadTabDelimitedDataFromFile

            using (var reader = new CsvReader("PlanetaryData.tdv"))
            {
                reader.ValueSeparator = '\t';
                reader.ValueDelimiter = '\'';

                while (reader.HasMoreRecords)
                {
                    var dataRecord = reader.ReadDataRecord();
                    Console.WriteLine("{0} is nicknamed {1}.", dataRecord[0], dataRecord[dataRecord.Count - 1]);
                }
            }

            #endregion
        }

        private static void ReadCSVFromFile()
        {
            #region ReadCSVFromFile

            using (var reader = new CsvReader("PlanetaryData.csv"))
            {
                // the CSV file has a header record, so we read that first
                reader.ReadHeaderRecord();

                while (reader.HasMoreRecords)
                {
                    var dataRecord = reader.ReadDataRecord();

                    // since the reader has a header record, we can access data by column names as well as by index
                    Console.WriteLine("{0} is nicknamed {1}.", dataRecord[0], dataRecord["Nickname"]);
                }
            }

            #endregion
        }

        private static void ReadCSVFromStream()
        {
            #region ReadCSVFromStream

            using (var stream = new FileStream("PlanetaryData.csv", FileMode.Open))
            using (var reader = new CsvReader(stream, Encoding.UTF8))
            {
                reader.ReadHeaderRecord();

                while (reader.HasMoreRecords)
                {
                    var dataRecord = reader.ReadDataRecord();
                    Console.WriteLine("{0} is nicknamed {1}.", dataRecord["Name"], dataRecord["Nickname"]);
                }
            }

            #endregion
        }

        private static void ReadCSVFromFileWithExplicitHeader()
        {
            #region ReadCSVFromFileWithExplicitHeader

            using (var reader = new CsvReader("PlanetaryData_NoHeader.csv"))
            {
                reader.HeaderRecord = new HeaderRecord("OfficialName", "NickName");

                while (reader.HasMoreRecords)
                {
                    var dataRecord = reader.ReadDataRecord();
                    Console.WriteLine("{0} is nicknamed {1}.", dataRecord["OfficialName"], dataRecord["NickName"]);
                    reader.SkipRecord();
                }
            }

            #endregion
        }

        private async static Task ReadCSVFromFileAsynchronously()
        {
            #region ReadCSVFromFileAsynchronously

            using (var textReader = new StreamReader("PlanetaryData.csv"))
            using (var reader = new CsvReader(textReader, true))
            {
                await reader.ReadHeaderRecordAsync();

                // realistically, you'll probably want a larger buffer, but this suffices for demonstration purposes
                var buffer = new DataRecord[4];

                while (reader.HasMoreRecords)
                {
                    var read = await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length);

                    for (var i = 0; i < read; ++i)
                    {
                        Console.WriteLine("{0} is nicknamed {1}.", buffer[i]["Name"], buffer[i]["Nickname"]);
                    }
                }
            }

            #endregion
        }

        private static void WriteCSVToString()
        {
            #region WriteCSVToString

            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.WriteRecord("Name", "Age");
                    writer.WriteRecord("Kent", "33");
                    writer.WriteRecord("Belinda", "34");
                    writer.WriteRecord("Tempany", "8");
                }

                Console.WriteLine(stringWriter);
            }

            #endregion
        }

        private static void WriteCSVToFile()
        {
            #region WriteCSVToFile

            using (var writer = new CsvWriter("Output.csv"))
            {
                writer.ForceDelimit = true;

                writer.WriteRecord("Name", "Age");
                writer.WriteRecord("Kent", "33");
                writer.WriteRecord("Belinda", "34");
                writer.WriteRecord("Tempany", "8");

                Console.WriteLine("{0} records written", writer.RecordNumber);
            }

            #endregion
        }

        private static void WriteCSVToStreamWithForcedDelimiting()
        {
            #region WriteCSVToStreamWithForcedDelimiting

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new CsvWriter(memoryStream, Encoding.ASCII, true))
                {
                    writer.ForceDelimit = true;

                    writer.WriteRecord("Name", "Age");
                    writer.WriteRecord("Kent", "33");
                    writer.WriteRecord("Belinda", "34");
                    writer.WriteRecord("Tempany", "8");
                }

                Console.WriteLine(Encoding.ASCII.GetString(memoryStream.ToArray()));
            }

            #endregion
        }

        private async static Task ReadCSVFromFileAndWriteToTabDelimitedFile()
        {
            #region ReadCSVFromFileAndWriteToTabDelimitedFile

            using (var reader = new CsvReader("PlanetaryData.csv"))
            using (var writer = new CsvWriter("PlanetaryData_Modified.csv"))
            {
                writer.ValueSeparator = '\t';
                writer.ValueDelimiter = '\'';

                // realistically, you'll probably want a larger buffer, but this suffices for demonstration purposes
                var buffer = new DataRecord[4];

                while (reader.HasMoreRecords)
                {
                    var read = await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length);
                    await writer.WriteRecordsAsync(buffer, 0, read);
                }
            }

            #endregion
        }

        private static void FillDataTableFromCSVFile()
        {
            #region FillDataTableFromCSVFile

            var table = new DataTable();

            using (var reader = new CsvReader("PlanetaryData.csv"))
            {
                reader.ReadHeaderRecord();
                table.Fill(reader);
            }

            Console.WriteLine("Table contains {0} rows.", table.Rows.Count);

            #endregion
        }

        private static void WriteDataTableToCSV()
        {
            #region WriteDataTableToCSV

            var table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Age");
            table.Rows.Add("Kent", 33);
            table.Rows.Add("Belinda", 34);
            table.Rows.Add("Tempany", 8);
            table.Rows.Add("Xak", 0);

            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    table.WriteCsv(writer);
                }

                Console.WriteLine("CSV: {0}", stringWriter);
            }

            #endregion
        }

        private async static Task FillDataTableFromCSVFileThenWriteSomeToStringAsynchronously()
        {
            #region FillDataTableFromCSVFileThenWriteSomeToStringAsynchronously

            var table = new DataTable();

            using (var reader = new CsvReader("PlanetaryData.csv"))
            {
                await reader.ReadHeaderRecordAsync();
                await table.FillAsync(reader);
            }

            using (var stringWriter = new StringWriter())
            {
                using (var writer = new CsvWriter(stringWriter))
                {
                    await table.WriteCsvAsync(writer, false, 5);
                }

                Console.WriteLine("CSV: {0}", stringWriter);
            }

            #endregion
        }

        private static void WriteScreenInformationToCSV()
        {
            #region WriteScreenInformationToCSV

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                Screen.AllScreens.WriteCsv(writer);
                writer.Flush();

                Console.WriteLine(stringWriter);
            }

            #endregion
        }

        private async static void WriteSelectedProcessInformationCSVAsynchronously()
        {
            #region WriteSelectedProcessInformationCSVAsynchronously

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                await Process.GetProcesses().WriteCsvAsync(writer, true, new string[] { "Id", "ProcessName", "WorkingSet64" });
                await writer.FlushAsync();

                Console.WriteLine(stringWriter);
            }

            #endregion
        }

        private static void CopyCSVFileToStringWriter()
        {
            #region CopyCSVFileToStringWriter

            using (var stringWriter = new StringWriter())
            {
                using (var reader = new CsvReader("PlanetaryData.csv"))
                using (var writer = new CsvWriter(stringWriter))
                {
                    writer.ValueSeparator = '\t';
                    writer.ValueDelimiter = '\'';

                    reader.CopyTo(writer);
                }

                Console.WriteLine(stringWriter);
            }

            #endregion
        }
    }
}