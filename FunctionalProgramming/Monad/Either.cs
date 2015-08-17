using System;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public interface IEither<out TLeft, out TRight>
    {
        bool IsRight { get; }
        TMatch Match<TMatch>(Func<TLeft, TMatch> left, Func<TRight, TMatch> right);
    }

    public abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        public bool IsRight { get { return this is Right<TLeft, TRight>; } }
        public TMatch Match<TMatch>(Func<TLeft, TMatch> left, Func<TRight, TMatch> right)
        {
            TMatch retval = default(TMatch);
            if (this is Left<TLeft, TRight>)
            {
                var temp = this as Left<TLeft, TRight>;
                retval = left(temp.Value);
            }
            else if (this is Right<TLeft, TRight>)
            {
                var temp = this as Right<TLeft, TRight>;
                retval = right(temp.Value);
            }
            else
            {
                throw new MatchException(typeof(Either<TLeft, TRight>), GetType());
            }
            return retval;
        }
    }

    public sealed class Left<TLeft, TRight> : Either<TLeft, TRight>
    {
        public readonly TLeft Value;
        
        public Left(TLeft value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("-\\{0}", Value);
        }
    }

    public sealed class Right<TLeft, TRight> : Either<TLeft, TRight>
    {
        public readonly TRight Value;

        public Right(TRight value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("/-{0}", Value);
        }
    }

    public static class Either
    {
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

        public static IEither<T3, T2> SelectLeft<T1, T2, T3>(this IEither<T1, T2> m, Func<T1, T3> f)
        {
            return m.Match(
                left: l => f(l).AsLeft<T3, T2>(),
                right: r => r.AsRight<T3, T2>());
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
            return m.SelectMany(a => f(a).SelectMany(b => selector(a, b).AsRight<T1, T4>()));
        }

        public static IEither<T1, T2> Or<T1, T2>(this IEither<T1, T2> left, IEither<T1, T2> right)
        {
            return left.Match(
                right: val => val.AsRight<T1, T2>(),
                left: _ => right.Match(
                    left: err => err.AsLeft<T1, T2>(),
                    right: val => val.AsRight<T1, T2>()));
        }

        public static IEither<T1, TLeft> CombineTakeLeft<T1, TLeft, TRight>(this IEither<T1, TLeft> left, IEither<T1, TRight> right)
        {
            return
                from leftVal in left
                from rightVal in right
                select leftVal;
        }

        public static IEither<T1, TRight> CombineTakeRight<T1, TLeft, TRight>(this IEither<T1, TLeft> left, IEither<T1, TRight> right)
        {
            return
                from leftval in left
                from rightVal in right
                select rightVal;
        } 

        #region BuildApplicative
        public static IEither<TErr, Tuple<T1, T2>> BuildApplicative<TErr, T1, T2>(this IEither<TErr, T1> e1,
            IEither<TErr, T2> e2, IMonoid<TErr> mo)
        {
            return e1.Match(
                left: err1 => e2.Match(
                    left: err2 => mo.MAppend(err1, err2).AsLeft<TErr, Tuple<T1, T2>>(),
                    right: t2 => err1.AsLeft<TErr, Tuple<T1, T2>>()),
                right: t1 => e2.Match(
                    left: err2 => err2.AsLeft<TErr, Tuple<T1, T2>>(),
                    right: t2 => Tuple.Create(t1, t2).AsRight<TErr, Tuple<T1, T2>>()));
        }

        public static IEither<TErr, Tuple<T1, T2, T3>> BuildApplicative<TErr, T1, T2, T3>(this IEither<TErr, Tuple<T1, T2>> e1,
    IEither<TErr, T3> e2, IMonoid<TErr> mo)
        {
            return e1.Match(
                left: err1 => e2.Match(
                    left: err2 => mo.MAppend(err1, err2).AsLeft<TErr, Tuple<T1, T2, T3>>(),
                    right: t2 => err1.AsLeft<TErr, Tuple<T1, T2, T3>>()),
                right: t1 => e2.Match(
                    left: err2 => err2.AsLeft<TErr, Tuple<T1, T2, T3>>(),
                    right: t2 => Tuple.Create(t1.Item1, t1.Item2, t2).AsRight<TErr, Tuple<T1, T2, T3>>()));
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4>> BuildApplicative<TErr, T1, T2, T3, T4>(this IEither<TErr, Tuple<T1, T2, T3>> e1,
    IEither<TErr, T4> e2, IMonoid<TErr> mo)
        {
            return e1.Match(
                left: err1 => e2.Match(
                    left: err2 => mo.MAppend(err1, err2).AsLeft<TErr, Tuple<T1, T2, T3, T4>>(),
                    right: t2 => err1.AsLeft<TErr, Tuple<T1, T2, T3, T4>>()),
                right: t1 => e2.Match(
                    left: err2 => err2.AsLeft<TErr, Tuple<T1, T2, T3, T4>>(),
                    right: t2 => Tuple.Create(t1.Item1, t1.Item2, t1.Item3, t2).AsRight<TErr, Tuple<T1, T2, T3, T4>>()));
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4, T5>> BuildApplicative<TErr, T1, T2, T3, T4, T5>(this IEither<TErr, Tuple<T1, T2, T3, T4>> e1,
    IEither<TErr, T5> e2, IMonoid<TErr> mo)
        {
            return e1.Match(
                left: err1 => e2.Match(
                    left: err2 => mo.MAppend(err1, err2).AsLeft<TErr, Tuple<T1, T2, T3, T4, T5>>(),
                    right: t2 => err1.AsLeft<TErr, Tuple<T1, T2, T3, T4, T5>>()),
                right: t1 => e2.Match(
                    left: err2 => err2.AsLeft<TErr, Tuple<T1, T2, T3, T4, T5>>(),
                    right: t2 => Tuple.Create(t1.Item1, t1.Item2, t1.Item3, t1.Item4, t2).AsRight<TErr, Tuple<T1, T2, T3, T4, T5>>()));
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6>> BuildApplicative<TErr, T1, T2, T3, T4, T5, T6>(this IEither<TErr, Tuple<T1, T2, T3, T4, T5>> e1,
    IEither<TErr, T6> e2, IMonoid<TErr> mo)
        {
            return e1.Match(
                left: err1 => e2.Match(
                    left: err2 => mo.MAppend(err1, err2).AsLeft<TErr, Tuple<T1, T2, T3, T4, T5, T6>>(),
                    right: t2 => err1.AsLeft<TErr, Tuple<T1, T2, T3, T4, T5, T6>>()),
                right: t1 => e2.Match(
                    left: err2 => err2.AsLeft<TErr, Tuple<T1, T2, T3, T4, T5, T6>>(),
                    right: t2 => Tuple.Create(t1.Item1, t1.Item2, t1.Item3, t1.Item4, t1.Item5, t2).AsRight<TErr, Tuple<T1, T2, T3, T4, T5, T6>>()));
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6, T7>> BuildApplicative<TErr, T1, T2, T3, T4, T5, T6, T7>(this IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6>> e1,
    IEither<TErr, T7> e2, IMonoid<TErr> mo)
        {
            return e1.Match(
                left: err1 => e2.Match(
                    left: err2 => mo.MAppend(err1, err2).AsLeft<TErr, Tuple<T1, T2, T3, T4, T5, T6, T7>>(),
                    right: t2 => err1.AsLeft<TErr, Tuple<T1, T2, T3, T4, T5, T6, T7>>()),
                right: t1 => e2.Match(
                    left: err2 => err2.AsLeft<TErr, Tuple<T1, T2, T3, T4, T5, T6, T7>>(),
                    right: t2 => Tuple.Create(t1.Item1, t1.Item2, t1.Item3, t1.Item4, t1.Item5, t1.Item6, t2).AsRight<TErr, Tuple<T1, T2, T3, T4, T5, T6, T7>>()));
        }
        #endregion
    }
}
