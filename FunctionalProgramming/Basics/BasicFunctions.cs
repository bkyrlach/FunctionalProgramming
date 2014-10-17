using System;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;

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

        public static IEither<TLeft, TRight> EIf<TLeft, TRight>(bool predicate, Func<TRight> consequent,
            Func<TLeft> alternative)
        {
            return predicate ? consequent().AsRight<TLeft, TRight>() : alternative().AsLeft<TLeft, TRight>();
        }

        public static IEither<TLeft, TRight> EIf<TLeft, TRight>(bool predicate, Func<IEither<TLeft, TRight>> consequent, 
            Func<TLeft> alternative)
        {
            return predicate ? consequent() : alternative().AsLeft<TLeft, TRight>();
        }

        public static IEither<TLeft, TRight> EIf<TLeft, TRight>(bool predicate, Func<TRight> consequent,
            Func<IEither<TLeft, TRight>> alternative)
        {
            return predicate ? consequent().AsRight<TLeft, TRight>() : alternative();
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

        public static IMaybe<T> Cast<T>(this object o) where T : class
        {
            return (o as T).ToMaybe();
        }

        public static IMaybe<T> CastV<T>(this object o) where T : struct
        {
            return TryOps.Attempt(() => (T) o).AsMaybe();
        }

        #region Const
        public static Func<T1, T2> Const<T1, T2>(T2 t)
        {
            return ignored => t;
        }

        public static Func<T1, T2, T3> Const<T1, T2, T3>(T3 t)
        {
            return (ignored1, ignored2) => t;
        }

        public static Func<T1, T2, T3, T4> Const<T1, T2, T3, T4>(T4 t)
        {
            return (ignored1, ignored2, ignored3) => t;
        }

        public static Func<T1, T2, T3, T4, T5> Const<T1, T2, T3, T4, T5>(T5 t)
        {
            return (ignored1, ignored2, ignored3, ignored4) => t;
        }
        #endregion
    }
}
