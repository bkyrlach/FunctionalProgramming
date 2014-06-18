using System;

using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Monad
{
    public abstract class Try<T>
    {
        public abstract TResult Match<TResult>(Func<T, TResult> success, Func<Exception, TResult> failure);
    }

    public static class TryExtensions
    {
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

        public static Try<TResult> Select<TInitial, TResult>(this Try<TInitial> m, Func<TInitial, TResult> f)
        {
            return m.Match(
                success: a => Attempt(() => f(a)),
                failure: ex => new Failure<TResult>(ex));
        }

        public static Try<TResult> SelectMany<TInitial, TResult>(this Try<TInitial> m, Func<TInitial, Try<TResult>> f)
        {
            return m.Match(
                success: f,
                failure: ex => new Failure<TResult>(ex));
        }

        public static T GetOrElse<T>(this Try<T> m, Func<T> defaultValue)
        {
            return m.Match(
                success: BF.Identity,
                failure: ex => defaultValue());
        }

        public static T GetOrError<T>(this Try<T> m)
        {
            return m.Match(
                success: BF.Identity,
                failure: ex => { throw ex; });
        }

        public static IMaybe<T> AsMaybe<T>(this Try<T> m)
        {
            return m.Match(
                success: val => val.ToMaybe(),
                failure: ex => MaybeExtensions.Nothing<T>());
        }
    }
}
