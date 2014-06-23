using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public interface IConsList<out T>
    {
        IMaybe<T> Head { get; }
        IMaybe<IConsList<T>> Tail { get; } 
        bool Any { get; }
        int Count { get; }

        TResult Match<TResult>(Func<T, IConsList<T>, TResult> cons, Func<TResult> nil);
    }
    
    public static class ConsListExtensions
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

        public static IConsList<TResult> Select<TInitial, TResult>(this IConsList<TInitial> xs,
            Func<TInitial, TResult> f)
        {
            return xs.Match(
                cons: (h, t) => f(h).Cons(t.Select(f)),
                nil: Nil<TResult>);
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

        public static IEnumerable<T> AsEnumerable<T>(this IConsList<T> xs)
        {
            return xs.Match(
                cons: (h, t) => h.LifEnumerable().Concat(t.AsEnumerable()),
                nil: Enumerable.Empty<T>);
        }

        public static TResult FoldL<TValue, TResult>(this IConsList<TValue> xs, TResult initial,
            Func<TResult, TValue, TResult> f)
        {
            return xs.Match(
                cons: (h, t) => FoldL(t, f(initial, h), f),
                nil: () => initial);
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

            public TResult Match<TResult>(Func<T, IConsList<T>, TResult> cons, Func<TResult> nil)
            {
                return cons(_head, _tail);
            }
        }

        private class EmptyList<T> : IConsList<T>
        {
            public EmptyList() { }

            public IMaybe<T> Head
            {
                get { return MaybeExtensions.Nothing<T>(); }
            }

            public IMaybe<IConsList<T>> Tail
            {
                get { return MaybeExtensions.Nothing<IConsList<T>>(); }
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
        }
    }
}
