using System;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Helpers
{
    /// <summary>
    /// Methods that should be part of the stdlib but aren't
    /// </summary>
    public static class Replacements
    {
        /// <summary>
        /// Type safe way to parse a string to an int
        /// </summary>
        /// <param name="s">The string to attempt to parse as an int</param>
        /// <returns>The parsed int, or nothing if parsing fails (note: not null, but Nothing 'int)</returns>
        public static IMaybe<int> SafeParseInt(this string s)
        {
            int parseResult;
            return int.TryParse(s, out parseResult) ? parseResult.ToMaybe() : Maybe.Nothing<int>();
        }

        /// <summary>
        /// Type safe way to parse a string to an ulong
        /// </summary>
        /// <param name="s">The string to attempt to parse as an ulong</param>
        /// <returns>The parsed ulong, or nothing if parsing fails (note: not null, but Nothing 'ulong)</returns>
        public static IMaybe<ulong> SafeParseULong(this string s)
        {
            ulong parseResult;
            return ulong.TryParse(s, out parseResult) ? Maybe.Pure(parseResult) : Maybe.Nothing<ulong>();
        }

        /// <summary>
        /// Type safe way to parse a string to a Guid
        /// </summary>
        /// <param name="s">The string to attempt to parse as a Guid</param>
        /// <returns>The parsed Guid, or nothing if parsing fails</returns>
        public static IMaybe<Guid> SafeParseGuid(this string s)
        {
            Guid parseResult;
            return Guid.TryParse(s, out parseResult) ? Maybe.Pure(parseResult) : Maybe.Nothing<Guid>();
        }

        /// <summary>
        /// Type safe way to parse a string to a bool
        /// </summary>
        /// <param name="s">The string to attempt to parse as a bool</param>
        /// <returns>The parsed bool, or nothing if parsing fails</returns>
        public static IMaybe<bool> SafeParseBool(this string s)
        {
            bool parseResult;
            return bool.TryParse(s, out parseResult) ? Maybe.Pure(parseResult) : Maybe.Nothing<bool>();
        }

         /// <summary>
        /// Type safe way to parse a string to a date
        /// </summary>
        /// <param name="s">The string to attempt to parse as a date</param>
        /// <returns>The parsed date, or nothing if parsing fails (note: not null, but Nothing 'date)</returns>
        public static IMaybe<DateTime> SafeParseDate(this string s)
        {
            DateTime parseResult;
            return DateTime.TryParse(s, out parseResult) ? Maybe.Pure(parseResult) : Maybe.Nothing<DateTime>();
        }
    }
}
