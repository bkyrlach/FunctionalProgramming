using FunctionalProgramming.Basics;
using System;
using FunctionalProgramming.Monad.Outlaws;
using FunctionalProgramming.Monad.Transformer;

namespace FunctionalProgramming.Monad
{
    /// <summary>
    /// A monadic encapsulation of effectful code that yields a value of type T
    /// </summary>
    /// <typeparam name="T">The type of value effectual code will yield at the end of the universe</typeparam>
    public sealed class Io<T>
    {
        /// <summary>
        /// This function represents an eventual lazy value resulting from effectual code.
        /// </summary>
        private readonly Func<T> _f;

        public Io(Func<T> f)
        {
            _f = f;
        }

        /// <summary>
        /// Method you call at the end of the universe to perform side-effects and yield an eventual value.
        /// </summary>
        /// <returns>The result of the effectual computation</returns>
        public T UnsafePerformIo()
        {
            return _f();
        }
    }

    /// <summary>
    /// Static class that contains all of the standard monadic operators for Io expressed in LINQ compatible terms.
    /// </summary>
    public static class Io
    {
        public static Io<T> Pure<T>(T t) where T : struct
        {
            return new Io<T>(() => t);
        }

        /// <summary>
        /// "Factory" function that takes a lazy value (represented by a lambda) that wraps effectual code which
        /// yields a value at the end of the universe.
        /// </summary>
        /// <param name="f">A function that represents a side-effect</param>
        /// <returns>Lifted T into IO</returns>
        public static Io<T> Apply<T>(Func<T> f)
        {
            return new Io<T>(f);
        }

        /// <summary>
        /// "Factory" function that wraps effectual code which does not yield a value 
        /// at the end of the universe.
        /// </summary>
        /// <param name="a">Effectual code that doesn't yield a value</param>
        /// <returns>Lifted Unit into IO</returns>
        public static Io<Unit> Apply(Action a)
        {
            return new Io<Unit>(() =>
            {
                a();
                return Unit.Only;
            });
        }

        public static Io<T> Join<T>(this Io<Io<T>> m)
        {
            return m.SelectMany(BasicFunctions.Identity);
        }

        public static Io<T2> Join<T1, T2>(this Io<T1> m, Io<T2> o)
        {
            return m.SelectMany(u => o);
        }

        /// <summary>
        /// Lifts (and applies) a morphism in the category universe to the category Io.
        /// </summary>
        /// <typeparam name="TValue">Domain of morphism f</typeparam>
        /// <typeparam name="TResult">Range of morphism f</typeparam>
        /// <param name="io">A TValue lifted to Io</param>
        /// <param name="f">A morphism TValue =&gt; TResult</param>
        /// <returns>The value in the range TResult lifted to Io</returns>
        public static Io<TResult> Select<TValue, TResult>(this Io<TValue> io, Func<TValue, TResult> f)
        {
            return Apply(() => f(io.UnsafePerformIo()));
        }

        /// <summary>
        /// Lifts (and applies) a morphism from universe to Io in the category universe to the category Io
        /// </summary>
        /// <typeparam name="TValue">Domain of the morphism f</typeparam>
        /// <typeparam name="TResult">Range of the morphism f lifted to Io</typeparam>
        /// <param name="io">A TValue lifted to Io</param>
        /// <param name="f">A morphism TValue =&gt; Io 'TResult</param>
        /// <returns>The value in the range TResult lifted to Io</returns>
        public static Io<TResult> SelectMany<TValue, TResult>(this Io<TValue> io, Func<TValue, Io<TResult>> f)
        {
            return Apply(() => f(io.UnsafePerformIo()).UnsafePerformIo());
        }

        public static Io<TSelect> SelectMany<TValue, TResult, TSelect>(this Io<TValue> m, Func<TValue, Io<TResult>> f,
            Func<TValue, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => Apply(() => selector(a, b))));
        }

        public static Io<T> LiftIo<T>(this T t)
        {
            return Apply(() => t);
        }

        /// <summary>
        /// Helper function that logs (and then discards) potential errors, returning a Maybe instead
        /// </summary>
        /// <typeparam name="T">The type of value we attempted to compute</typeparam>
        /// <param name="io">An effectual computation that might compute a value, or may fail</param>
        /// <param name="ifFailure">A function that handles errors in an effectual way</param>
        /// <returns>A value that represents an effectual computation that yields Either 'Just' a 'T, or 'Nothing' if the original computation yields a failure</returns>
        public static Io<IMaybe<T>> GetOrLog<T>(this Io<Try<T>> io, Func<Exception, Io<Unit>> ifFailure)
        {
            return io.SelectMany(t => t.Match(
                success: val => val.ToIoMaybe().Out,
                failure: ex => from _ in ifFailure(ex)
                               select Maybe.Nothing<T>()));
        }

        /// <summary>
        /// Helper function that, given an effectual computation that might fail, or might not yield a result, will
        /// log potential failures and potentially missing values, returning a Maybe instead
        /// </summary>
        /// <typeparam name="T">The type of value we attempted to compute</typeparam>
        /// <param name="io">An effectual computation that might compute a value, fail, or not compute a value</param>
        /// <param name="ifFailure">A function that handles errors in an effectual way</param>
        /// <param name="ifMissing">A function that handles the absence of a result in an effectual way</param>
        /// <returns>A value that represents an effectual computation that yields Either 'Just' a 'T, or 'Nothing' if the original computation yields a failure or no result</returns>
        public static Io<IMaybe<T>> GetOrLog<T>(this Io<Try<IMaybe<T>>> io, Func<Exception, Io<Unit>> ifFailure,
            Func<Io<Unit>> ifMissing)
        {
            return io.SelectMany(t => t.GetOrLog(ifFailure).SelectMany(m => m.Match(
                just: val => val.ToIoMaybe().Out,
                nothing: () => from _ in ifMissing()
                               select Maybe.Nothing<T>())));
        }
    }
}
