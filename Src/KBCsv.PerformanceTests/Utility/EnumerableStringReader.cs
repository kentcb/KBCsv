namespace KBCsv.PerformanceTests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Provides a <see cref="TextReader"/> over an enumeration of <see cref="String"/>s.
    /// </summary>
    public sealed class EnumerableStringReader : TextReader
    {
        private readonly IEnumerator<string> enumerator;
        private string current;
        private int currentIndex;

        public EnumerableStringReader(IEnumerable<string> enumerable)
        {
            this.enumerator = enumerable.GetEnumerator();

            if (this.enumerator.MoveNext())
            {
                this.current = this.enumerator.Current;
            }
        }

        public override int Peek()
        {
            if (this.current == null)
            {
                return -1;
            }

            if (this.currentIndex == this.current.Length)
            {
                if (!this.ReadNext())
                {
                    return -1;
                }
            }

            return this.current[this.currentIndex];
        }

        public override int Read()
        {
            var result = this.Peek();

            if (result == -1)
            {
                return -1;
            }

            ++this.currentIndex;
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.enumerator.Dispose();
            }
        }

        private bool ReadNext()
        {
            if (!this.enumerator.MoveNext())
            {
                this.current = null;
                return false;
            }

            this.current = this.enumerator.Current;
            this.currentIndex = 0;

            return true;
        }
    }
}
