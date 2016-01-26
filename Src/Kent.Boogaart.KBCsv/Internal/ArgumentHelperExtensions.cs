// taken from https://github.com/kentcb/TheHelperTrinity/

namespace Kent.Boogaart.KBCsv.Internal
{
    using System.Collections;
    using System.Collections.Generic;

    internal static class ArgumentHelperExtensions
    {
        public static void AssertNotNull<T>(this T arg, string argName)
            where T : class
        {
            ArgumentHelper.AssertNotNull(arg, argName);
        }

        public static void AssertNotNull<T>(this T? arg, string argName)
            where T : struct
        {
            ArgumentHelper.AssertNotNull(arg, argName);
        }

        public static void AssertGenericArgumentNotNull<T>(this T arg, string argName)
        {
            ArgumentHelper.AssertGenericArgumentNotNull(arg, argName);
        }

        public static void AssertNotNull<T>(this IEnumerable<T> arg, string argName, bool assertContentsNotNull)
        {
            ArgumentHelper.AssertNotNull(arg, argName, assertContentsNotNull);
        }

        public static void AssertNotNullOrEmpty(this string arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrEmpty(arg, argName);
        }

        public static void AssertNotNullOrEmpty(this IEnumerable arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrEmpty(arg, argName);
        }

        public static void AssertNotNullOrEmpty(this ICollection arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrEmpty(arg, argName);
        }

        public static void AssertNotNullOrWhiteSpace(this string arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrWhiteSpace(arg, argName);
        }

        //public static void AssertEnumMember<TEnum>(this TEnum enumValue, string argName)
        //    where TEnum : struct, IConvertible
        //{
        //    ArgumentHelper.AssertEnumMember(enumValue, argName);
        //}

        //public static void AssertEnumMember<TEnum>(this TEnum enumValue, string argName, params TEnum[] validValues)
        //    where TEnum : struct, IConvertible
        //{
        //    ArgumentHelper.AssertEnumMember(enumValue, argName, validValues);
        //}
    }
}