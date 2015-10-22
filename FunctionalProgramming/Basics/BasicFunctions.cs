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
        /// Convenience function for returning disparate types as a result of satisfying (or failing to satisfy) a predicate.
        /// This function returns a disjoint union TLeft \/ TRight
        /// </summary>
        /// <typeparam name="TLeft">The type of value we will get if the predicate is unsatisfied</typeparam>
        /// <typeparam name="TRight">The type of value we will get if the predicate is satisfied</typeparam>
        /// <param name="predicate">A hypothesis expressed as a boolean</param>
        /// <param name="consequent">The conclusion we can draw from the predicate holding</param>
        /// <param name="alternative">The conclusion we can draw from the predicate failing to hold</param>
        /// <returns>TLeft \/ TRight</returns>
        public static IEither<TLeft, TRight> EIf<TLeft, TRight>(bool predicate, Func<TRight> consequent,
            Func<TLeft> alternative)
        {
            return predicate ? consequent().AsRight<TLeft, TRight>() : alternative().AsLeft<TLeft, TRight>();
        }

        /// <summary>
        /// Convenience function for returning disparate types where the consequent branch is already returning a disjoint
        /// union.
        /// </summary>
        /// <typeparam name="TLeft">The type of value we will get if the predicate is unsatisfied</typeparam>
        /// <typeparam name="TRight">The type of value we will get if the predicate is satisfied</typeparam>
        /// <param name="predicate">A hypothesis expressed as a boolean</param>
        /// <param name="consequent">The conclusion we can draw from the predicate holding (returns TLeft \/ TRight)</param>
        /// <param name="alternative">The conclusion we can draw from the predicate failing to hold (returns TLeft)</param>
        /// <returns>TLeft \/ TRight</returns>
        public static IEither<TLeft, TRight> EIf<TLeft, TRight>(bool predicate, Func<IEither<TLeft, TRight>> consequent, 
            Func<TLeft> alternative)
        {
            return predicate ? consequent() : alternative().AsLeft<TLeft, TRight>();
        }

        /// <summary>
        /// Convenience function for returning disparate types where the alternative branch is already returning a disjoint
        /// union.
        /// </summary>
        /// <typeparam name="TLeft">The type of value we will get if the predicate is unsatisfied</typeparam>
        /// <typeparam name="TRight">The type of value we will get if the predicate is satisfied</typeparam>
        /// <param name="predicate">A hypothesis expressed as a boolean</param>
        /// <param name="consequent">The conclusion we can draw from the predicate holding (returns TRight)</param>
        /// <param name="alternative">The conclusion we can draw from the predicate failing to hold (returns TLeft \/ TRight)</param>
        /// <returns>TLeft \/ TRight</returns>
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

        /// <summary>
        /// Functional implementation of the 'using' keyword from C#.
        /// </summary>
        /// <param name="source">An 'IDisposable' that will be disposed after body is invoked</param>
        /// <param name="body">Statements to execute before disposing of source</param>
        /// <returns>A value that represents an effectual computation that is only useful for its side-effects</returns>
        public static Io<Unit> Using<T>(Func<T> source, Action<T> body) where T : IDisposable
        {
            return Io.Apply(() =>
            {
                using (var resource = source())
                {
                    body(resource);
                }

                return Unit.Only;
            });
        }

        /// <summary>
        /// A replacement for the 'as' keyword that is a total function. Rather than returning null on a failed cast, this
        /// will return a value indicating that we may or may not have been able to compute a result.
        /// </summary>
        /// <typeparam name="T">The type of value we think 'o' is</typeparam>
        /// <param name="o">An object we intend to attempt to prove is a T</param>
        /// <returns>Potentially a T (or nothing if we cannot prove at runtime that 'o' is a T)</returns>
        public static IMaybe<T> Cast<T>(this object o) where T : class
        {
            return (o as T).ToMaybe();
        }

        /// <summary>
        /// A replacement for a 'hard' cast for value types that is a total function. Rather than throwing an expection on
        /// a failed cast, this will return a value indicating that we may or may not have been able to compute a result.
        /// </summary>
        /// <typeparam name="T">The type of value we think 'o' is</typeparam>
        /// <param name="o">An object we intend to attempt to prove is a T</param>
        /// <returns>Potentially a T (or nothing if we cannot prove at runtime that 'o' is a T)</returns>
        public static IMaybe<T> CastV<T>(this object o) where T : struct
        {
            return Try.Attempt(() => (T) o).AsMaybe();
        }

        /// <summary>
        /// Convenience function for deciding to evaluate an io or return an io maybe unit.
        /// </summary>
        /// <param name="predicate">A hypothesis expressed as a boolean</param>
        /// <param name="io">Io</param>
        /// <returns>If predicate evaluates to true then the io is applied or an io is applied on maybe a unit.</returns>
        public static Io<IMaybe<Unit>> IfTrue(bool predicate, Io<IMaybe<Unit>> io)
        {
            return predicate ? io : Io.Apply(() => Unit.Only.ToMaybe());
        }

        /// <summary>
        /// Convenience function for deciding to evaluate an io or return an io unit.
        /// </summary>
        /// <param name="predicate">A hypothesis expressed as a boolean</param>
        /// <param name="io">Io</param>
        /// <returns>If predicate evaluates to true then the io is applied or an io is applied on a unit.</returns>
        public static Io<Unit> IfTrue(bool predicate, Io<Unit> io)
        {
            return predicate ? io : Io.Apply(() => Unit.Only);
        } 

        #region Const
        /// <summary>
        /// Ignores its input to return the supplied constant instead. 
        /// </summary>
        /// <typeparam name="T1">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T2">The type of values we wish to use instead </typeparam>
        /// <param name="t">A value for which any input value will yield</param>
        /// <returns>A function that will ignore its input and return the supplied constant</returns>
        public static Func<T1, T2> Const<T1, T2>(T2 t)
        {
            return ignored => t;
        }

        /// <summary>
        /// Ignores its inputs to return the supplied constant instead
        /// </summary>
        /// <typeparam name="T1">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T2">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T3">The type of values we wish to use instead</typeparam>
        /// <param name="t">A value for which any input values will yield</param>
        /// <returns>A function that will ignore its inputs and return the supplied constant</returns>
        public static Func<T1, T2, T3> Const<T1, T2, T3>(T3 t)
        {
            return (ignored1, ignored2) => t;
        }

        /// <summary>
        /// Ignores its inputs to return the supplied constant instead
        /// </summary>
        /// <typeparam name="T1">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T2">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T3">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T4">The type of values we wish to use instead</typeparam>
        /// <param name="t">A value for which any input values will yield</param>
        /// <returns>A function that will ignore its inputs and return the supplied constant</returns>
        public static Func<T1, T2, T3, T4> Const<T1, T2, T3, T4>(T4 t)
        {
            return (ignored1, ignored2, ignored3) => t;
        }

        /// <summary>
        /// Ignores its inputs to return the supplied constant instead
        /// </summary>
        /// <typeparam name="T1">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T2">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T3">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T4">The type of values we wish to ignore</typeparam>
        /// <typeparam name="T5">The type of values we wish to use instead</typeparam>
        /// <param name="t">A value for which any input values will yield</param>
        /// <returns>A function that will ignore its inputs and return the supplied constant</returns>
        public static Func<T1, T2, T3, T4, T5> Const<T1, T2, T3, T4, T5>(T5 t)
        {
            return (ignored1, ignored2, ignored3, ignored4) => t;
        }
        #endregion
    }
}
