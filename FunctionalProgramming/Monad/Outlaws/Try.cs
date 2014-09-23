using System;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Outlaws
{
    public abstract class Try<T>
    {
        public abstract TResult Match<TResult>(Func<T, TResult> success, Func<Exception, TResult> failure);
    }

    public static class TryOps
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
            return m.SelectMany(a => Attempt(() => f(a)));
        }

        public static Try<TResult> SelectMany<TInitial, TResult>(this Try<TInitial> m, Func<TInitial, Try<TResult>> f)
        {
            return m.Match(
                success: f,
                failure: ex => new Failure<TResult>(ex));
        }

        public static Try<TSelect> SelectMany<TInitial, TResult, TSelect>(this Try<TInitial> m,
            Func<TInitial, Try<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => Attempt(() => selector(a, b))));
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
