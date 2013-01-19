namespace Kent.Boogaart.KBCsv
{
    using System;
    using System.IO;
    using System.Text;
    using Kent.Boogaart.HelperTrinity;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Kent.Boogaart.KBCsv.Internal;

    /// <ignore/>
    public partial class CsvReader : IDisposable
    {
        private static readonly ExceptionHelper exceptionHelper = new ExceptionHelper(typeof(CsvReader));
        private readonly CsvParser parser;
        private readonly bool leaveOpen;
        private readonly DataRecord[] buffer;
        private HeaderRecord headerRecord;
        private long recordNumber;
        private bool disposed;

        /// <ignore/>
        public CsvReader(Stream stream)
            : this(stream, Constants.DefaultEncoding)
        {
        }

        /// <ignore/>
        public CsvReader(Stream stream, Encoding encoding)
            : this(new StreamReader(stream, encoding), false)
        {
        }

        /// <ignore/>
        public CsvReader(Stream stream, Encoding encoding, bool leaveOpen)
            : this(new StreamReader(stream, encoding), leaveOpen)
        {
        }

        /// <ignore/>
        public CsvReader(string path)
            : this(path, Constants.DefaultEncoding)
        {
        }

        /// <ignore/>
        public CsvReader(string path, Encoding encoding)
            : this(new StreamReader(path, encoding), false)
        {
        }

        /// <ignore/>
        public CsvReader(TextReader textReader)
            : this(textReader, false)
        {
            textReader.AssertNotNull("textReader");
        }

        /// <ignore/>
        public CsvReader(TextReader textReader, bool leaveOpen)
        {
            textReader.AssertNotNull("textReader");

            this.parser = new CsvParser(textReader);
            this.leaveOpen = leaveOpen;

            // used to parse singular records
            this.buffer = new DataRecord[1];
        }

        /// <ignore/>
        public bool PreserveLeadingWhiteSpace
        {
            get
            {
                this.EnsureNotDisposed();
                return this.parser.PreserveLeadingWhiteSpace;
            }

            set
            {
                this.EnsureNotDisposed();
                this.parser.PreserveLeadingWhiteSpace = value;
            }
        }

        /// <ignore/>
        public bool PreserveTrailingWhiteSpace
        {
            get
            {
                this.EnsureNotDisposed();
                return this.parser.PreserveTrailingWhiteSpace;
            }

            set
            {
                this.EnsureNotDisposed();
                this.parser.PreserveTrailingWhiteSpace = value;
            }
        }

        /// <ignore/>
        public char ValueSeparator
        {
            get
            {
                this.EnsureNotDisposed();
                return this.parser.ValueSeparator;
            }

            set
            {
                this.EnsureNotDisposed();
                this.parser.ValueSeparator = value;
            }
        }

        /// <ignore/>
        public char ValueDelimiter
        {
            get
            {
                this.EnsureNotDisposed();
                return this.parser.ValueDelimiter;
            }

            set
            {
                this.EnsureNotDisposed();
                this.parser.ValueDelimiter = value;
            }
        }

        /// <ignore/>
        public HeaderRecord HeaderRecord
        {
            get
            {
                this.EnsureNotDisposed();
                return this.headerRecord;
            }

            set
            {
                this.EnsureNotDisposed();
                this.EnsureNotPassedFirstRecord();
                value.AssertNotNull("value");
                this.headerRecord = value;
            }
        }

        /// <ignore/>
        public long RecordNumber
        {
            get
            {
                this.EnsureNotDisposed();
                return this.recordNumber;
            }
        }

        /// <ignore/>
        public bool HasMoreRecords
        {
            get
            {
                this.EnsureNotDisposed();
                return this.parser.HasMoreRecords;
            }
        }

        /// <ignore/>
        public static CsvReader FromCsvString(string csv)
        {
            csv.AssertNotNull("csv");
            return new CsvReader(new StringReader(csv), true);
        }

        /// <ignore/>
        public bool SkipRecord()
        {
            return this.SkipRecords(1, true) == 1;
        }

        /// <ignore/>
        public bool SkipRecord(bool incrementRecordNumber)
        {
            return this.SkipRecords(1, incrementRecordNumber) == 1;
        }

        /// <ignore/>
        public int SkipRecords(int count)
        {
            return this.SkipRecords(count, true);
        }

        /// <ignore/>
        public int SkipRecords(int count, bool incrementRecordNumber)
        {
            this.EnsureNotDisposed();
            var skipped = this.parser.SkipRecords(count);

            if (incrementRecordNumber)
            {
                this.recordNumber += skipped;
            }

            return skipped;
        }

        /// <ignore/>
        public HeaderRecord ReadHeaderRecord()
        {
            this.EnsureNotDisposed();
            this.EnsureNotPassedFirstRecord();

            if (this.parser.ParseRecords(null, this.buffer, 0, 1) == 1)
            {
                ++this.recordNumber;
                this.headerRecord = new HeaderRecord(this.buffer[0]);
                return this.headerRecord;
            }

            throw exceptionHelper.Resolve("noRecords");
        }

        /// <ignore/>
        public DataRecord ReadDataRecord()
        {
            this.EnsureNotDisposed();

            if (this.parser.ParseRecords(null, this.buffer, 0, 1) == 1)
            {
                ++this.recordNumber;
                return this.buffer[0];
            }

            throw exceptionHelper.Resolve("noRecords");
        }

        /// <ignore/>
        public int ReadDataRecords(DataRecord[] buffer, int offset, int count)
        {
            this.EnsureNotDisposed();

            var read = this.parser.ParseRecords(this.headerRecord, buffer, offset, count);
            this.recordNumber += read;
            return read;
        }

        /// <ignore/>
        public void Close()
        {
            this.Dispose();
        }

        /// <ignore/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.disposed = true;
        }

        /// <ignore/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.leaveOpen)
            {
                this.parser.TextReader.Dispose();
            }
        }

        private void EnsureNotPassedFirstRecord()
        {
            if (this.RecordNumber > 0 || this.headerRecord != null)
            {
                throw exceptionHelper.Resolve("passedFirstRecord");
            }
        }

        private void EnsureNotDisposed()
        {
            if (this.disposed)
            {
                throw exceptionHelper.Resolve("disposed");
            }
        }
    }
}