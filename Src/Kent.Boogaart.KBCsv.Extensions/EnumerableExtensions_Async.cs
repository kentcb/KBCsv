namespace Kent.Boogaart.KBCsv.Extensions
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    // async equivalents to enumerable extension methods
    // NOTE: changes should be made to the synchronous variants first, then ported here
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Asynchronously writes the items in <paramref name="this"/> to <paramref name="csvWriter"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All public properties of <typeparamref name="T"/> will be written to <paramref name="csvWriter"/>. A header record will also be written, comprised of the property names.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the items to be written to <paramref name="csvWriter"/>.
        /// </typeparam>
        /// <param name="this">
        /// The items to write.
        /// </param>
        /// <param name="csvWriter">
        /// The <see cref="CsvWriter"/>.
        /// </param>
        /// <returns>
        /// The number of items written.
        /// </returns>
        public async static Task<int> WriteCsvAsync<T>(this IEnumerable<T> @this, CsvWriter csvWriter)
        {
            return await @this.WriteCsvAsync(csvWriter, true).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the items in <paramref name="this"/> to <paramref name="csvWriter"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All public properties of <typeparamref name="T"/> will be written to <paramref name="csvWriter"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the items to be written to <paramref name="csvWriter"/>.
        /// </typeparam>
        /// <param name="this">
        /// The items to write.
        /// </param>
        /// <param name="csvWriter">
        /// The <see cref="CsvWriter"/>.
        /// </param>
        /// <param name="writeHeaderRecord">
        /// If <see langword="true"/>, a header record will first be written to <paramref name="csvWriter"/>, which is comprised of all property names.
        /// </param>
        /// <returns>
        /// The number of items written.
        /// </returns>
        public async static Task<int> WriteCsvAsync<T>(this IEnumerable<T> @this, CsvWriter csvWriter, bool writeHeaderRecord)
        {
            return await @this.WriteCsvAsync(csvWriter, writeHeaderRecord, typeof(T).GetRuntimeProperties().Where(x => x.CanRead && x.GetMethod.IsPublic).Select(x => x.Name).ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the items in <paramref name="this"/> to <paramref name="csvWriter"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Property values are obtained via reflection.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the items to be written to <paramref name="csvWriter"/>.
        /// </typeparam>
        /// <param name="this">
        /// The items to write.
        /// </param>
        /// <param name="csvWriter">
        /// The <see cref="CsvWriter"/>.
        /// </param>
        /// <param name="writeHeaderRecord">
        /// If <see langword="true"/>, a header record will first be written to <paramref name="csvWriter"/>, which is comprised of all specified property names.
        /// </param>
        /// <param name="propertyNames">
        /// The names of public properties in <typeparamref name="T"/> that should be written to <paramref name="csvWriter"/>.
        /// </param>
        /// <returns>
        /// The number of items written.
        /// </returns>
        public async static Task<int> WriteCsvAsync<T>(this IEnumerable<T> @this, CsvWriter csvWriter, bool writeHeaderRecord, string[] propertyNames)
        {
            return await @this.WriteCsvAsync(csvWriter, writeHeaderRecord, propertyNames, o => o == null ? string.Empty : o.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the items in <paramref name="this"/> to <paramref name="csvWriter"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Property values are obtained via reflection.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the items to be written to <paramref name="csvWriter"/>.
        /// </typeparam>
        /// <param name="this">
        /// The items to write.
        /// </param>
        /// <param name="csvWriter">
        /// The <see cref="CsvWriter"/>.
        /// </param>
        /// <param name="writeHeaderRecord">
        /// If <see langword="true"/>, a header record will first be written to <paramref name="csvWriter"/>, which is comprised of all specified property names.
        /// </param>
        /// <param name="propertyNames">
        /// The names of public properties in <typeparamref name="T"/> that should be written to <paramref name="csvWriter"/>.
        /// </param>
        /// <param name="objectToStringConverter">
        /// Provides a means of converting items in <paramref name="this"/> to <see cref="String"/>s.
        /// </param>
        /// <returns>
        /// The number of items written.
        /// </returns>
        public async static Task<int> WriteCsvAsync<T>(this IEnumerable<T> @this, CsvWriter csvWriter, bool writeHeaderRecord, string[] propertyNames, Func<object, string> objectToStringConverter)
        {
            objectToStringConverter.AssertNotNull("objectToStringConverter");
            propertyNames.AssertNotNull("propertyNames");

            var propertyInfos = new PropertyInfo[propertyNames.Length];

            // get and cache all property infos that we'll need below
            for (var i = 0; i < propertyNames.Length; ++i)
            {
                exceptionHelper.ResolveAndThrowIf(propertyNames[i] == null, "nullPropertyName");
                var propertyInfo = typeof(T).GetRuntimeProperty(propertyNames[i]);
                exceptionHelper.ResolveAndThrowIf(propertyInfo == null, "propertyNotFound", propertyNames[i], typeof(T).FullName);
                propertyInfos[i] = propertyInfo;
            }

            // if a header is requested, just return the property names
            var header = writeHeaderRecord ? propertyNames : null;

            // convert each object to a record by obtaining each property value and running it through the objectToStringConverter
            Func<T, IEnumerable<string>> objectToRecordConverter = o =>
            {
                var record = new string[propertyNames.Length];

                for (var i = 0; i < propertyInfos.Length; ++i)
                {
                    record[i] = objectToStringConverter(propertyInfos[i].GetValue(o, null));
                }

                return record;
            };

            return await @this.WriteCsvAsync(csvWriter, header, objectToRecordConverter).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the items in <paramref name="this"/> to <paramref name="csvWriter"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This overload provides maximum flexibility in how items are written CSV.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the items to be written to <paramref name="csvWriter"/>.
        /// </typeparam>
        /// <param name="this">
        /// The items to write.
        /// </param>
        /// <param name="csvWriter">
        /// The <see cref="CsvWriter"/>.
        /// </param>
        /// <param name="header">
        /// If non-<see langword="null"/>, this will be written to <paramref name="csvWriter"/> before any data records are written.
        /// </param>
        /// <param name="objectToRecordConverter">
        /// Converts an item in <paramref name="this"/> to a CSV record.
        /// </param>
        /// <returns>
        /// The number of items written.
        /// </returns>
        public async static Task<int> WriteCsvAsync<T>(this IEnumerable<T> @this, CsvWriter csvWriter, IEnumerable<string> header, Func<T, IEnumerable<string>> objectToRecordConverter)
        {
            @this.AssertNotNull("@this");
            csvWriter.AssertNotNull("csvWriter");
            objectToRecordConverter.AssertNotNull("objectToRecordConverter");

            HeaderRecord headerRecord = null;

            if (header != null)
            {
                headerRecord = new HeaderRecord(header);
                await csvWriter.WriteRecordAsync(headerRecord).ConfigureAwait(false);
            }

            var num = 0;
            var buffer = new DataRecord[16];
            var bufferOffset = 0;

            foreach (var item in @this)
            {
                var record = new DataRecord(headerRecord, objectToRecordConverter(item));
                buffer[bufferOffset++] = record;

                if (bufferOffset == buffer.Length)
                {
                    // buffer full
                    await csvWriter.WriteRecordsAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    bufferOffset = 0;
                }

                ++num;
            }

            // write any outstanding data in buffer
            await csvWriter.WriteRecordsAsync(buffer, 0, bufferOffset).ConfigureAwait(false);

            return num;
        }
    }
}