namespace Kent.Boogaart.KBCsv.Examples.CSharp
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Kent.Boogaart.KBCsv.Extensions;

    class Program
    {
        static void Main(string[] args)
        {
            // uncomment the example you would like to run
            //Example1();
            //Example2();
            //Example3();
            //Example4();
            //Example5().Wait();
            //Example6();
            //Example7();
            //Example8().Wait();
            //Example9();
            //Example10();
            //Example11().Wait();
            //Example12();
            //Example13();
            //Example14();

            Console.WriteLine();
            Console.WriteLine("DONE - any key to exit");
            Console.ReadKey();
        }

        private static void Example1()
        {
            #region Example 1

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

        private static void Example2()
        {
            #region Example 2

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

        private static void Example3()
        {
            #region Example 3

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

        private static void Example4()
        {
            #region Example 4

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

        private async static Task Example5()
        {
            #region Example 5

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

        private static void Example6()
        {
            #region Example 6

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

        private static void Example7()
        {
            #region Example 7

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

        private async static Task Example8()
        {
            #region Example 8

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

        private static void Example9()
        {
            #region Example 9

            var table = new DataTable();

            using (var reader = new CsvReader("PlanetaryData.csv"))
            {
                reader.ReadHeaderRecord();
                table.Fill(reader);
            }

            Console.WriteLine("Table contains {0} rows.", table.Rows.Count);

            #endregion
        }

        private static void Example10()
        {
            #region Example 10

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

        private async static Task Example11()
        {
            #region Example 11

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
                    table.WriteCsv(writer, false, 5);
                }

                Console.WriteLine("CSV: {0}", stringWriter);
            }

            #endregion
        }

        private static void Example12()
        {
            #region Example 12

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                Screen.AllScreens.WriteCsv(writer);
                writer.Flush();

                Console.WriteLine(stringWriter);
            }

            #endregion
        }

        private async static void Example13()
        {
            #region Example 13

            using (var stringWriter = new StringWriter())
            using (var writer = new CsvWriter(stringWriter))
            {
                await Process.GetProcesses().WriteCsvAsync(writer, true, new string[] { "Id", "ProcessName", "WorkingSet64" });
                await writer.FlushAsync();

                Console.WriteLine(stringWriter);
            }

            #endregion
        }

        private static void Example14()
        {
            #region Example 14

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