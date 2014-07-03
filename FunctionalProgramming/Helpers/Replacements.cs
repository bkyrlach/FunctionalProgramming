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
            return int.TryParse(s, out parseResult) ? parseResult.ToMaybe() : MaybeExtensions.Nothing<int>();
        }

        /// <summary>
        /// Type safe way to parse a string to a Guid
        /// </summary>
        /// <param name="s">The string to attempt to parse as a Guid</param>
        /// <returns>The parsed Guid, or nothing if parsing fails</returns>
        public static IMaybe<Guid> SafeParseGuid(this string s)
        {
            Guid parseResult;
            return Guid.TryParse(s, out parseResult) ? parseResult.ToMaybe() : MaybeExtensions.Nothing<Guid>();
        }
    }
}
