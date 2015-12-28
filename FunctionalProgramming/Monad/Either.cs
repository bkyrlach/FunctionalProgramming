using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;

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

        public void UnsafeMatch(Action<TLeft> left, Action<TRight> right)
        {
            if (this is Left<TLeft, TRight>)
            {
                var temp = this as Left<TLeft, TRight>;
                left(temp.Value);
            }
            else if (this is Right<TLeft, TRight>)
            {
                var temp = this as Right<TLeft, TRight>;
                right(temp.Value);
            }
            else
            {
                throw new MatchException(typeof(Either<TLeft, TRight>), GetType());
            }
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
            return string.Format("Left({0})", Value);
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
            return string.Format("Right({0})", Value);
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

        public static IEither<TLeft, Unit> Unless<TLeft>(this TLeft failure, bool conditional)
        {
            return BasicFunctions.EIf(conditional, () => Unit.Only, () => failure);
        }

        public static IEnumerable<TRight> KeepRights<TLeft, TRight>(this IEnumerable<IEither<TLeft, TRight>> xs)
        {
            return xs.SelectMany(x => x.Match(
                left: l => Enumerable.Empty<TRight>(),
                right: r => r.LiftEnumerable()));
        }

        public static IEnumerable<TLeft> KeepLefts<TLeft, TRight>(this IEnumerable<IEither<TLeft, TRight>> xs)
        {
            return xs.SelectMany(x => x.Match(
                left: l => l.LiftEnumerable(),
                right: r => Enumerable.Empty<TLeft>()));
        } 

        #region ApplicativeStuff
        public static IEither<TErr, Tuple<T1, T2>> With<TErr, T1, T2>(this IEither<TErr, T1> e1,
            IEither<TErr, T2> e2)
        {
            return from t1 in e1
                   from t2 in e2
                   select Tuple.Create(t1, t2);
        }

        public static IEither<TErr, Tuple<T1, T2, T3>> With<TErr, T1, T2, T3>(this IEither<TErr, Tuple<T1, T2>> e1, IEither<TErr, T3> e2)
        {
            return from tuple in e1
                   from t3 in e2
                   select Tuple.Create(tuple.Item1, tuple.Item2, t3);
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4>> With<TErr, T1, T2, T3, T4>(this IEither<TErr, Tuple<T1, T2, T3>> e1, IEither<TErr, T4> e2)
        {
            return from tuple in e1
                   from t4 in e2
                   select Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item3, t4);
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4, T5>> With<TErr, T1, T2, T3, T4, T5>(this IEither<TErr, Tuple<T1, T2, T3, T4>> e1,
    IEither<TErr, T5> e2)
        {
            return from tuple in e1
                   from t5 in e2
                   select Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, t5);
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6>> With<TErr, T1, T2, T3, T4, T5, T6>(this IEither<TErr, Tuple<T1, T2, T3, T4, T5>> e1,
    IEither<TErr, T6> e2)
        {
            return from tuple in e1
                   from t6 in e2
                   select Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, t6);
        }

        public static IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6, T7>> With<TErr, T1, T2, T3, T4, T5, T6, T7>(this IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6>> e1,
    IEither<TErr, T7> e2)
        {
            return from tuple in e1
                   from t7 in e2
                   select Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, t7);
        }

        public static IEither<TErr, TResult> Apply<TErr, T1, T2, TResult>(this IEither<TErr, Tuple<T1, T2>> e,
            Func<T1, T2, TResult> f)
        {
            return from tuple in e
                   select tuple.Apply(f);
        }

        public static IEither<TErr, TResult> Apply<TErr, T1, T2, T3, TResult>(this IEither<TErr, Tuple<T1, T2, T3>> e,
            Func<T1, T2, T3, TResult> f)
        {
            return from tuple in e
                   select tuple.Apply(f);
        }

        public static IEither<TErr, TResult> Apply<TErr, T1, T2, T3, T4, TResult>(this IEither<TErr, Tuple<T1, T2, T3, T4>> e,
            Func<T1, T2, T3, T4, TResult> f)
        {
            return from tuple in e
                   select tuple.Apply(f);
        }

        public static IEither<TErr, TResult> Apply<TErr, T1, T2, T3, T4, T5, TResult>(this IEither<TErr, Tuple<T1, T2, T3, T4, T5>> e,
            Func<T1, T2, T3, T4, T5, TResult> f)
        {
            return from tuple in e
                   select tuple.Apply(f);
        }

        public static IEither<TErr, TResult> Apply<TErr, T1, T2, T3, T4, T5, T6, TResult>(this IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6>> e,
            Func<T1, T2, T3, T4, T5, T6, TResult> f)
        {
            return from tuple in e
                   select tuple.Apply(f);
        }

        public static IEither<TErr, TResult> Apply<TErr, T1, T2, T3, T4, T5, T6, T7, TResult>(this IEither<TErr, Tuple<T1, T2, T3, T4, T5, T6, T7>> e,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> f)
        {
            return from tuple in e
                   select tuple.Apply(f);
        }
        #endregion
    }
}
