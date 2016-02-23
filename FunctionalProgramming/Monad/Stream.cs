using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Outlaws;
using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Monad
{
    public interface IStream<out T>
    {
        IMaybe<T> Head { get; }
        IMaybe<IStream<T>> Tail { get; }
        bool Any { get; }
        TResult Match<TResult>(Func<T, IStream<T>, TResult> cons, Func<TResult> nil);
    }

    public static class Stream
    {
        public static IStream<T> ToStream<T>(this IEnumerable<T> xs)
        {
            var retval = Empty<T>();
            var input = xs.Reverse().ToList();
            return input.Aggregate(retval, (current, x) => x.Cons(() => current));  
        }

        public static List<T> ToList<T>(this IStream<T> xs)
        {
            var retval = new List<T>();
            xs.ForEach(retval.Add);
            return retval;
        }

        private static ITrampoline<Unit> ForEachT<T>(IStream<T> xs, Action<T> f)
        {
            return xs.Match<ITrampoline<Unit>>(
                cons: (h, t) => new More<Unit>(() =>
                {
                    f(h);
                    return ForEachT(t, f);
                }),
                nil: () => new Done<Unit>(Unit.Only));
        } 

        public static Unit ForEach<T>(this IStream<T> xs, Action<T> f)
        {
            return ForEachT(xs, f).Run();
        }

        public static IStream<T> Cons<T>(this T head, Func<IStream<T>> tail)
        {
            return new NonEmptyStream<T>(head, new Lazy<IStream<T>>(tail));
        }

        public static IStream<T> LiftStream<T>(this T t)
        {
            return t.Cons(Empty<T>);
        }

        public static IStream<T> Empty<T>()
        {
            return new EmptyStream<T>();
        }

        public static IStream<T> Take<T>(this IStream<T> xs, int n)
        {
            return n <= 0
                ? Empty<T>()
                : xs.Match(
                    cons: (h, t) => h.Cons(() => t.Take(n - 1)),
                    nil: Empty<T>);
        }

        public static IStream<T> Drop<T>(this IStream<T> xs, int n)
        {
            return n <= 0
                ? xs
                : xs.Tail.Select(ys => ys.Drop(n - 1)).GetOrElse(Empty<T>);
        }

        public static Tuple<IStream<T>, IStream<T>> SplitAt<T>(this IStream<T> xs, int n)
        {
            return Tuple.Create(xs.Take(n), xs.Drop(n));
        }

        public static IStream<T> Repeat<T>(this T t)
        {
            return t.Cons(() => Repeat(t));
        }

        public static IStream<Tuple<T1, T2>> ZipWith<T1, T2>(this IStream<T1> ts, IStream<T2> vs)
        {
            return ts.Match(
                nil: Empty<Tuple<T1, T2>>,
                cons: (x, xs) => vs.Match(
                    nil: Empty<Tuple<T1, T2>>,
                    cons: (y, ys) => Tuple.Create(x, y).Cons(() => xs.ZipWith(ys))));
        } 

        public static TResult FoldL<TInput, TResult>(this IStream<TInput> xs, TResult defaultValue,
            Func<TResult, TInput, TResult> f)
        {
            return xs.Match(
                cons: (h, t) => t.FoldL(f(defaultValue, h), f),
                nil: () => defaultValue);
        }

        public static string MkString(this IStream<char> xs)
        {
            return xs.FoldL(string.Empty, (str, c) => str + c.ToString());
        }

        public static IStream<TResult> Select<TInput, TResult>(this IStream<TInput> m, Func<TInput, TResult> f)
        {
            return m.Match(
                cons: (h, t) => f(h).Cons(() => t.Select(f)),
                nil: Empty<TResult>);
        }

        private class NonEmptyStream<T> : IStream<T>
        {
            private readonly T _head;
            private readonly Lazy<IStream<T>> _tail;

            public IMaybe<T> Head => _head.ToMaybe();

            public IMaybe<IStream<T>> Tail => _tail.Value.ToMaybe();

            public bool Any => true;

            public NonEmptyStream(T head, Lazy<IStream<T>> tail)
            {
                _head = head;
                _tail = tail;
            }

            public TResult Match<TResult>(Func<T, IStream<T>, TResult> cons, Func<TResult> nil)
            {
                return cons(_head, _tail.Value);
            }

            public override string ToString()
            {
                return $"Stream({_head}, ?)";
            }
        }

        private class EmptyStream<T> : IStream<T>
        {
            public IMaybe<T> Head => Maybe.Nothing<T>();
            public IMaybe<IStream<T>> Tail => Maybe.Nothing<IStream<T>>();
            public bool Any => false;

            public TResult Match<TResult>(Func<T, IStream<T>, TResult> cons, Func<TResult> nil)
            {
                return nil();
            }

            public override string ToString()
            {
                return "Stream()";
            }
        }
    }
}
