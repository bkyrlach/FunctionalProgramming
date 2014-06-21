using System;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Helpers
{
    public static class GuidHelpers
    {
        public static IMaybe<Guid> SafeTryParse(this string s)
        {
            Guid parseResult;
            return Guid.TryParse(s, out parseResult) ? parseResult.ToMaybe() : MaybeExtensions.Nothing<Guid>();
        }
    }
}
