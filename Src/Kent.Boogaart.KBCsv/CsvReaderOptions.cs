using System;

namespace Ingenious.KBCsv
{
	/// <summary>
	/// Defines possible options for parsing CSV data.
	/// </summary>
	[Flags]
	public enum CsvReaderOptions
	{
		/// <summary>
		/// No options defined.
		/// </summary>
		None = 0,
		/// <summary>
		/// Specifies that the parser should not discard leading white space that is not delimited.
		/// </summary>
		/// <remarks>
		/// The default behaviour of the parser is to discard leading white space if it is not delimited. Specifying this options forces the parser
		/// to preserve this white space.
		/// </remarks>
		PreserveLeadingWhiteSpace = 1,
		/// <summary>
		/// Specifies that the parser should not discard trailing white space that is not delimited.
		/// </summary>
		/// <remarks>
		/// The default behaviour of the parser is to discard trailing white space if it is not delimited. Specifying this options forces the parser
		/// to preserve this white space.
		/// </remarks>
		PreserveTrailingWhiteSpace = 2
	}
}
