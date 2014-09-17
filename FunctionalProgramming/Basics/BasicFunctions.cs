using System;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Basics
{
    /// <summary>
    /// Basic functions missing from the .NET standard library.
    /// </summary>
    public static class BasicFunctions
    {
        /// <summary>
        /// An implementation of the C# keyword "if" that allows cleaner expressions than the ternary operator in
        /// some cases. If the predicate holds, then consequent, else alternative.
        /// </summary>
        /// <typeparam name="TResult">The type yielded from the consequent and alternative</typeparam>
        /// <param name="predicate">The predicate</param>
        /// <param name="consequent">The consequent</param>
        /// <param name="alternative">The alternative</param>
        /// <returns>The value yielded from consequent if predicate holds, else the value yielded from alternative</returns>
        public static TResult If<TResult>(bool predicate, Func<TResult> consequent, Func<TResult> alternative)
        {
            return predicate ? consequent() : alternative();
        }

        /// <summary>
        /// The standard identity function. Useful in functional programming, it just gives back the value presented.
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="t">The value that you'll get back</param>
        /// <returns>The value you give it</returns>
        public static T Identity<T>(T t)
        {
            return t;
        }

        public static Io<Unit> Using(Func<IDisposable> source, Action<IDisposable> body)
        {
            return Io<Unit>.Apply(() =>
            {
                using (var resource = source())
                {
                    body(resource);
                }
                return Unit.Only;
            });
        } 
    }
}
