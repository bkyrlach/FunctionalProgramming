using System;
using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Monad
{
    public interface IEither<out T1, out T2>
    {
        bool IsRight { get; }
    }

    public static class EitherExtensions
    {
        public static T3 Match<T1, T2, T3>(this IEither<T1, T2> e, Func<T1, T3> left, Func<T2, T3> right)
        {
            return BF.If(e.IsRight,
                () => right((e as Right<T1, T2>).Value),
                () => left((e as Left<T1, T2>).Value));
        }

        private sealed class Left<T1, T2> : IEither<T1, T2>
        {
            public T1 Value { get; private set; }

            public bool IsRight { get; private set; }

            public Left(T1 value)
            {
                IsRight = false;
                Value = value;
            }

        }

        private sealed class Right<T1, T2> : IEither<T1, T2>
        {
            public T2 Value { get; private set; }
            public bool IsRight { get; private set; }

            public Right(T2 value)
            {
                IsRight = true;
                Value = value;
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

        public static IEither<T1, T3> Select<T1, T2, T3>(this IEither<T1, T2> m, Func<T2, T3> f)
        {
            return m.Match(
                left: l => l.AsLeft<T1, T3>(),
                right: r => f(r).AsRight<T1, T3>());
        }

        public static IEither<T1, T3> SelectMany<T1, T2, T3>(this IEither<T1, T2> m, Func<T2, IEither<T1, T3>> f)
        {
            return m.Match(
                left: l => l.AsLeft<T1, T3>(),
                right: f);
        }

        public static IEither<T1, T4> SelectMany<T1, T2, T3, T4>(this IEither<T1, T2> m, Func<T2, IEither<T1, T3>> f,
            Func<T2, T3, T4> select)
        {
            return m.Match(
                left: l => l.AsLeft<T1, T4>(),
                right: a => f(a).Match(
                    left: l => l.AsLeft<T1, T4>(),
                    right: b => select(a, b).AsRight<T1, T4>()));
        }
    }
}
