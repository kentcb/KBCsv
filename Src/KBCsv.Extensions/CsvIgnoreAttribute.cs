namespace KBCsv.Extensions
{
    using System;

    /// <summary>
    /// Indicates that a property should be ignored when using <see cref="EnumerableExtensions.WriteCsv"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvIgnoreAttribute : Attribute
    {
    }
}