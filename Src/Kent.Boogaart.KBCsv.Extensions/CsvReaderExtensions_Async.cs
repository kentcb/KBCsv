namespace Kent.Boogaart.KBCsv.Extensions
{
    using System.Threading.Tasks;
    using Kent.Boogaart.KBCsv.Internal;

    // async equivalents to csv reader extension methods
    // NOTE: changes should be made to the synchronous variants first, then ported here
    public static partial class CsvReaderExtensions
    {
        /// <summary>
        /// Asynchronously copies all remaining records in <paramref name="this"/> to <paramref name="destination"/>.
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
        public async static Task<int> CopyToAsync(this CsvReader @this, CsvWriter destination)
        {
            @this.AssertNotNull("@this");
            destination.AssertNotNull("destination");

            var num = 0;
            var buffer = new DataRecord[16];
            var read = 0;

            while ((read = await @this.ReadDataRecordsAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
            {
                await destination.WriteRecordsAsync(buffer, 0, read).ConfigureAwait(false);
                num += read;
            }

            return num;
        }
    }
}