namespace Kent.Boogaart.KBCsv.Extensions
{
    using Kent.Boogaart.HelperTrinity.Extensions;

    /// <summary>
    /// Provides extensions against <see cref="CsvReader"/>.
    /// </summary>
    /// <example>
    /// <para>
    /// The following example uses <see cref="CopyTo"/> to translate CSV-formatted data into tab-delimited data:
    /// </para>
    /// <code source="..\Src\Kent.Boogaart.KBCsv.Examples.CSharp\Program.cs" region="Example 14" lang="cs"/>
    /// <code source="..\Src\Kent.Boogaart.KBCsv.Examples.VB\Program.vb" region="Example 14" lang="vb"/>
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
    }
}
