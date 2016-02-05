using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;

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

        public static Validation<TFailure, TResult> SelectMany<TFailure, TSuccess, TResult>(
            this Validation<TFailure, TSuccess> m, Func<TSuccess, Validation<TFailure, TResult>> f)
        {
            return m.Match(
                success: val => f(val),
                failure: err => new Failure<TFailure, TResult>(err));
        }

        public static Validation<TFailure, TSelect> SelectMany<TFailure, TSuccess, TResult, TSelect>(
            this Validation<TFailure, TSuccess> m, Func<TSuccess, Validation<TFailure, TResult>> f,
            Func<TSuccess, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => new Success<TFailure, TSelect>(selector(a, b))));
        }

        public static Validation<TFailure, TResult> Apply<TFailure, TSuccess, TResult>(
            this Validation<TFailure, Func<TSuccess, TResult>> fa, Validation<TFailure, TSuccess> ma, IMonoid<TFailure> m)
        {
            return fa.Match(
                success: ma.Select,
                failure: e1 => ma.Match(
                    failure: e2 => new Failure<TFailure, TResult>(m.MAppend(e1, e2)),
                    success: _ => new Failure<TFailure, TResult>(e1)));
        }

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

            public override string ToString()
            {
                return string.Format("Success({0})", Value);
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

            public override string ToString()
            {
                return string.Format("Failure({0})", Value);
            }
        }
    }
}
