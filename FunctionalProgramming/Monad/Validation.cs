using System;
using System.Collections.Generic;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public abstract class Validation<TFailure, TSuccess>
    {
        public abstract bool IsSuccess { get; }
        public abstract T Match<T>(Func<TSuccess, T> success, Func<TFailure, T> failure);
    }

    public static class Validation
    {
        public static Validation<TFailure, TSuccess> AsFailure<TFailure, TSuccess>(this TFailure error)
        {
            return new Failure<TFailure, TSuccess>(error);
        }

        public static Validation<IEnumerable<TFailure>, TSuccess> AsFailureList<TFailure, TSuccess>(this TFailure error)
        {
            return new Failure<IEnumerable<TFailure>, TSuccess>(error.LiftEnumerable());
        }

        public static Validation<TFailure, TSuccess> AsSuccess<TFailure, TSuccess>(this TSuccess val)
        {
            return new Success<TFailure, TSuccess>(val);
        }

        public static Validation<IEnumerable<TFailure>, TSuccess> AsSuccessWithFailureList<TFailure, TSuccess>(this TSuccess val)
        {
            return new Success<IEnumerable<TFailure>, TSuccess>(val);
        }

        public static Validation<TFailure2, TSuccess2> SelectEither<TFailure1, TFailure2, TSuccess1, TSuccess2>(
            this Validation<TFailure1, TSuccess1> v, Func<TFailure1, TFailure2> failureFunc,
            Func<TSuccess1, TSuccess2> successFunc)
        {
            return v.Match(
                success: s => successFunc(s).AsSuccess<TFailure2, TSuccess2>(),
                failure: f => failureFunc(f).AsFailure<TFailure2, TSuccess2>());
        }

        public static Validation<TFailure, TResult> Select<TFailure, TSuccess, TResult>(
            this Validation<TFailure, TSuccess> m, Func<TSuccess, TResult> f)
        {
            return m.Match<Validation<TFailure, TResult>>(
                success: val => new Success<TFailure, TResult>(f(val)),
                failure: err => new Failure<TFailure, TResult>(err));
        }

        #region BuildApplicative
        public static Validation<TFailure, Tuple<TSuccess1, TSuccess2>> BuildApplicative<TFailure, TSuccess1, TSuccess2>(
            this Validation<TFailure, TSuccess1> v1, Validation<TFailure, TSuccess2> v2, IMonoid<TFailure> m)
        {
            return v1.Match(
                success: val1 => v2.Match<Validation<TFailure, Tuple<TSuccess1, TSuccess2>>>(
                    success: val2 => new Success<TFailure, Tuple<TSuccess1, TSuccess2>>(Tuple.Create(val1, val2)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2>>(m.MAppend(err2, m.MZero))),
                failure: err1 => v2.Match(
                    success: val2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2>>(m.MAppend(err1, m.MZero)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2>>(m.MAppend(err1, err2))));
        }

        public static Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>> BuildApplicative<TFailure, TSuccess1, TSuccess2, TSuccess3>(
            this Validation<TFailure, Tuple<TSuccess1, TSuccess2>> v1, Validation<TFailure, TSuccess3> v2, IMonoid<TFailure> m)
        {
            return v1.Match(
                success: val1 => v2.Match<Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>>>(
                    success: val2 => new Success<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>>(Tuple.Create(val1.Item1, val1.Item2, val2)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>>(m.MAppend(err2, m.MZero))),
                failure: err1 => v2.Match(
                    success: val2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>>(m.MAppend(err1, m.MZero)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>>(m.MAppend(err1, err2))));
        }

        public static Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4>> BuildApplicative<TFailure, TSuccess1, TSuccess2, TSuccess3, TSuccess4>(
            this Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>> v1, Validation<TFailure, TSuccess4> v2, IMonoid<TFailure> m)
        {
            return v1.Match(
                success: val1 => v2.Match<Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4>>>(
                    success: val2 => new Success<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4>>(Tuple.Create(val1.Item1, val1.Item2, val1.Item3, val2)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4>>(m.MAppend(err2, m.MZero))),
                failure: err1 => v2.Match(
                    success: val2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4>>(m.MAppend(err1, m.MZero)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4>>(m.MAppend(err1, err2))));
        }

        public static Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>> BuildApplicative<TFailure, TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>(
            this Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4>> v1, Validation<TFailure, TSuccess5> v2, IMonoid<TFailure> m)
        {
            return v1.Match(
                success: val1 => v2.Match<Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>>>(
                    success: val2 => new Success<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>>(Tuple.Create(val1.Item1, val1.Item2, val1.Item3, val1.Item4, val2)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>>(m.MAppend(err2, m.MZero))),
                failure: err1 => v2.Match(
                    success: val2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>>(m.MAppend(err1, m.MZero)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>>(m.MAppend(err1, err2))));
        }

        public static Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>> BuildApplicative<TFailure, TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>(
            this Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5>> v1, Validation<TFailure, TSuccess6> v2, IMonoid<TFailure> m)
        {
            return v1.Match(
                success: val1 => v2.Match<Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>>>(
                    success: val2 => new Success<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>>(Tuple.Create(val1.Item1, val1.Item2, val1.Item3, val1.Item4, val1.Item5, val2)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>>(m.MAppend(err2, m.MZero))),
                failure: err1 => v2.Match(
                    success: val2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>>(m.MAppend(err1, m.MZero)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>>(m.MAppend(err1, err2))));
        }

        public static Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6, TSuccess7>> BuildApplicative<TFailure, TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6, TSuccess7>(
            this Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6>> v1, Validation<TFailure, TSuccess7> v2, IMonoid<TFailure> m)
        {
            return v1.Match(
                success: val1 => v2.Match<Validation<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6, TSuccess7>>>(
                    success: val2 => new Success<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6, TSuccess7>>(Tuple.Create(val1.Item1, val1.Item2, val1.Item3, val1.Item4, val1.Item5, val1.Item6, val2)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6, TSuccess7>>(m.MAppend(err2, m.MZero))),
                failure: err1 => v2.Match(
                    success: val2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6, TSuccess7>>(m.MAppend(err1, m.MZero)),
                    failure: err2 => new Failure<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3, TSuccess4, TSuccess5, TSuccess6, TSuccess7>>(m.MAppend(err1, err2))));
        }
        #endregion

        private class Success<TFailure, TSuccess> : Validation<TFailure, TSuccess>
        {
            private readonly TSuccess _val;

            public override bool IsSuccess { get { return true; } }
            public TSuccess Value { get { return _val; } }

            public Success(TSuccess val)
            {
                _val = val;
            }

            public override T Match<T>(Func<TSuccess, T> success, Func<TFailure, T> failure)
            {
                return success(_val);
            }
        }

        private class Failure<TFailure, TSuccess> : Validation<TFailure, TSuccess>
        {
            private readonly TFailure _err;

            public override bool IsSuccess { get { return false; } }
            public TFailure Value { get { return _err; } }

            public Failure(TFailure err)
            {
                _err = err;
            }

            public override T Match<T>(Func<TSuccess, T> success, Func<TFailure, T> failure)
            {
                return failure(_err);
            }
        }
    }
}
