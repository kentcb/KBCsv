namespace Kent.Boogaart.KBCsv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Kent.Boogaart.HelperTrinity;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Kent.Boogaart.KBCsv.Internal;

    /// <ignore/>
    public partial class CsvWriter : IDisposable
    {
        private static readonly ExceptionHelper exceptionHelper = new ExceptionHelper(typeof(CsvWriter));
        private readonly TextWriter textWriter;
        private readonly bool leaveOpen;
        private readonly StringBuilder valueBuilder;
        private readonly StringBuilder bufferBuilder;
        private bool forceDelimit;
        private bool disposed;
        private char valueDelimiter;
        private char valueSeparator;
        private long recordNumber;

        /// <ignore/>
        public CsvWriter(Stream stream)
            : this(stream, Constants.DefaultEncoding)
        {
        }

        /// <ignore/>
        public CsvWriter(Stream stream, Encoding encoding)
            : this(stream, encoding, false)
        {
        }

        /// <ignore/>
        public CsvWriter(Stream stream, Encoding encoding, bool leaveOpen)
            : this(new StreamWriter(stream, encoding), false)
        {
        }

        /// <ignore/>
        public CsvWriter(string path)
            : this(path, Constants.DefaultEncoding)
        {
        }

        /// <ignore/>
        public CsvWriter(string path, Encoding encoding)
            : this(path, false, encoding)
        {
        }

        /// <ignore/>
        public CsvWriter(string path, bool append)
            : this(path, append, Constants.DefaultEncoding)
        {
        }

        /// <ignore/>
        public CsvWriter(string path, bool append, Encoding encoding)
            : this(new StreamWriter(path, append, encoding))
        {
        }

        /// <ignore/>
        public CsvWriter(TextWriter textWriter)
            : this(textWriter, false)
        {
        }

        /// <ignore/>
        public CsvWriter(TextWriter textWriter, bool leaveOpen)
        {
            textWriter.AssertNotNull("textWriter");

            this.textWriter = textWriter;
            this.leaveOpen = leaveOpen;
            this.valueBuilder = new StringBuilder(128);
            this.bufferBuilder = new StringBuilder(2048);
            this.valueSeparator = Constants.DefaultValueSeparator;
            this.valueDelimiter = Constants.DefaultValueDelimiter;
        }

        /// <ignore/>
        public Encoding Encoding
        {
            get
            {
                this.EnsureNotDisposed();
                return this.textWriter.Encoding;
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
        public bool ForceDelimit
        {
            get
            {
                this.EnsureNotDisposed();
                return this.forceDelimit;
            }

            set
            {
                this.EnsureNotDisposed();
                this.forceDelimit = value;
            }
        }

        /// <ignore/>
        public char ValueSeparator
        {
            get
            {
                this.EnsureNotDisposed();
                return this.valueSeparator;
            }

            set
            {
                this.EnsureNotDisposed();
                exceptionHelper.ResolveAndThrowIf(value == this.valueDelimiter, "valueSeparatorAndDelimiterCannotMatch");
                exceptionHelper.ResolveAndThrowIf(IsWhitespace(value), "valueSeparatorAndDelimiterCannotBeWhiteSpace");
                this.valueSeparator = value;
            }
        }

        /// <ignore/>
        public char ValueDelimiter
        {
            get
            {
                this.EnsureNotDisposed();
                return this.valueDelimiter;
            }

            set
            {
                this.EnsureNotDisposed();
                exceptionHelper.ResolveAndThrowIf(value == this.valueSeparator, "valueSeparatorAndDelimiterCannotMatch");
                exceptionHelper.ResolveAndThrowIf(IsWhitespace(value), "valueSeparatorAndDelimiterCannotBeWhiteSpace");
                this.valueDelimiter = value;
            }
        }

        /// <ignore/>
        public string NewLine
        {
            get
            {
                this.EnsureNotDisposed();
                return this.textWriter.NewLine;
            }

            set
            {
                this.EnsureNotDisposed();
                this.textWriter.NewLine = value;
            }
        }

        /// <ignore/>
        public void WriteRecord(RecordBase record)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            record.AssertNotNull("record");
            this.WriteRecordToBuffer(record);
            this.FlushBufferToTextWriter();
        }

        /// <ignore/>
        public void WriteRecord(params string[] values)
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            values.AssertNotNull("values");
            this.WriteRecordToBuffer(values);
            this.FlushBufferToTextWriter();
        }

        /// <ignore/>
        public void WriteRecords(RecordBase[] buffer, int offset, int length)
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
            this.FlushBufferToTextWriter();
        }

        /// <ignore/>
        public void Flush()
        {
            Debug.Assert(this.bufferBuilder.Length == 0, "Expecting buffer to be empty.");

            this.EnsureNotDisposed();
            this.textWriter.Flush();
        }

        /// <ignore/>
        public void Close()
        {
            this.Dispose();
        }

        /// <ignore/>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.disposed = true;
        }

        /// <ignore/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.leaveOpen)
            {
                this.textWriter.Dispose();
            }
        }

        private static bool IsWhitespace(char ch)
        {
            return ch == Constants.Space || ch == Constants.Tab;
        }

        private void EnsureNotDisposed()
        {
            if (this.disposed)
            {
                throw exceptionHelper.Resolve("disposed");
            }
        }

        // synchronously push whatever's in the buffer to the text writer and reset the buffer
        private void FlushBufferToTextWriter()
        {
            this.textWriter.Write(this.bufferBuilder.ToString());
            this.bufferBuilder.Length = 0;
        }

        // write a record to the buffer builder
        private void WriteRecordToBuffer(IEnumerable<string> values)
        {
            Debug.Assert(values != null, "Expecting non-null values.");

            var firstValue = true;

            foreach (var value in values)
            {
                if (!firstValue)
                {
                    this.bufferBuilder.Append(this.valueSeparator);
                }

                this.WriteValueToBuffer(value);
                firstValue = false;
            }

            this.bufferBuilder.Append(this.NewLine);
            ++this.recordNumber;
        }

        // write value to the buffer, escaping embedded delimiters, and wrapping in delimiters as necessary
        private void WriteValueToBuffer(string value)
        {
            var delimit = this.forceDelimit;
            this.valueBuilder.Length = 0;

            if (!string.IsNullOrEmpty(value))
            {
                // delimit to preserve white-space at the beginning or end of the value
                if (IsWhitespace(value[0]) || IsWhitespace(value[value.Length - 1]))
                {
                    delimit = true;
                }

                for (var i = 0; i < value.Length; ++i)
                {
                    var ch = value[i];

                    if ((ch == this.valueSeparator) || (ch == Constants.CR) || (ch == Constants.LF))
                    {
                        // all these characters require the value to be delimited
                        this.valueBuilder.Append(ch);
                        delimit = true;
                    }
                    else if (ch == this.valueDelimiter)
                    {
                        // if the value contains the delimiter, we need to delimit the value and repeat the delimiter within to escape it
                        this.valueBuilder.Append(this.valueDelimiter, 2);
                        delimit = true;
                    }
                    else
                    {
                        this.valueBuilder.Append(ch);
                    }
                }
            }

            if (delimit)
            {
                this.bufferBuilder.Append(this.valueDelimiter).Append(this.valueBuilder).Append(this.valueDelimiter);
            }
            else
            {
                this.bufferBuilder.Append(this.valueBuilder);
            }
        }
    }
}