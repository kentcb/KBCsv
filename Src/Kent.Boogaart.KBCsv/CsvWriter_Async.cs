#if ASYNC

namespace Kent.Boogaart.KBCsv
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public partial class CsvWriter
    {
        /// <ignore/>
        public async Task WriteRecordAsync(RecordBase record)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            record.AssertNotNull("record");
            this.WriteRecordToBuffer(record);
            await this.FlushBufferToTextWriterAsync();
        }

        /// <ignore/>
        public async Task WriteRecordAsync(params string[] values)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            values.AssertNotNull("values");
            this.WriteRecordToBuffer(values);
            await this.FlushBufferToTextWriterAsync();
        }

        /// <ignore/>
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

        /// <ignore/>
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