using System;
using System.Collections.Generic;
using System.Linq;
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

    public static class StreamExtensions
    {
        public static IStream<T> AsStream<T>(this IEnumerable<T> xs)
        {
            var retval = Empty<T>();
            var input = xs.Reverse().ToList();
            foreach (var x in input)
            {
                retval = x.Cons(retval);
            }            
            return retval;  //xs.Reverse().Aggregate(, (stream, n) => n.Cons(stream));
            //xs.Reverse().ToList().ForEach(x => );
            //return input.Any()
            //    ? (IStream<T>) new NonEmptyStream<T>(input.First(), new Lazy<IStream<T>>(() => input.Skip(1).AsStream()))
            //    : new EmptyStream<T>();
        }

        public static IStream<T> Cons<T>(this T head, IStream<T> tail)
        {
            return new NonEmptyStream<T>(head, new Lazy<IStream<T>>(() => tail));
        }

        public static IStream<T> LiftStream<T>(this T t)
        {
            return t.Cons(Empty<T>());
        }

        public static IStream<T> Empty<T>()
        {
            return new EmptyStream<T>();
        }

        public static IStream<T> Drop<T>(this IStream<T> xs, int n)
        {
            return n <= 0
                ? xs
                : xs.Tail.Select(ys => ys.Drop(n - 1)).GetOrElse(Empty<T>);
        }

        private class NonEmptyStream<T> : IStream<T>
        {
            private readonly T _head;
            private readonly Lazy<IStream<T>> _tail;

            public IMaybe<T> Head { get { return _head.ToMaybe(); } }

            public IMaybe<IStream<T>> Tail
            {
                get { return _tail.Value.ToMaybe(); }
            }

            public bool Any
            {
                get { return true; }
            }

            public NonEmptyStream(T head, Lazy<IStream<T>> tail)
            {
                _head = head;
                _tail = tail;
            }

            public TResult Match<TResult>(Func<T, IStream<T>, TResult> cons, Func<TResult> nil)
            {
                return cons(_head, _tail.Value);
            }
        }

        private class EmptyStream<T> : IStream<T>
        {
            public IMaybe<T> Head { get { return MaybeExtensions.Nothing<T>(); } }
            public IMaybe<IStream<T>> Tail { get { return MaybeExtensions.Nothing<IStream<T>>(); } }
            public bool Any { get { return false; } }

            public EmptyStream()
            {

            } 

            public TResult Match<TResult>(Func<T, IStream<T>, TResult> cons, Func<TResult> nil)
            {
                return nil();
            }
        }
    }
}
