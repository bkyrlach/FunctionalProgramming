using System;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Outlaws
{
    /// <summary>
    /// A computational context that represents a computation that can fail.
    /// 
    /// Note that this class should only ever have one implementation. Also, this is
    /// in the 'Outlaws' package because, although I've provided the monadic operators
    /// here, it doesn't always satisfy the monad laws.
    /// </summary>
    /// <typeparam name="T">The type of value the computation will yield if successful</typeparam>
    public abstract class Try<T>
    {
        /// <summary>
        /// ML style pattern matching for the 'Try' ADT. The 'success' lambda will be invoked if the 
        /// computation was successful, whereas the 'failure' lambda will be invoked if the computation
        /// was a failure
        /// </summary>
        /// <typeparam name="TResult">The type of value we're computing based off of the success or failure of this computation</typeparam>
        /// <param name="success">A computation to preform if this computation was successful (will have access to the computed value)</param>
        /// <param name="failure">A computation to perform if this computation was a failure (will have access to the exception thrown)</param>
        /// <returns>The value computed by the 'success' or 'failure' lambda, depending on if this computation was successful, or if it failed</returns>
        public abstract TResult Match<TResult>(Func<T, TResult> success, Func<Exception, TResult> failure);
    }

    /// <summary>
    /// Extension methods for 'Try' that implement the monadic operators and other helpful functions
    /// </summary>
    public static class Try
    {
        /// <summary>
        /// Represents a successful computation
        /// </summary>
        /// <typeparam name="T">The type of value that was successfully computed</typeparam>
        private sealed class Success<T> : Try<T>
        {
            private readonly T _a;

            public Success(T a)
            {
                _a = a;
            }

            public override TResult Match<TResult>(Func<T, TResult> success, Func<Exception, TResult> failure)
            {
                return success(_a);
            }
        }

        /// <summary>
        /// Represents a failed computation
        /// </summary>
        /// <typeparam name="T">The type of value that was attempted to be computed</typeparam>
        private sealed class Failure<T> : Try<T>
        {
            private readonly Exception _ex;

            public Failure(Exception ex)
            {
                _ex = ex;
            }

            public override TResult Match<TResult>(Func<T, TResult> success, Func<Exception, TResult> failure)
            {
                return failure(_ex);
            }
        }

        public static Try<T> Pure<T>(T t) where T : struct
        {
            return new Success<T>(t);
        }

        /// <summary>
        /// Attempts to execute the supplied lambda, handling failure in a type safe way by returning a value that forces you to 
        /// handle failure scenarios
        /// </summary>
        /// <typeparam name="T">The type of value that this computation will yield if successful</typeparam>
        /// <param name="supplier">A lambda that encapsulates code that may or may not throw an exception</param>
        /// <returns>A value that represents a potentially failed computation</returns>
        public static Try<T> Attempt<T>(Func<T> supplier)
        {
            Try<T> result;
            try
            {
                result = new Success<T>(supplier());
            }
            catch (Exception ex)
            {
                result = new Failure<T>(ex);
            }
            return result;
        }

        /// <summary>
        /// Attempts to execute the supplied side-effecting function, handling failure in a type safe way by returning a value
        /// that forces you to handle failure scenarios
        /// </summary>
        /// <param name="a">A side-effecting function</param>
        /// <returns>A value that represents a potentially failed computation</returns>
        public static Try<Unit> Attempt(Action a)
        {
            Try<Unit> result;
            try
            {
                a();
                result = new Success<Unit>(Unit.Only);
            }
            catch (Exception ex)
            {
                result = new Failure<Unit>(ex);
            }
            return result;
        }

        /// <summary>
        /// Helper that constructs a value representing a failed computation. Useful when you need to restore a previously
        /// discarded failure
        /// </summary>
        /// <typeparam name="T">The type of value that was attempted to be computed</typeparam>
        /// <param name="ex">The exception you wish to promote</param>
        /// <returns>A value that represents a failed computation with the supplied exception as the failure reason</returns>
        public static Try<T> Fail<T>(this Exception ex)
        {
            return new Failure<T>(ex);
        }

        /// <summary>
        /// Lifts the function 'TInitial -> 'TResult from the category C# to the category 'Try', and then applies it to the value 'm'
        /// </summary>
        /// <typeparam name="TInitial"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="m">A value in the category 'Try to apply the lifted function 'f'</param>
        /// <param name="f">The function to lift to the category 'Try</param>
        /// <returns>The result of applying the lifted function 'f' to 'm'</returns>
        public static Try<TResult> Select<TInitial, TResult>(this Try<TInitial> m, Func<TInitial, TResult> f)
        {
            return m.SelectMany(a => Attempt(() => f(a)));
        }

        /// <summary>
        /// Lifts the function 'TInitial -> 'Try 'TResult from the category C# to the category 'Try', and then applies it to the value 'm'
        /// </summary>
        /// <typeparam name="TInitial"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="m">A value in the category 'Try to apply the lifted function 'f'</param>
        /// <param name="f">The function to lift to the category 'Try</param>
        /// <returns>The result of applying the lifted function 'f' to 'm'</returns>
        public static Try<TResult> SelectMany<TInitial, TResult>(this Try<TInitial> m, Func<TInitial, Try<TResult>> f)
        {
            return m.Match(
                success: f,
                failure: ex => new Failure<TResult>(ex));
        }

        /// <summary>
        /// Helper function to support LINQ Query syntax. Lifts the function 'TInitial -> 'Try 'TResult from the category C# to the category
        /// 'Try, and then applies it to the value 'm', additionally applying a selector function
        /// </summary>
        /// <typeparam name="TInitial"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TSelect"></typeparam>
        /// <param name="m">A value in the category 'Try to apply the lifted function 'f'</param>
        /// <param name="f">The function to lift to the category 'Try</param>
        /// <param name="selector">A function that computes a result from the initial state and the result of applying 'f'</param>
        /// <returns>The result of applying the lifted function 'f' to 'm', and then additionally applying 'selector' to the initial state and that result</returns>
        public static Try<TSelect> SelectMany<TInitial, TResult, TSelect>(this Try<TInitial> m,
            Func<TInitial, Try<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => Attempt(() => selector(a, b))));
        }

        /// <summary>
        /// Syntactic sugar for t >>= identity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Try<T> Join<T>(this Try<Try<T>> t)
        {
            return t.SelectMany(BasicFunctions.Identity);
        }

        public static T GetOrElse<T>(this Try<T> m, Func<T> defaultValue)
        {
            return m.Match(
                success: BasicFunctions.Identity,
                failure: ex => defaultValue());
        }

        public static T GetOrError<T>(this Try<T> m)
        {
            return m.Match(
                success: BasicFunctions.Identity,
                failure: ex => { throw ex; });
        }

        public static IMaybe<T> AsMaybe<T>(this Try<T> m)
        {
            return m.Match(
                success: val => val.ToMaybe(),
                failure: ex => Maybe.Nothing<T>());
        }

        public static IEither<Exception, T> AsEither<T>(this Try<T> m)
        {
            return m.Match(
                success: val => val.AsRight<Exception, T>(),
                failure: ex => ex.AsLeft<Exception, T>());
        }

        public static IEither<TErr, TVal> AsEither<TErr, TVal>(this Try<TVal> m, Func<Exception, TErr> errorMapping)
        {
            return m.Match(
                success: val => val.AsRight<TErr, TVal>(),
                failure: ex => errorMapping(ex).AsLeft<TErr, TVal>());
        }

        private static readonly Io<Unit> IoUnit = Io.Apply(() => Unit.Only);

        /// <summary>
        /// Helper function that, given an effectual computation that might compute a value, or might fail, will
        /// perform a side-effect if that computation fails
        /// </summary>
        /// <typeparam name="T">The type of value we are attempting to compute</typeparam>
        /// <param name="ioT">An effectual computation that might compute a value, or might fail</param>
        /// <param name="logger">A function that will perform a side effect if the computation fails</param>
        /// <returns>A value indicating an effectual computation that is only useful for the side-effects performed</returns>
        public static Io<Unit> LogFailure<T>(this Io<Try<T>> ioT, Func<Exception, Io<Unit>> logger)
        {
            return ioT.SelectMany(@try => @try.Match(
                success: _ => IoUnit,
                failure: logger));
        }

        /// <summary>
        /// Helper function that, given a failed computation, will perform a side effect
        /// </summary>
        /// <typeparam name="T">The type of value we are attempting to compute</typeparam>
        /// <param name="t">A value that might be a successful computation or a failure</param>
        /// <param name="logger">A function that performs a side effect given some failure</param>
        /// <returns>A value indicating an effectual computation that is only useful for the side-effects performed</returns>
        public static Io<Unit> LogFailure<T>(this Try<T> t, Func<Exception, Io<Unit>> logger)
        {
            return t.Match(
                success: _ => IoUnit,
                failure: logger);
        }

        /// <summary>
        /// Helper function that, given a potentially successful computation will perform one side-effect if the computation was
        /// successful, or another side-effect if the computation was a failure
        /// </summary>
        /// <typeparam name="T">The type of value we are attempting to compute</typeparam>
        /// <param name="t">A value that might be a successful computation or a failure</param>
        /// <param name="successLogger">A function that performs a side-effect given some value of type T</param>
        /// <param name="failureLogger">A function that performs a side-effect given an exception</param>
        /// <returns>A value indicating an effectual computation that is only useful for the side-effects performed</returns>
        public static Io<Unit> Log<T>(this Try<T> t, Func<T, Io<Unit>> successLogger,
            Func<Exception, Io<Unit>> failureLogger)
        {
            return t.Match(
                success: successLogger,
                failure: failureLogger);
        }

        /// <summary>
        /// Helper function that given a potentially successful computation that may or may not yield a reuslt, will yield the
        /// potential value if the computation was succesful, or perform a side-effect if the computation failed
        /// </summary>
        /// <typeparam name="T">The type of value we are attempting to compute</typeparam>
        /// <param name="t">A value that might be a successful computation or a failure, and may not yield a value</param>
        /// <param name="logger">A function that performs a side-effect given an exception</param>
        /// <returns>A value indicating an effectual computation that may or may not yield a result</returns>
        public static Io<IMaybe<T>> GetOrLog<T>(this Try<IMaybe<T>> t, Func<Exception, Io<Unit>> logger)
        {
            return t.Match(
                success: maybeVal => Io.Apply(() => maybeVal),
                failure: ex => from _ in logger(ex)
                               select Maybe.Nothing<T>());
        }

        /// <summary>
        /// Helper function that given a potentially successful computation, will maybe yield a value, performing a side 
        /// effect if the computation was a failure
        /// </summary>
        /// <typeparam name="T">The type of value we are attempting to compute</typeparam>
        /// <param name="t">A value that might be a successful computation or a failure</param>
        /// <param name="logger">A function that performs a side-effect given an exception</param>
        /// <returns>A value indicating an effectual computation that may or may not yield a result</returns>
        public static Io<IMaybe<T>> GetOrLog<T>(this Try<T> t, Func<Exception, Io<Unit>> logger)
        {
            return t.Match(
                success: val => Io.Apply(() => val.ToMaybe()),
                failure: ex => from _ in logger(ex)
                               select Maybe.Nothing<T>());
        }

        /// <summary>
        /// Helper function that, given a potentially successful computation and a reasonable default value, will yield the 
        /// computed value if successful, or yield the default value and perform a side-effect if a failure
        /// </summary>
        /// <typeparam name="T">The type of value we are attempting to compute</typeparam>
        /// <param name="t">A value that might be a successufl computation or a failure</param>
        /// <param name="logger">A function that performs a side-effect given an exception</param>
        /// <param name="defaultValue">A lazily evaluated default</param>
        /// <returns>A value indicating an effectual computation that will always yield a result</returns>
        public static Io<T> GetOrLog<T>(this Try<T> t, Func<Exception, Io<Unit>> logger, Func<T> defaultValue)
        {
            return t.Match(
                success: val => Io.Apply(() => val),
                failure: ex => from _ in logger(ex)
                               select defaultValue());
        }

        #region BuildApplicative
        public static Try<Tuple<T1,T2>> BuildApplicative<T1,T2>(this Try<T1> try1, Try<T2> try2)
        {
            return try1.SelectMany(t1 => try2.Select(t2 => Tuple.Create(t1, t2)));
        }

        public static Try<Tuple<T1,T2,T3>> BuildApplicative<T1,T2,T3>(this Try<Tuple<T1,T2>> try1, Try<T3> try2)
        {
            return try1.SelectMany(_2 => try2.Select(t3 => Tuple.Create(_2.Item1, _2.Item2, t3)));
        }

        public static Try<Tuple<T1,T2,T3,T4>> BuildApplicative<T1,T2,T3,T4>(this Try<Tuple<T1,T2,T3>> try1, Try<T4> try2)
        {
            return try1.SelectMany(_3 => try2.Select(t4 => Tuple.Create(_3.Item1, _3.Item2, _3.Item3, t4)));
        }

        public static Try<Tuple<T1,T2,T3,T4,T5>> BuildApplicative<T1,T2,T3,T4,T5>(this Try<Tuple<T1,T2,T3,T4>> try1, Try<T5> try2)
        {
            return try1.SelectMany(_4 => try2.Select(t5 => Tuple.Create(_4.Item1, _4.Item2, _4.Item3, _4.Item4, t5)));
        }

        public static Try<Tuple<T1,T2,T3,T4,T5,T6>> BuildApplicative<T1,T2,T3,T4,T5,T6>(this Try<Tuple<T1,T2,T3,T4,T5>> try1, Try<T6> try2)
        {
            return try1.SelectMany(_5 => try2.Select(t6 => Tuple.Create(_5.Item1, _5.Item2, _5.Item3, _5.Item4, _5.Item5, t6)));
        }

        public static Try<Tuple<T1,T2,T3,T4,T5,T6,T7>> BuildApplicative<T1,T2,T3,T4,T5,T6,T7>(this Try<Tuple<T1,T2,T3,T4,T5,T6>> try1, Try<T7> try2)
        {
            return try1.SelectMany(_6 => try2.Select(t7 => Tuple.Create(_6.Item1, _6.Item2, _6.Item3, _6.Item4, _6.Item5, _6.Item6, t7)));
        }

        #endregion
    }
}
