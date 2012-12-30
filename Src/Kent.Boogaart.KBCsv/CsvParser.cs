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
                else
                {
                    // the buffer is empty, so we only have more records if we successfully fill it
                    return this.FillBuffer();
                }			
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
            if (!this.HasMoreRecords)
            {
                // no record to skip
                return false;
            }

            var delimitState = DelimitState.Undelimited;
            var previousCharacterState = PreviousCharacterState.NothingImportant;

            while (true)
            {
                if (this.IsBufferEmpty &&
                    !this.FillBuffer())
                {
                    // all out of data, so we've successfully skipped the last record
                    return true;
                }

                var ch = this.buffer[this.bufferIndex];

                // fast path: no outstanding state to apply and not a special character, so just skip the character and continue with loop
                if (previousCharacterState == PreviousCharacterState.NothingImportant && !this.IsPossiblySpecialCharacter(ch))
                {
                    ++this.bufferIndex;
                    continue;
                }

                // apply any outstanding state from the last character parsed
                if (previousCharacterState == PreviousCharacterState.CarriageReturn)
                {
                    if (ch == LF)
                    {
                        // skip over the LF in the CRLF combination
                        ++this.bufferIndex;
                    }

                    // undelimited CR or CRLF both indicate the end of a record
                    return true;
                }
                else if (previousCharacterState == PreviousCharacterState.DelimiterInDelimitedArea)
                {
                    if (ch == this.valueDelimiter)
                    {
                        // delimiter was escaped, so skip it and continue on
                        previousCharacterState = PreviousCharacterState.NothingImportant;
                        ++this.bufferIndex;
                        continue;
                    }
                    else
                    {
                        // delimiter not escaped in a delimited area, so we are no longer delimited
                        delimitState = DelimitState.Undelimited;
                        previousCharacterState = PreviousCharacterState.NothingImportant;
                    }
                }

                ++this.bufferIndex;

                if (delimitState == DelimitState.Undelimited)
                {
                    if (ch == this.valueDelimiter)
                    {
                        delimitState = DelimitState.Delimited;
                    }
                    else if (ch == CR)
                    {
                        // if the next character is LF, we need to swallow it before returning, so we set this state. Either way, an undelimited carriage return means the end of the record
                        previousCharacterState = PreviousCharacterState.CarriageReturn;
                    }
                    else if (ch == LF)
                    {
                        return true;
                    }
                }
                else if (ch == this.valueDelimiter)
                {
                    // we've read a value delimiter in a delimited area. What we do with it is dependent upon the next character read
                    previousCharacterState = PreviousCharacterState.DelimiterInDelimitedArea;
                }
            }
        }

        public string[] ParseRecord()
        {
            if (!this.HasMoreRecords)
            {
                return null;
            }

            this.values.Clear();
            this.valueBuilder.Clear();

            var delimitState = DelimitState.Undelimited;
            var previousCharacterState = PreviousCharacterState.NothingImportant;

            while (true)
            {
                if (this.IsBufferEmpty && !this.FillBuffer())
                {
                    this.values.Add(this.valueBuilder.ToString());

                    // data exhausted - we're done
                    break;
                }

                var ch = this.buffer[this.bufferIndex];

                // fast path: no outstanding state to apply and not a special character, so just add the character and continue with loop
                if (previousCharacterState == PreviousCharacterState.NothingImportant && !this.IsPossiblySpecialCharacter(ch))
                {
                    this.valueBuilder.Append(ch, delimitState == DelimitState.Delimited);
                    ++this.bufferIndex;
                    continue;
                }

                // apply any outstanding state from the last character parsed
                if (previousCharacterState == PreviousCharacterState.CarriageReturn)
                {
                    if (ch == LF)
                    {
                        // skip over the LF in the CRLF combination
                        ++this.bufferIndex;
                    }

                    // undelimited CR or CRLF both indicate the end of a record, so add the existing value and then exit
                    this.values.Add(this.valueBuilder.ToString());
                    break;
                }
                else if (previousCharacterState == PreviousCharacterState.DelimiterInDelimitedArea)
                {
                    if (ch == this.valueDelimiter)
                    {
                        // delimiter was escaped, so add the delimiter to the value and continue on
                        this.valueBuilder.Append(this.valueDelimiter, delimitState == DelimitState.Delimited);
                        previousCharacterState = PreviousCharacterState.NothingImportant;
                        ++this.bufferIndex;
                        continue;
                    }
                    else
                    {
                        // delimiter not escaped in a delimited area, so we are no longer delimited
                        delimitState = DelimitState.Undelimited;
                        previousCharacterState = PreviousCharacterState.NothingImportant;
                    }
                }

                ++this.bufferIndex;

                if (delimitState == DelimitState.Undelimited)
                {
                    if (ch == this.valueDelimiter)
                    {
                        delimitState = DelimitState.Delimited;
                    }
                    else if (ch == this.valueSeparator)
                    {
                        this.values.Add(this.valueBuilder.ToString());
                        this.valueBuilder.Clear();
                        delimitState = DelimitState.Undelimited;
                        previousCharacterState = PreviousCharacterState.NothingImportant;
                    }
                    else if (ch == CR)
                    {
                        // if the next character is LF, we need to swallow it before returning, so we set this state. Either way, an undelimited carriage return means the end of the record
                        previousCharacterState = PreviousCharacterState.CarriageReturn;
                    }
                    else if (ch == LF)
                    {
                        this.values.Add(this.valueBuilder.ToString());
                        break;
                    }
                    else
                    {
                        this.valueBuilder.Append(ch, false);
                    }
                }
                else if (ch == this.valueDelimiter)
                {
                    // we've read a value delimiter in a delimited area. What we do with it is dependent upon the next character read
                    previousCharacterState = PreviousCharacterState.DelimiterInDelimitedArea;
                }
                else
                {
                    this.valueBuilder.Append(ch, delimitState == DelimitState.Delimited);
                }
            }

            if (this.values.Count == 0)
            {
                return null;
            }

            return this.values.ToArray();
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
        // false positives are possible, false negatives are not
        // ie. may return true when ch is not special, but will never return false if it is special
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPossiblySpecialCharacter(char ch)
        {
            return (ch & this.specialCharacterMask) == ch;
        }

        // fill the character buffer with data from the text reader
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FillBuffer()
        {
            Debug.Assert(this.IsBufferEmpty, "Buffer not empty.", "The buffer is not empty because the buffer index ({0}) does not equal the buffer end index ({1}).", this.bufferIndex, this.bufferEndIndex);

            this.bufferEndIndex = this.reader.Read(this.buffer, 0, BufferSize);
            this.bufferIndex = 0;
            this.passedFirstRecord = true;

            return this.bufferEndIndex > 0;
        }

        // used to track whether the parser is within a delimited area or not
        private enum DelimitState
        {
            Undelimited,
            Delimited
        }

        // used to track whether a previously read character may need to be enacted when a subsequent character is read
        private enum PreviousCharacterState
        {
            NothingImportant,
            DelimiterInDelimitedArea,
            CarriageReturn
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string[] ToArray()
            {
                var result = new string[this.valueEndIndex];
                Array.Copy(this.values, 0, result, 0, this.valueEndIndex);
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
            private char[] buffer;
            private int bufferEndIndex;
            private int? firstDelimitedIndexInclusive;
            private int? lastDelimitedIndexExclusive;

            public ValueBuilder(CsvParser parser)
            {
                this.parser = parser;
                this.buffer = new char[1024];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                this.bufferEndIndex = 0;
                this.firstDelimitedIndexInclusive = null;
                this.lastDelimitedIndexExclusive = null;
            }

            public void Append(char ch, bool delimited)
            {
                // we keep track of first and last delimited indexes so that we can retrospectively strip whitespace
                if (delimited)
                {
                    if (!this.firstDelimitedIndexInclusive.HasValue)
                    {
                        this.firstDelimitedIndexInclusive = this.bufferEndIndex;
                    }

                    this.lastDelimitedIndexExclusive = this.bufferEndIndex + 1;
                }

                this.EnsureSufficientCapacity();
                this.buffer[this.bufferEndIndex++] = ch;
            }

            public override string ToString()
            {
                var startIndexInclusive = 0;
                var endIndexExclusive = this.bufferEndIndex;

                if (!this.parser.preserveLeadingWhiteSpace)
                {
                    // strip leading whitespace up to either the first delimited index, or the end index
                    var delimitStartIndex = this.firstDelimitedIndexInclusive.GetValueOrDefault(endIndexExclusive - 1);

                    while (startIndexInclusive < delimitStartIndex && IsWhiteSpace(this.buffer[startIndexInclusive]))
                    {
                        ++startIndexInclusive;
                    }
                }

                if (!this.parser.preserveTrailingWhiteSpace)
                {
                    // strip trailing whitespace back to either the last delimited index, or the start index
                    var delimitEndIndex = this.lastDelimitedIndexExclusive.GetValueOrDefault(startIndexInclusive);

                    while (endIndexExclusive > delimitEndIndex && IsWhiteSpace(this.buffer[endIndexExclusive - 1]))
                    {
                        --endIndexExclusive;
                    }
                }

                var length = endIndexExclusive - startIndexInclusive;
                return new string(this.buffer, startIndexInclusive, length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void EnsureSufficientCapacity()
            {
                if (this.bufferEndIndex == this.buffer.Length)
                {
                    // need to reallocate larger buffer
                    var newBuffer = new char[this.buffer.Length * 2];
                    Array.Copy(this.buffer, 0, newBuffer, 0, this.buffer.Length);
                    this.buffer = newBuffer;
                }
            }
        }
    }
}
