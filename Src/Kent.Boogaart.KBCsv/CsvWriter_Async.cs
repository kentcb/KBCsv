#if ASYNC

namespace Kent.Boogaart.KBCsv
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public partial class CsvWriter
    {
        /// <summary>
        /// Asynchronously writes a record to this <c>CsvWriter</c>.
        /// </summary>
        /// <remarks>
        /// All values within <paramref name="record"/> are written in the order they appear.
        /// </remarks>
        /// <param name="record">
        /// The record to write.
        /// </param>
        public async Task WriteRecordAsync(RecordBase record)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            record.AssertNotNull("record");
            this.WriteRecordToBuffer(record);
            await this.FlushBufferToTextWriterAsync();
        }

        /// <summary>
        /// Asynchronously writes a record to this <c>CsvWriter</c>.
        /// </summary>
        /// <remarks>
        /// None of the <see cref="System.String"/>s within <paramref name="values"/> can be <see langword="null"/>. If so, an exception will be thrown.
        /// </remarks>
        /// <param name="values">
        /// The values comprising the record.
        /// </param>
        public async Task WriteRecordAsync(params string[] values)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            values.AssertNotNull("values");
            this.WriteRecordToBuffer(values);
            await this.FlushBufferToTextWriterAsync();
        }

        /// <summary>
        /// Asynchronously writes a record to this <c>CsvWriter</c>.
        /// </summary>
        /// <remarks>
        /// None of the <see cref="System.String"/>s within <paramref name="values"/> can be <see langword="null"/>. If so, an exception will be thrown.
        /// </remarks>
        /// <param name="values">
        /// The values comprising the record.
        /// </param>
        public async Task WriteRecordAsync(IEnumerable<string> values)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            values.AssertNotNull("values");
            this.WriteRecordToBuffer(values);
            await this.FlushBufferToTextWriterAsync();
        }

        /// <summary>
        /// Asynchronously writes <paramref name="length"/> records to this <c>CsvWriter</c>.
        /// </summary>
        /// <remarks>
        /// When writing a lot of data, it is possible that better performance can be achieved by using this method.
        /// </remarks>
        /// <param name="buffer">
        /// The buffer containing the records to be written.
        /// </param>
        /// <param name="offset">
        /// The offset into <paramref name="buffer"/> from which the first record will be obtained.
        /// </param>
        /// <param name="length">
        /// The number of records to write.
        /// </param>
        public async Task WriteRecordsAsync(RecordBase[] buffer, int offset, int length)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            buffer.AssertNotNull("buffer");
            exceptionHelper.ResolveAndThrowIf(offset < 0 || offset >= buffer.Length, "invalidOffset");
            exceptionHelper.ResolveAndThrowIf(offset + length > buffer.Length, "invalidLength");

            for (var i = offset; i < offset + length; ++i)
            {
                var record = buffer[i];
                exceptionHelper.ResolveAndThrowIf(record == null, "recordNull");
                this.WriteRecordToBuffer(record);
            }

            // we only flush once, when all records have been written
            await this.FlushBufferToTextWriterAsync();
        }

        /// <summary>
        /// Asynchronously flushes this <c>CsvWriter</c>.
        /// </summary>
        /// <remarks>
        /// This method can be used to flush the underlying <see cref="System.IO.TextWriter"/> to which this <c>CsvWriter</c> is writing data.
        /// </remarks>
        public async Task FlushAsync()
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            await this.textWriter.FlushAsync();
        }

        // asynchronously push whatever's in the buffer to the text writer and reset the buffer
        private async Task FlushBufferToTextWriterAsync()
        {
            await this.textWriter.WriteAsync(this.bufferBuilder.ToString());
            this.bufferBuilder.Length = 0;
        }
    }

}

#endif