using System;
using System.Collections.Generic;

namespace FunctionalProgramming.Monad
{
    public static class TrampolineExtensions
    {
        public static ITrampoline<T2> Select<T, T2>(this ITrampoline<T> m, Func<T, T2> f)
        {
            return new Transform<T,T2>(m, f);
        }

        public static ITrampoline<T2> SelectMany<T, T2>(this ITrampoline<T> m, Func<T, ITrampoline<T2>> f)
        {
            return new Cont<T, T2>(m, f);
        }

        public static T Run<T>(this ITrampoline<T> self)
        {
            object o = null;
            var stack = new Stack<Func<object, object>>();
            var next = (ITrampoline)self;
            var isDone = false;
            while (!isDone)
            {
                var data = next.RunStep();
                if (data[0] != null)
                    next = (ITrampoline)data[0];
                if (data[1] != null)
                    stack.Push((Func<object, object>)data[1]);
                if (data[2] != null)
                {
                    o = data[2];
                    if (stack.Count == 0)
                    {
                        isDone = true;
                    }
                    else
                    {
                        var apply = true;
                        while (apply)
                        {
                            var f = stack.Pop();
                            var temp = f(o);
                            if (temp is ITrampoline)
                            {
                                apply = false;
                                next = (ITrampoline) temp;
                            }
                            else
                            {
                                o = temp;
                            }
                            apply = apply && (stack.Count != 0);
                        }

                    }
                }
            }
            return (T)o;
        }

    }

    internal interface ITrampoline
    {
        object[] RunStep();
    }

    public interface ITrampoline<T>
    {
    }

    public struct More<T> : ITrampoline<T>, ITrampoline
    {
        private readonly Func<ITrampoline<T>> _continuation;
        public More(Func<ITrampoline<T>> continuation)
        {
            _continuation = continuation;
        }

        public object[] RunStep()
        {
            return new object[] {(ITrampoline) _continuation(), null, null};
        }
    }

    public struct Cont<T, T2> : ITrampoline<T2>, ITrampoline
    {
        private readonly ITrampoline<T> _next;
        private readonly Func<object, object> _transform;

        public Cont(ITrampoline<T> next, Func<T, ITrampoline<T2>> transform)
        {
            Func<object, object> replacement = o => transform((T) o);
            _next = next;
            _transform = replacement;
        }

        public object[] RunStep()
        {
            return new object[] {(ITrampoline) _next, _transform, null};
        }
    }

    public struct Transform<T, T2> : ITrampoline<T2>, ITrampoline
    {
        private readonly ITrampoline<T> _next;
        private readonly Func<object, object> _transform;

        public Transform(ITrampoline<T> next, Func<T, T2> transform)
        {
            Func<object, object> replacement = o => transform((T) o);
            _next = next;
            _transform = replacement;
        }

        public object[] RunStep()
        {
            return new object[] { (ITrampoline)_next, _transform, null };
        }

    }

    public struct Done<T> : ITrampoline<T>, ITrampoline
    {
        private readonly T _value;

        public Done(T value)
        {
            _value = value;
        }

        public object[] RunStep()
        {
            return new object[] {null, null, _value};
        }
    }
}