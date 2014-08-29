using System;

namespace FunctionalProgramming.Monad
{
    public interface IEither<out T1, out T2>
    {
        bool IsRight { get; }
        T3 Match<T3>(Func<T1, T3> left, Func<T2, T3> right);
    }

    public static class EitherExtensions
    {
        private sealed class Left<T1, T2> : IEither<T1, T2>
        {
            private readonly bool _isRight;
            public readonly T1 Value;
            public bool IsRight { get { return _isRight; } }

            public Left(T1 value)
            {
                _isRight = false;
                Value = value;
            }

            public T3 Match<T3>(Func<T1, T3> left, Func<T2, T3> right)
            {
                return left(Value);
            }
        }

        private sealed class Right<T1, T2> : IEither<T1, T2>
        {
            private readonly bool _isRight;
            public readonly T2 Value;            
            public bool IsRight { get { return _isRight; } }

            public Right(T2 value)
            {
                _isRight = true;
                Value = value;
            }

            public T3 Match<T3>(Func<T1, T3> left, Func<T2, T3> right)
            {
                return right(Value);
            }
        }

        public static IEither<T1, T2> AsLeft<T1, T2>(this T1 left)
        {
            return new Left<T1, T2>(left);
        }

        public static IEither<T1, T2> AsRight<T1, T2>(this T2 right)
        {
            return new Right<T1, T2>(right);
        }

        public static IEither<T2, T1> Swap<T1, T2>(this IEither<T1, T2> e)
        {
            return e.Match(
                left: l => l.AsRight<T2, T1>(),
                right: r => r.AsLeft<T2, T1>());
        }

        public static IEither<T1, T3> Select<T1, T2, T3>(this IEither<T1, T2> m, Func<T2, T3> f)
        {
            return m.Match(
                left: l => l.AsLeft<T1, T3>(),
                right: r => f(r).AsRight<T1, T3>());
        }

        public static IEither<T3, T4> SelectEither<T1, T2, T3, T4>(this IEither<T1, T2> m, Func<T1, T3> left,
            Func<T2, T4> right)
        {
            return m.Match(
                left: l => left(l).AsLeft<T3, T4>(),
                right: r => right(r).AsRight<T3, T4>());
        }

        public static IEither<T1, T3> SelectMany<T1, T2, T3>(this IEither<T1, T2> m, Func<T2, IEither<T1, T3>> f)
        {
            return m.Match(
                left: l => l.AsLeft<T1, T3>(),
                right: f);
        }

        public static IEither<T1, T4> SelectMany<T1, T2, T3, T4>(this IEither<T1, T2> m, Func<T2, IEither<T1, T3>> f,
            Func<T2, T3, T4> selector)
        {
            return from initial in m
                from result in f(initial)
                select selector(initial, result);
        }
    }
}
