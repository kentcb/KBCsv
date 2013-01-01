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
        private const char Space = ' ';
        private const char Tab = '\t';
        private const char CR = (char)0x0d;
        private const char LF = (char)0x0a;
        private const int BufferSize = 4096;
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
                                if (this.IsBufferEmpty && !this.FillBuffer())
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
                            if (this.IsBufferEmpty && !this.FillBuffer())
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
                    else if (!this.FillBuffer())
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
                        this.valueBuilder.Append(ch, delimited);
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
                            this.valueBuilder.Append(ch, false);
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
                            this.valueBuilder.Append(this.valueDelimiter, true);
                            ++this.bufferIndex;
                        }
                        else
                        {
                            // delimiter isn't escaped, so we are no longer in a delimited area
                            this.delimited = false;
                        }
                    }
                    else
                    {
                        // it wasn't a special character after all, so just append it
                        this.valueBuilder.Append(ch, true);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPossiblySpecialCharacter(char ch)
        {
            return (ch & this.specialCharacterMask) == ch;
        }

        // fill the character buffer with data from the text reader
        private bool FillBuffer()
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                this.valueEndIndex = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(string value)
            {
                this.EnsureSufficientCapacity();
                this.values[this.valueEndIndex++] = value;
            }

            // convert this value list to an array, or null if there are no values
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string[] ToArray(string extra)
            {
                var result = new string[this.valueEndIndex + 1];
                Array.Copy(this.values, 0, result, 0, this.valueEndIndex);
                result[this.valueEndIndex] = extra;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        // lightweight value builder, which also encapsulates whitespace stripping logic
        // using this out-performs a StringBuilder significantly
        private sealed class ValueBuilder
        {
            private readonly CsvParser parser;
            private PendingChar[] pendingChars;
            private int pendingCharsEndIndex;
            private char[] buffer;
            private int bufferEndIndex;

            // preserved means either it's not whitespace, or it is delimited whitespace
            private int? firstPreservedIndexInclusive;
            private int? lastPreservedIndexExclusive;

            public ValueBuilder(CsvParser parser)
            {
                this.parser = parser;
                this.pendingChars = new PendingChar[256];
                this.buffer = new char[1024];

                Debug.Assert(this.pendingChars.Length <= this.buffer.Length, "Buffer is smaller than pending characters buffer.", "The pending characters buffer is of length {0} whilst the buffer is of length {1}. The buffer must never be smaller than the pending characters buffer.", this.pendingChars.Length, this.buffer.Length);
            }

            public bool HasValue
            {
                get { return this.bufferEndIndex > 0 || this.pendingCharsEndIndex > 0; }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                this.bufferEndIndex = 0;
                this.firstPreservedIndexInclusive = null;
                this.lastPreservedIndexExclusive = null;
            }

            // by pending appends until later, we can aggressively inline this method and make a big performance gain
            // we can also avoid the overhead of calling EnsureSufficientCapacity per character, instead calling it once every time we apply pending chars
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Append(char ch, bool delimited)
            {
                if (this.pendingCharsEndIndex == this.pendingChars.Length)
                {
                    // the pending characters buffer is full, so we need to drain it
                    this.ApplyPendingChars();
                }

                this.pendingChars[this.pendingCharsEndIndex++] = new PendingChar(ch, delimited || !IsWhiteSpace(ch));
            }

            private void ApplyPendingChars()
            {
                Debug.Assert(this.pendingCharsEndIndex > 0, "No pending chars.", "An attempt was made to apply pending characters, but there are none to apply.");

                this.EnsureSufficientCapacity(this.pendingCharsEndIndex);

                for (var i = 0; i < this.pendingCharsEndIndex; ++i)
                {
                    var pendingChar = this.pendingChars[i];

                    // we keep track of first and last preserved indexes so that we can retrospectively strip whitespace if necessary
                    if (pendingChar.IsPreserved)
                    {
                        if (!this.firstPreservedIndexInclusive.HasValue)
                        {
                            this.firstPreservedIndexInclusive = this.bufferEndIndex;
                        }

                        this.lastPreservedIndexExclusive = this.bufferEndIndex + 1;
                    }

                    this.buffer[this.bufferEndIndex++] = pendingChar.Char;
                }

                this.pendingCharsEndIndex = 0;
            }

            public override string ToString()
            {
                if (this.pendingCharsEndIndex != 0)
                {
                    this.ApplyPendingChars();
                }

                // the start and end indexes depend on whether white space is being preserved. If not, we use the first/last preserved indexes
                var startIndexInclusive = this.parser.preserveLeadingWhiteSpace ? 0 : this.firstPreservedIndexInclusive.GetValueOrDefault();
                var endIndexExclusive = this.parser.preserveTrailingWhiteSpace ? this.bufferEndIndex : this.lastPreservedIndexExclusive.GetValueOrDefault(0);

                var length = endIndexExclusive - startIndexInclusive;
                return new string(this.buffer, startIndexInclusive, length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void EnsureSufficientCapacity(int extraCapacityRequired)
            {
                if ((this.bufferEndIndex + extraCapacityRequired) > this.buffer.Length)
                {
                    // need to allocate larger buffer
                    var newBuffer = new char[this.buffer.Length * 2];
                    Array.Copy(this.buffer, 0, newBuffer, 0, this.buffer.Length);
                    this.buffer = newBuffer;
                }
            }

            // used to cache pending character appends so that we can apply them in bulk to save some cycles
            private struct PendingChar
            {
                public readonly char Char;
                public readonly bool IsPreserved;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public PendingChar(char ch, bool isPreserved)
                {
                    this.Char = ch;
                    this.IsPreserved = isPreserved;
                }
            }
        }
    }
}
