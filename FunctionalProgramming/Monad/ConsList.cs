using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionalProgramming.Monad
{
    /// <summary>
    /// An immutable, singly-linked, structural sharing cons list
    /// </summary>
    /// <typeparam name="T">The type of elements in this sequence</typeparam>
    public interface IConsList<out T>
    {
        /// <summary>
        /// A property that gives you the head of the list, or Nothing if the list is empty
        /// </summary>
        IMaybe<T> Head { get; }

        /// <summary>
        /// 
        /// </summary>
        IMaybe<IConsList<T>> Tail { get; }

        /// <summary>
        /// Indicates if the sequence contains any elements
        /// </summary>
        bool Any { get; }
        int Count { get; }

        TResult Match<TResult>(Func<T, IConsList<T>, TResult> cons, Func<TResult> nil);

        IEnumerable<T> AsEnumerable();
    }

    public static class ConsList
    {
        public static IConsList<T> Cons<T>(this T t, IConsList<T> xs)
        {
            return new NonEmptyList<T>(t, xs);
        }

        public static IConsList<T> Nil<T>()
        {
            return new EmptyList<T>();
        }

        public static IConsList<T> LiftList<T>(this T t)
        {
            return t.Cons(Nil<T>());
        }

        public static IConsList<T> Concat<T>(this IConsList<T> xs, IConsList<T> ys)
        {
            return xs.Match(
                cons: (h, t) => h.Cons(t.Concat(ys)),
                nil: () => ys);
        }

        public static bool Contains<T>(this IConsList<T> xs, T x)
        {
            return xs.FoldL(false, (result, next) => result || next.Equals(x));
        }

        public static IConsList<T> Distinct<T>(this IConsList<T> xs)
        {
            return xs.FoldL(Nil<T>(), (result, next) => result.Contains(next) ? result : next.Cons(result)).Reverse();
        } 

        public static IStream<T> ToStream<T>(this IConsList<T> xs)
        {
            return xs.Match(
                nil: Stream.Empty<T>,
                cons: (h, t) => h.Cons(t.ToStream));
        }

        public static IConsList<TResult> Select<TInitial, TResult>(this IConsList<TInitial> xs,
            Func<TInitial, TResult> f)
        {
            return SelectTrampoline(xs, f).Run();
        }

        private static Trampoline<IConsList<T2>> SelectTrampoline<T1, T2>(this IConsList<T1> xs, Func<T1, T2> f)
        {
            return xs.Match(
                cons: (h, t) => new More<IConsList<T2>>(() => t.SelectTrampoline(f)).Select(ts => f(h).Cons(ts)),
                nil: () => new Done<IConsList<T2>>(Nil<T2>()));
        }

        public static IConsList<TResult> SelectMany<TInitial, TResult>(this IConsList<TInitial> xs,
            Func<TInitial, IConsList<TResult>> f)
        {
            return xs.Match(
                cons: (h, t) => f(h).Concat(t.SelectMany(f)),
                nil: Nil<TResult>);
        }

        public static IConsList<TSelect> SelectMany<TInitial, TResult, TSelect>(this IConsList<TInitial> xs,
            Func<TInitial, IConsList<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return xs.SelectMany(a => f(a).SelectMany(b => selector(a, b).LiftList()));
        }

        public static IConsList<T> Where<T>(this IConsList<T> xs, Func<T, bool> predicate)
        {
            return xs.FoldL(Nil<T>(), (results, next) => predicate(next) ? next.Cons(results) : results).Reverse();
        } 

        public static TResult FoldL<TValue, TResult>(this IConsList<TValue> xs, TResult initial,
            Func<TResult, TValue, TResult> f)
        {
            return xs.Match(
                cons: (h, t) => FoldL(t, f(initial, h), f),
                nil: () => initial);
        }

        public static IConsList<T> Reverse<T>(this IConsList<T> xs)
        {
            return xs.Match(
                cons: (h, t) => Reverse(t).Concat(h.LiftList()),
                nil: Nil<T>);
        }
        
        public static IConsList<T> ToConsList<T>(this IEnumerable<T> xs)
        {
            return xs.Reverse().Aggregate(Nil<T>(), (ts, t) => t.Cons(ts));
        }

        public static string MkString(this IConsList<char> chars)
        {
            return chars.FoldL(string.Empty, (str, c) => str + c.ToString());
        }

        private class NonEmptyList<T> : IConsList<T>
        {
            private readonly T _head;
            private readonly IConsList<T> _tail;

            public NonEmptyList(T head, IConsList<T> tail)
            {
                _head = head;
                _tail = tail;
            }

            public IMaybe<T> Head { get { return _head.ToMaybe(); } }

            public IMaybe<IConsList<T>> Tail { get { return _tail.ToMaybe(); } }

            public bool Any
            {
                get { return true; }
            }

            public int Count
            {
                get { return 1 + _tail.Count; }
            }

            public IEnumerable<T> AsEnumerable()
            {
                var next = _head;
                var tail = _tail;
                while (tail != null)
                {
                    yield return next;
                    next = tail.Head.Match(
                        h => h,
                        () => default(T));
                    tail = tail.Tail.Match(
                        t => t,
                        () => null);
                }
            }

            public TResult Match<TResult>(Func<T, IConsList<T>, TResult> cons, Func<TResult> nil)
            {
                return cons(_head, _tail);
            }

            public override bool Equals(object obj)
            {
                var retval = false;
                var list = obj as NonEmptyList<T>;
                if (list != null)
                {
                    var ol = list;
                    retval = Head.Equals(ol.Head) && Tail.Equals(ol.Tail);
                }
                return retval;
            }

            public override int GetHashCode()
            {
                return FoldL(this, 181, (hash, t) => (hash*503) + t.GetHashCode());
            }

            public override string ToString()
            {
                return AsEnumerable().Select(x => x.ToString()).Aggregate((str, s) => string.Format("{0},{1}", str, s));
            }
        }

        private class EmptyList<T> : IConsList<T>
        {
            public IMaybe<T> Head
            {
                get { return Maybe.Nothing<T>(); }
            }

            public IMaybe<IConsList<T>> Tail
            {
                get { return Maybe.Nothing<IConsList<T>>(); }
            }

            public bool Any
            {
                get { return false; }
            }

            public int Count
            {
                get { return 0; }
            }

            public TResult Match<TResult>(Func<T, IConsList<T>, TResult> cons, Func<TResult> nil)
            {
                return nil();
            }

            public IEnumerable<T> AsEnumerable()
            {
                return Enumerable.Empty<T>();
            }

            public override bool Equals(object obj)
            {
                return (obj is EmptyList<T>);
            }

            public override int GetHashCode()
            {
                return 1;
            }

            public override string ToString()
            {
                return "Nil";
            }
        }
    }
}
