# Quick-start Guide

## Obtaining KBCsv via NuGet

1. Open the Visual Studio Library Package Manager or the Package Manager Console
2. Add package *Kent.Boogaart.KBCsv* (or *Kent.Boogaart.KBCsv.Extensions*/*Kent.Boogaart.KBCsv.Extensions.Data* if you want extensions in addition to KBCsv)

## Reading CSV from a File Synchronously

```XML
using (var streamReader = new StreamReader("PlanetaryData.csv"))
using (var reader = new CsvReader(streamReader))
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
```

**NOTE**: KBCsv also supports reading and writing data asynchronously. Please see the API documentation for more information.

## Writing CSV to a File Synchronously

```C#
using (var streamWriter = new StreamWriter("Output.csv"))
using (var writer = new CsvWriter(streamWriter))
{
    writer.ForceDelimit = true;

    writer.WriteRecord("Name", "Age");
    writer.WriteRecord("Kent", "33");
    writer.WriteRecord("Belinda", "34");
    writer.WriteRecord("Tempany", "8");

    Console.WriteLine("{0} records written", writer.RecordNumber);
}
```

**NOTE**: KBCsv also supports reading and writing data asynchronously. Please see the API documentation for more information.

## Fill a DataTable from a CSV File

```C#
var table = new DataTable();

using (var streamReader = new StreamReader("PlanetaryData.csv"))
using (var reader = new CsvReader(streamReader))
{
    reader.ReadHeaderRecord();
    table.Fill(reader);
}

Console.WriteLine("Table contains {0} rows.", table.Rows.Count);
```

**NOTE**: This functionality is provided by KBCsv's data extensions

**NOTE**: KBCsv also supports reading and writing data asynchronously. Please see the API documentation for more information.