using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Helpers
{
    public static class IntHelpers
    {
        /// <summary>
        /// Type safe way to parse a string to an Integer
        /// </summary>
        /// <param name="s">The string to attempt to parse as an int</param>
        /// <returns>The parsed int, or nothing if parsing fails (note: not null, but Nothing 'int)</returns>
        public static IMaybe<int> SafeTryParse(this string s)
        {
            int parseResult;
            return int.TryParse(s, out parseResult) ? parseResult.ToMaybe() : MaybeExtensions.Nothing<int>();
        }
    }
}
