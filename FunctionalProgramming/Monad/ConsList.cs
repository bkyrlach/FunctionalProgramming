using System;

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
