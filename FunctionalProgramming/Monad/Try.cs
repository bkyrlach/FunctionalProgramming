using System;

namespace FunctionalProgramming.Monad
{
    public abstract class Try<A>
    {
        public abstract B Match<B>(Func<A, B> success, Func<Exception, B> failure);
    }

    public static class TryOps
    {
        private sealed class Success<A> : Try<A>
        {
            private readonly A _a;

            public Success(A a)
            {
                _a = a;
            }

            public override B Match<B>(Func<A, B> success, Func<Exception, B> failure)
            {
                return success(_a);
            }
        }

        private sealed class Failure<A> : Try<A>
        {
            private readonly Exception _ex;

            public Failure(Exception ex)
            {
                _ex = ex;
            }

            public override B Match<B>(Func<A, B> success, Func<Exception, B> failure)
            {
                return failure(_ex);
            }
        }

        public static Try<A> Attempt<A>(Func<A> supplier)
        {
            Try<A> result;
            try
            {
                result = new Success<A>(supplier());
            }
            catch (Exception ex)
            {
                result = new Failure<A>(ex);
            }
            return result;
        }

        public static Try<B> Select<A, B>(this Try<A> m, Func<A, B> f)
        {
            return m.Match(
                success: a => Attempt(() => f(a)),
                failure: ex => new Failure<B>(ex));
        }

        public static Try<B> SelectMany<A, B>(this Try<A> m, Func<A, Try<B>> f)
        {
            return m.Match(
                success: f,
                failure: ex => new Failure<B>(ex));
        }
    }
}
