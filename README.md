# KBCsv

## What?

*KBCsv* is an efficient, easy to use .NET parsing and writing library for the [CSV](http://en.wikipedia.org/wiki/Comma-separated_values) (comma-separated values) format.

## Why?

CSV is a common data format that developers need to work with, and .NET does not include intrinsic support for it. Implementing an efficient, standards-compliant CSV parser is not a trivial task, so using KBCsv avoids the need for developers to do so.

## Where?

The easiest way to get *KBCsv* is to install via NuGet:

```
Install-Package Kent.Boogaart.KBCsv
```

Or, if you want the extensions:

```
Install-Package Kent.Boogaart.KBCsv.Extensions
```

Data-specific extensions are available as a separate package for .NET 4.5 (the other packages above are portable):

```
Install-Package Kent.Boogaart.KBCsv.Extensions.Data
```

## How?

```C#
using (var reader = new CsvReader("data.csv"))
{
    reader.ReadHeaderRecord();

    while (reader.HasMoreRecords)
    {
        var record = reader.ReadDataRecord();
        var name = record["Name"];
        var age = record["Age"];
    }
}
```

Please see [the documentation](Doc/overview.md) for more details.

## Who?

*KBCsv* is currently developed solely by [Kent Boogaart](http://kent-boogaart.com/).

## Primary Features

* Very easy to use
* Very efficient
* Separate extension libraries to provide additional (but optional) features such as working with `System.Data` types
* Portable Class Library that targets .NET 4.5, Windows 8, and Windows Phone 8
* Full `async` support
* Includes extensive documentation and examples in both C# and VB.NET
* Conforms to available standards as found [here](http://www.ietf.org/rfc/rfc4180.txt) and [here](http://www.creativyst.com/Doc/Articles/CSV/CSV01.htm)
* Highly customizable, such as specifying non-standard value separators and delimiters
* Very high test coverage