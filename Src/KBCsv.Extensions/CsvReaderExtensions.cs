namespace KBCsv
{
    using System.Collections.Generic;
    using KBCsv.Internal;

    /// <summary>
    /// Provides extensions against <see cref="CsvReader"/>.
    /// </summary>
    /// <example>
    /// <para>
    /// The following example uses <see cref="CopyTo"/> to translate CSV-formatted data into tab-delimited data:
    /// </para>
    /// <code source="..\Src\KBCsv.Examples.CSharp\Program.cs" region="CopyCSVFileToStringWriter" lang="cs"/>
    /// <code source="..\Src\KBCsv.Examples.VB\Program.vb" region="CopyCSVFileToStringWriter" lang="vb"/>
    /// </example>
    public static partial class CsvReaderExtensions
    {
        /// <summary>
        /// Copies all remaining records in <paramref name="this"/> to <paramref name="destination"/>.
        /// </summary>
        /// <param name="this">
        /// The data source.
        /// </param>
        /// <param name="destination">
        /// The data destination.
        /// </param>
        /// <returns>
        /// The number of records written to <paramref name="destination"/>.
        /// </returns>
        public static int CopyTo(this CsvReader @this, CsvWriter destination)
        {
            @this.AssertNotNull("@this");
            destination.AssertNotNull("destination");

            var num = 0;
            var buffer = new DataRecord[16];
            var read = 0;

            while ((read = @this.ReadDataRecords(buffer, 0, buffer.Length)) != 0)
            {
                destination.WriteRecords(buffer, 0, read);
                num += read;
            }

            return num;
        }

        /// <summary>
        /// Exposes the records in a <see cref="CsvReader"/> as an enumeration of <see cref="DataRecord"/>.
        /// </summary>
        /// <param name="this">
        /// The data source.
        /// </param>
        /// <param name="readHeader">
        /// If <see langword="true"/>, the first record in <paramref name="this"/> will be read in as the header record.
        /// </param>
        /// <returns>
        /// An enumerable of all records in <paramref name="this"/>.
        /// </returns>
        public static IEnumerable<DataRecord> ToEnumerable(this CsvReader @this, bool readHeader = false)
        {
            @this.AssertNotNull(nameof(@this));

            if (readHeader && @this.HasMoreRecords)
            {
                @this.ReadHeaderRecord();
            }

            while (@this.HasMoreRecords)
            {
                yield return @this.ReadDataRecord();
            }
        }
    }
}
