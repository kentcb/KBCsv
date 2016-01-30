# Quick-start Guide

## Choosing and Installing a Package

**KBCsv** provides several packages. This section helps you decide which you need.

### KBCsv

This is a PCL containing the core **KBCsv** types for reading and writing CSV. Other packages depend on it, so you will always need it. 

Install with:

```
Install-Package KBCsv
```

### KBCsv.Extensions

This is a PCL containing extension methods for working with CSV. Example features include copying the contents of a source `CsvReader` to a destination `CsvWriter`, and writing the items from an enumerable to a `CsvWriter`.

Install with:

```
Install-Package KBCsv.Extensions
```

### KBCsv.Extensions.Data

This is a .NET 4.5 library that contains extension methods for working with the `System.Data.DataSet` and `System.Data.DataTable` types. For example, you can populate a `DataSet` or `DataTable` with data from a `CsvReader`, and you can write all data from a `DataSet` or `DataTable` to a `CsvWriter`.

Install with:

```
Install-Package KBCsv.Extensions.Data
```

## Examples

For the following examples, assume the file *PlanetaryData.csv* contains data in the following format:

```
Name,RelativeMeanDistanceFromSun,RelativePeriodOfOrbit,MeanOrbitalVelocity,OrbitalEccentricity,InclinationToEcliptic,EquatorialRadius,PolarRadius,RelativeMass,MeanDensity,BodyRotationPeriod,Tilt,ObservedSatelliteCount,Nickname
Mercury,0.3871,0.24,47.89,0.206,7,2439,same,0.06,5.43,1408,2,0,The Swift Planet
Venus,0.7233,0.62,35.04,0.007,3.4,6052,same,0.82,5.25,5832,177.3,0,"The Morning Star, The Evening Star"
```

### Reading CSV from a String

```C#
var csv = @"Kent,36
Belinda,37
Tempany,11
Xak,2";

using (var reader = CsvReader.FromCsvString(csv))
{
    while (reader.HasMoreRecords)
    {
        var dataRecord = reader.ReadDataRecord();
        Console.WriteLine("{0} is {1} years old.", dataRecord[0], dataRecord[1]);
    }
}
```

### Customizing Value Separators and Delimiters

```C#
CsvReader reader = ...;
reader.ValueSeparator = '\t';	// this will be used between each value
reader.ValueDelimiter = '\'';	// this will be used to wrap values that require it (because they contain the separator or a linefeed character)

CsvWriter writer = ...;
writer.ValueSeparator = '\t';
writer.ValueDelimiter = '\'';
```

### Reading CSV from a File

#### Asynchronous

```C#
using (var textReader = new StreamReader("PlanetaryData.csv"))
using (var reader = new CsvReader(textReader, true))
{
    await reader.ReadHeaderRecordAsync();

    var buffer = new DataRecord[128];

    while (reader.HasMoreRecords)
    {
        var read = await reader.ReadDataRecordsAsync(buffer, 0, buffer.Length);

        for (var i = 0; i < read; ++i)
        {
            Console.WriteLine("{0} is nicknamed {1}.", buffer[i]["Name"], buffer[i]["Nickname"]);
        }
    }
}
```

Notice the use of a buffer in this example. Note that there is a `ReadDataRecordAsync` method to asynchronously read a *single* record. It uses an internal buffer to avoid excessive I/O, but you can attain greater performance by filling a buffer using `ReadDataRecordsAsync`. That's because you mitigate the inherent cost of asynchrony by performing work in larger chunks.  

#### Synchronous

```C#
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

### Writing CSV to a File Synchronously

#### Asynchronous

```C#
using (var streamWriter = new StreamWriter("Output.csv"))
using (var writer = new CsvWriter(streamWriter))
{
    writer.ForceDelimit = true;

    await writer.WriteRecordAsync("Name", "Age");
    await writer.WriteRecordAsync("Kent", "33");
    await writer.WriteRecordAsync("Belinda", "34");
    await writer.WriteRecordAsync("Tempany", "8");

    Console.WriteLine("{0} records written", writer.RecordNumber);
}
```

Note that this example uses multiple calls to `WriteRecordAsync`. You can also use `WriteRecordsAsync` to write a batch of records in one asynchronous operation.

#### Synchronous

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

### Copy a CsvReader to a CsvWriter

#### Asynchronous

```C#
using (var stringWriter = new StringWriter())
{
    using (var streamReader = new StreamReader("PlanetaryData.csv"))
    using (var reader = new CsvReader(streamReader))
    using (var writer = new CsvWriter(stringWriter))
    {
        writer.ValueSeparator = '\t';
        writer.ValueDelimiter = '\'';

        await reader.CopyToAsync(writer);
    }

    Console.WriteLine(stringWriter);
}
```

#### Synchronous

```C#
using (var stringWriter = new StringWriter())
{
    using (var streamReader = new StreamReader("PlanetaryData.csv"))
    using (var reader = new CsvReader(streamReader))
    using (var writer = new CsvWriter(stringWriter))
    {
        writer.ValueSeparator = '\t';
        writer.ValueDelimiter = '\'';

        reader.CopyTo(writer);
    }

    Console.WriteLine(stringWriter);
}
```

**NOTE**: This functionality is provided by the `KBCsv.Extensions` package.

### Fill a DataTable from a CSV File

#### Asynchronous

```C#
var table = new DataTable();

using (var streamReader = new StreamReader("PlanetaryData.csv"))
using (var reader = new CsvReader(streamReader))
{
    await reader.ReadHeaderRecordAsync();
    await table.FillAsync(reader);
}

Console.WriteLine("Table contains {0} rows.", table.Rows.Count);
```

#### Synchronous

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

**NOTE**: This functionality is provided by the `KBCsv.Extensions.Data` package