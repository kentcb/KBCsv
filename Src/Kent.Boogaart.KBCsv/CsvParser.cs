using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Kent.Boogaart.HelperTrinity;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace Kent.Boogaart.KBCsv
{
    /// <summary>
    /// Implements the actual CSV parsing logic.
    /// </summary>
    internal sealed class CsvParser : IDisposable
    {
        public const char DefaultValueSeparator = ',';
        public const char DefaultValueDelimiter = '"';
        public const int BufferSize = 4096;
        private const char Space = ' ';
        private const char Tab = '\t';
        private const char CR = (char)0x0d;
        private const char LF = (char)0x0a;
        private static readonly ExceptionHelper exceptionHelper = new ExceptionHelper(typeof(CsvParser));
        private readonly TextReader reader;
        private readonly char[] buffer;
        private readonly ValueList values;
        private readonly ValueBuilder valueBuilder;
        private int bufferEndIndex;
        private int bufferIndex;
        private int specialCharacterMask;
        private bool preserveLeadingWhiteSpace;
        private bool preserveTrailingWhiteSpace;
        private bool passedFirstRecord;
        private bool delimited;
        private char valueSeparator;
        private char valueDelimiter;

        public CsvParser(TextReader reader)
        {
            reader.AssertNotNull("reader");

            this.reader = reader;
            this.buffer = new char[BufferSize];
            this.values = new ValueList();
            this.valueBuilder = new ValueBuilder(this);
            this.valueSeparator = DefaultValueSeparator;
            this.valueDelimiter = DefaultValueDelimiter;

            this.UpdateSpecialCharacterMask();
        }

        public bool PreserveLeadingWhiteSpace
        {
            get { return this.preserveLeadingWhiteSpace; }
            set { this.preserveLeadingWhiteSpace = value; }
        }

        public bool PreserveTrailingWhiteSpace
        {
            get { return this.preserveTrailingWhiteSpace; }
            set { this.preserveTrailingWhiteSpace = value; }
        }

        public char ValueSeparator
        {
            get { return this.valueSeparator; }
            set
            {
                exceptionHelper.ResolveAndThrowIf(value == this.valueDelimiter, "value-separator-same-as-value-delimiter");
                exceptionHelper.ResolveAndThrowIf(value == Space, "value-separator-or-value-delimiter-space");

                this.valueSeparator = value;
                this.UpdateSpecialCharacterMask();
            }
        }

        public char ValueDelimiter
        {
            get { return this.valueDelimiter; }
            set
            {
                exceptionHelper.ResolveAndThrowIf(value == this.valueSeparator, "value-separator-same-as-value-delimiter");
                exceptionHelper.ResolveAndThrowIf(value == Space, "value-separator-or-value-delimiter-space");

                this.valueDelimiter = value;
                this.UpdateSpecialCharacterMask();
            }
        }

        public bool HasMoreRecords
        {
            get
            {
                if (this.bufferIndex < this.bufferEndIndex)
                {
                    // the buffer isn't empty so there must be more records
                    return true;
                }

                // the buffer is empty, so we only have more records if we successfully fill it
                return this.FillBuffer();
            }
        }

        public bool PassedFirstRecord
        {
            get { return this.passedFirstRecord; }
            set { this.passedFirstRecord = value; }
        }

        private bool IsBufferEmpty
        {
            get { return this.bufferIndex == this.bufferEndIndex; }
        }

        public bool SkipRecord()
        {
            // Performance Notes:
            //   - Checking !HasMoreRecords and exiting early degrades performance
            //     Looking at the IL, I assume this is because the common case (more records) results in a branch, whereas the uncommon case (no more records) does not
            //   - Using a local to refer to the buffer yields better performance
            //     I haven't looked into the assembly, but I assume that it is able to better use registers
            //   - Using a local for other stuff (like the delimiter flag) yields poorer performance
            //     Again, I suspect this is related to effective use of registers, where too many locals thwarts the JITter
            //   - Temporarily swapping the special character mask to valueDelimiter whilst in a delimited area makes no noticeable improvement to performance

            Debug.Assert(!this.delimited, "Delimited is true.");

            if (this.HasMoreRecords)
            {
                var buffer = this.buffer;

                while (true)
                {
                    if (!this.IsBufferEmpty)
                    {
                        var ch = buffer[this.bufferIndex++];

                        if (!this.IsPossiblySpecialCharacter(ch))
                        {
                            // if it's definitely not a special character, then we can just continue on with the loop
                            continue;
                        }

                        if (!this.delimited)
                        {
                            if (ch == this.valueDelimiter)
                            {
                                this.delimited = true;
                            }
                            else if (ch == CR)
                            {
                                // we need to look at the next character, so make sure it is available
                                if (this.IsBufferEmpty && !this.FillBufferWithoutNotify())
                                {
                                    // last character available was CR, so we know we're done at this point
                                    return true;
                                }

                                // we deal with CRLF right here by checking if the next character is LF, in which case we just discard it
                                if (buffer[this.bufferIndex] == LF)
                                {
                                    ++this.bufferIndex;
                                }

                                return true;
                            }
                            else if (ch == LF)
                            {
                                return true;
                            }
                        }
                        else if (ch == this.valueDelimiter)
                        {
                            // we need to look at the next character, so make sure it is available
                            if (this.IsBufferEmpty && !this.FillBufferWithoutNotify())
                            {
                                return true;
                            }

                            if (buffer[this.bufferIndex] == this.valueDelimiter)
                            {
                                // delimiter is escaped, so just swallow it
                                ++this.bufferIndex;
                            }
                            else
                            {
                                // delimiter isn't escaped, so we are no longer in a delimited area
                                this.delimited = false;
                            }
                        }
                    }
                    else if (!this.FillBufferWithoutNotify())
                    {
                        // all out of data, so we successfully skipped the final record
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public string[] ParseRecord()
        {
            // many of the performance notes in SkipRecord are relevant here, too

            Debug.Assert(!this.delimited, "Delimited is true.");

            var buffer = this.buffer;
            var ch = char.MinValue;
            this.values.Clear();
            this.valueBuilder.Clear();

            while (true)
            {
                if (!this.IsBufferEmpty)
                {
                    ch = buffer[this.bufferIndex++];

                    if (!this.IsPossiblySpecialCharacter(ch))
                    {
                        // if it's definitely not a special character, then we can just append it and continue on with the loop
                        this.valueBuilder.NotifyPreviousCharIncluded(this.delimited);
                        continue;
                    }

                    if (!this.delimited)
                    {
                        if (ch == this.valueSeparator)
                        {
                            this.values.Add(this.valueBuilder.ToString());
                            this.valueBuilder.Clear();
                        }
                        else if (ch == this.valueDelimiter)
                        {
                            this.valueBuilder.NotifyPreviousCharExcluded();
                            this.delimited = true;
                        }
                        else if (ch == CR)
                        {
                            // we need to look at the next character, so make sure it is available
                            if (this.IsBufferEmpty && !this.FillBuffer())
                            {
                                // undelimited CR indicates the end of a record, so add the existing value and then exit
                                return this.values.ToArray(this.valueBuilder.ToString());
                            }

                            // we deal with CRLF right here by checking if the next character is LF, in which case we just discard it
                            if (buffer[this.bufferIndex] == LF)
                            {
                                ++this.bufferIndex;
                            }

                            // undelimited CR or CRLF both indicate the end of a record, so add the existing value and then exit
                            return this.values.ToArray(this.valueBuilder.ToString());
                        }
                        else if (ch == LF)
                        {
                            // undelimited LF indicates the end of a record, so add the existing value and then exit
                            return this.values.ToArray(this.valueBuilder.ToString());
                        }
                        else
                        {
                            // it wasn't a special character after all, so just append it
                            this.valueBuilder.NotifyPreviousCharIncluded(false);
                        }
                    }
                    else if (ch == this.valueDelimiter)
                    {
                        // we need to look at the next character, so make sure it is available
                        if (this.IsBufferEmpty && !this.FillBuffer())
                        {
                            // out of data
                            delimited = false;
                            return this.values.ToArray(this.valueBuilder.ToString());
                        }

                        if (buffer[this.bufferIndex] == this.valueDelimiter)
                        {
                            // delimiter is escaped, so append it to the value and discard the escape character
                            this.valueBuilder.NotifyPreviousCharExcluded();
                            ++this.bufferIndex;
                            this.valueBuilder.NotifyPreviousCharIncluded(true);
                        }
                        else
                        {
                            // delimiter isn't escaped, so we are no longer in a delimited area
                            this.valueBuilder.NotifyPreviousCharExcluded();
                            this.delimited = false;
                        }
                    }
                    else
                    {
                        // it wasn't a special character after all, so just append it
                        this.valueBuilder.NotifyPreviousCharIncluded(true);
                    }
                }
                else if (!this.FillBuffer())
                {
                    if (this.valueBuilder.HasValue)
                    {
                        // a value is outstanding, so add it
                        this.values.Add(this.valueBuilder.ToString());
                    }

                    if (ch == this.valueSeparator)
                    {
                        // special case: last character is a separator, which means there should be an empty value after it. eg. "foo," results in ["foo", ""]
                        return this.values.ToArray(string.Empty);
                    }

                    // data exhausted - we're done. Note that this will return null if there are no values
                    return this.values.ToArray();
                }
            }
        }

        public void Close()
        {
            this.reader.Close();
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        private static bool IsWhiteSpace(char ch)
        {
            return ch == Space || ch == Tab;
        }

        // if a character matches this mask then there's a chance it is a special character
        // if it fails to match the mask, we know very quickly that it definitely isn't a special character
        // this is basically used for a bloom filter
        private void UpdateSpecialCharacterMask()
        {
            this.specialCharacterMask = this.valueSeparator | this.valueDelimiter | CR | LF;
        }

        // gets a value indicating whether the character is possibly a special character
        // as indicated by the name, false positives are possible, false negatives are not
        // that is, this may return true even for a character that isn't special, but will never return false for a character that is special
        private bool IsPossiblySpecialCharacter(char ch)
        {
            return (ch & this.specialCharacterMask) == ch;
        }

        // fill the character buffer with data from the text reader
        private bool FillBuffer()
        {
            Debug.Assert(this.IsBufferEmpty, "Buffer not empty.", "The buffer is not empty because the buffer index ({0}) does not equal the buffer end index ({1}).", this.bufferIndex, this.bufferEndIndex);

            this.valueBuilder.NotifyBufferRefilling();
            this.bufferEndIndex = this.reader.Read(this.buffer, 0, BufferSize);
            this.bufferIndex = 0;
            this.passedFirstRecord = true;

            return this.bufferEndIndex > 0;
        }

        // fill the character buffer with data from the text reader. Does not notify the value builder that the fill is taking place, which is useful when the value builder is irrelevant (such as when skipping records)
        private bool FillBufferWithoutNotify()
        {
            Debug.Assert(this.IsBufferEmpty, "Buffer not empty.", "The buffer is not empty because the buffer index ({0}) does not equal the buffer end index ({1}).", this.bufferIndex, this.bufferEndIndex);

            this.bufferEndIndex = this.reader.Read(this.buffer, 0, BufferSize);
            this.bufferIndex = 0;
            this.passedFirstRecord = true;

            return this.bufferEndIndex > 0;
        }

        // lightweight list to store the values comprising the record currently being parsed
        // using this out-performs List<string> significantly
        // aggressive inlining also has a significantly positive effect on performance
        private sealed class ValueList
        {
            private string[] values;
            private int valueEndIndex;

            public ValueList()
            {
                this.values = new string[64];
            }

            public int Count
            {
                get { return this.valueEndIndex; }
            }

            public void Clear()
            {
                this.valueEndIndex = 0;
            }

            public void Add(string value)
            {
                this.EnsureSufficientCapacity();
                this.values[this.valueEndIndex++] = value;
            }

            // convert this value list to an array, or null if there are no values
            public string[] ToArray()
            {
                if (this.valueEndIndex == 0)
                {
                    return null;
                }

                var result = new string[this.valueEndIndex];
                Array.Copy(this.values, 0, result, 0, this.valueEndIndex);
                return result;
            }

            // convert this value list to an array, placing the extra value at the end of the array
            // this saves the client code having to add the value and then call ToArray, which is more expensive than just doing it in one step
            public string[] ToArray(string extra)
            {
                var result = new string[this.valueEndIndex + 1];
                Array.Copy(this.values, 0, result, 0, this.valueEndIndex);
                result[this.valueEndIndex] = extra;
                return result;
            }

            private void EnsureSufficientCapacity()
            {
                if (this.valueEndIndex == this.values.Length)
                {
                    // need to reallocate larger values array
                    var newValues = new string[this.values.Length * 2];
                    Array.Copy(this.values, 0, newValues, 0, this.values.Length);
                    this.values = newValues;
                }
            }
        }

        private sealed class ValueBuilder
        {
            private readonly CsvParser parser;

            // the total length of the value being built, regardless of whether it is stored in the parser's buffer or our own local buffer
            private int length;

            // the index into the parser's buffer where the value (or value part) starts, along with the length of the value (or value part) in the parser's buffer
            private int bufferStartIndex;
            private int bufferLength;

            // a local buffer (only used if necessary), along with the length of the value (or value part) stored within it
            private char[] localBuffer;
            private int localBufferLength;

            // indexes (relative to the start of the value) indicating where the first and last delimited characters are, if at all
            private int? delimitedStartIndex;
            private int? delimitedEndIndex;

            public ValueBuilder(CsvParser parser)
            {
                this.parser = parser;

                // to make the resize logic faster, our local buffer is the same size as the parser's buffer
                this.localBuffer = new char[BufferSize];
            }

            public bool HasValue
            {
                get { return this.length > 0; }
            }

            // reset the value builder so it can start building a new value
            public void Clear()
            {
                this.length = 0;
                this.bufferLength = 0;
                this.localBufferLength = 0;
                this.delimitedStartIndex = null;
                this.delimitedEndIndex = null;
            }

            // tell the value builder that the previous character parsed should be included in the value
            // the delimited parameter tells the value builder whether the character in question appeared within a delimited area
            public void NotifyPreviousCharIncluded(bool delimited)
            {
                if (this.bufferLength == 0)
                {
                    // this is the first included char (for this demarcation), so we need to set our buffer start index
                    this.bufferStartIndex = this.parser.bufferIndex - 1;
                }

                // the overall value length has increased, as well as the piece of the value within the parser's buffer
                ++this.length;
                ++this.bufferLength;

                if (delimited)
                {
                    if (!this.delimitedStartIndex.HasValue)
                    {
                        // haven't had a delimited character yet, so set the delimited index appropriately
                        this.delimitedStartIndex = this.length - 1;
                    }

                    this.delimitedEndIndex = this.length;
                }
            }

            // tell the value builder that the previous character parsed should not be included in the value
            // this might happen, for example, with delimiters in the value
            public void NotifyPreviousCharExcluded()
            {
                // since the value includes at least one extraneous character, we can't simply grab it straight out of the parser's buffer
                // therefore, we copy what we have demarcated so far into our local buffer and use that instead
                this.CopyBufferDemarcationToLocalBuffer();
            }

            // tell the value builder that the parser's buffer is about to be refilled because it is exhausted
            public void NotifyBufferRefilling()
            {
                // the value spans more than one parser buffer, so we have to save what we have demarcated so far into our local buffer and use that instead
                // of trying to just use the parser's buffer
                this.CopyBufferDemarcationToLocalBuffer();
                this.bufferStartIndex = 0;
            }

            // get the value built by the value builder
            public override string ToString()
            {
                if (this.localBufferLength == 0)
                {
                    // fast path: the value fit entirely and contiguously in the parser's buffer, so we didn't need to copy anything to our local buffer
                    var buffer = this.parser.buffer;
                    var startIndex = this.delimitedStartIndex.GetValueOrDefault(this.bufferStartIndex);
                    var endIndex = startIndex + this.delimitedEndIndex.GetValueOrDefault(this.bufferLength);

                    if (!this.parser.preserveLeadingWhiteSpace)
                    {
                        while (startIndex < endIndex && IsWhiteSpace(buffer[startIndex]))
                        {
                            ++startIndex;
                        }
                    }

                    if (!this.parser.preserveTrailingWhiteSpace)
                    {
                        while (endIndex > startIndex && IsWhiteSpace(buffer[endIndex - 1]))
                        {
                            --endIndex;
                        }
                    }

                    return new string(buffer, startIndex, endIndex - startIndex);
                }
                else
                {
                    // slow path: we had to use our local buffer to construct the value

                    // copy any outstanding data to our local buffer
                    this.CopyBufferDemarcationToLocalBuffer();

                    var buffer = this.localBuffer;
                    var startIndex = 0;
                    var endIndex = this.localBufferLength;

                    if (!this.parser.preserveLeadingWhiteSpace)
                    {
                        var stripWhiteSpaceUpToIndex = this.delimitedStartIndex.GetValueOrDefault(endIndex);

                        while (startIndex < stripWhiteSpaceUpToIndex  && IsWhiteSpace(buffer[startIndex]))
                        {
                            ++startIndex;
                        }
                    }

                    if (!this.parser.preserveTrailingWhiteSpace)
                    {
                        var stripWhiteSpaceDownToIndex = this.delimitedEndIndex.GetValueOrDefault(startIndex);

                        while (endIndex > stripWhiteSpaceDownToIndex && IsWhiteSpace(buffer[endIndex - 1]))
                        {
                            --endIndex;
                        }
                    }

                    return new string(buffer, startIndex, endIndex - startIndex);
                }
            }

            private void CopyBufferDemarcationToLocalBuffer()
            {
                if (this.bufferLength > 0)
                {
                    this.EnsureLocalBufferHasSufficientCapacity(this.bufferLength);

                    // copy what we demarcated in the parser's buffer into our local buffer
                    Array.Copy(this.parser.buffer, this.bufferStartIndex, this.localBuffer, this.localBufferLength, this.bufferLength);

                    this.localBufferLength += this.bufferLength;

                    // reset the demarcation of the parser's buffer back to nothing
                    this.bufferLength = 0;
                }
            }

            private void EnsureLocalBufferHasSufficientCapacity(int extraCapacityRequired)
            {
                Debug.Assert(this.localBuffer.Length >= BufferSize, "Local buffer is smaller than parser buffer.", "This method is not correct unless this assertion holds true. This saves having to do a Math.Max call to determine the new buffer size.");

                if ((this.localBufferLength + extraCapacityRequired) > this.localBuffer.Length)
                {
                    // need to allocate larger buffer
                    var newBuffer = new char[this.localBuffer.Length * 2];
                    Array.Copy(this.localBuffer, 0, newBuffer, 0, this.localBuffer.Length);
                    this.localBuffer = newBuffer;
                }
            }
        }
    }
}
