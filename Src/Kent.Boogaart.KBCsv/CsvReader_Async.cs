#if ASYNC

namespace Kent.Boogaart.KBCsv
{
    using System.Threading.Tasks;

    // async equivalents to relevant methods in the reader
    // NOTE: changes should be made to the synchronous variants first, then ported here
    public partial class CsvReader
    {
        /// <ignore/>
        public async Task<bool> SkipRecordAsync()
        {
            return await this.SkipRecordsAsync(1, true) == 1;
        }

        /// <ignore/>
        public async Task<bool> SkipRecordAsync(bool incrementRecordNumber)
        {
            return await this.SkipRecordsAsync(1, incrementRecordNumber) == 1;
        }

        /// <ignore/>
        public async Task<int> SkipRecordsAsync(int count)
        {
            return await this.SkipRecordsAsync(count, true);
        }

        /// <ignore/>
        public async Task<int> SkipRecordsAsync(int count, bool incrementRecordNumber)
        {
            this.EnsureNotDisposed();
            var skipped = await this.parser.SkipRecordsAsync(count);

            if (incrementRecordNumber)
            {
                this.recordNumber += skipped;
            }

            return skipped;
        }

        /// <ignore/>
        public async Task<HeaderRecord> ReadHeaderRecordAsync()
        {
            this.EnsureNotDisposed();
            this.EnsureNotPassedFirstRecord();

            if (await this.parser.ParseRecordsAsync(null, this.buffer, 0, 1) == 1)
            {
                ++this.recordNumber;
                this.headerRecord = new HeaderRecord(this.buffer[0]);
                return this.headerRecord;
            }

            throw exceptionHelper.Resolve("noRecords");
        }

        /// <ignore/>
        public async Task<DataRecord> ReadDataRecordAsync()
        {
            this.EnsureNotDisposed();

            if (await this.parser.ParseRecordsAsync(null, this.buffer, 0, 1) == 1)
            {
                ++this.recordNumber;
                return this.buffer[0];
            }

            throw exceptionHelper.Resolve("noRecords");
        }

        /// <ignore/>
        public async Task<int> ReadDataRecordsAsync(DataRecord[] buffer, int offset, int count)
        {
            this.EnsureNotDisposed();

            var read = await this.parser.ParseRecordsAsync(this.headerRecord, buffer, offset, count);
            this.recordNumber += read;
            return read;
        }
    }
}

#endif