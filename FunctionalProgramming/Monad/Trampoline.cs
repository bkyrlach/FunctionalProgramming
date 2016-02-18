using System;
using System.Collections.Generic;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public static class TrampolineExtensions
    {
        public static Trampoline<T2> Select<T, T2>(this Trampoline<T> m, Func<T, T2> f)
        {
            return m.SelectMany(a => new More<T2>(() => new Done<T2>(f(a))));
        }

        public static Trampoline<T2> SelectMany<T, T2>(this Trampoline<T> m,
            Func<T, Trampoline<T2>> f)
        {
            return new Cont<T, T2>(m, f);
        }
    }

    public class TrampolineException<T> : Exception
    {
        public Trampoline<T> Next;

        public TrampolineException(Trampoline<T> next)
        {
            Next = next;
        }
    }

    public interface ITrampoline
    {
        IEither<Tuple<ITrampoline, Delegate>, object> RunStep();
    }

    public abstract class Trampoline<T>
    {
        public T Run()
        {
            object o = null;
            var stack = new Stack<Delegate>();
            var next = (ITrampoline)this;
            var isDone = false;
            while (!isDone)
            {
                next.RunStep().Match(
                    left: pair =>
                    {
                        next = pair.Item1;
                        if (pair.Item2 != null)
                        {
                            stack.Push(pair.Item2);
                        }
                        return Unit.Only;
                    },
                    right: obj =>
                    {
                        o = obj;
                        if (stack.Count == 0)
                        {
                            isDone = true;
                        }
                        else
                        {
                            var d = stack.Pop();
                            next = (ITrampoline)d.DynamicInvoke(o);
                        }
                        return Unit.Only;
                    });
            }
            return (T)o;
        }
    }

    public sealed class More<T> : Trampoline<T>, ITrampoline
    {
        public readonly Func<Trampoline<T>> Continuation;
        public More(Func<Trampoline<T>> continuation)
        {
            Continuation = continuation;
        }

        public IEither<Tuple<ITrampoline, Delegate>, object> RunStep()
        {
            return Tuple.Create<ITrampoline, Delegate>((ITrampoline)Continuation(), null).AsLeft<Tuple<ITrampoline, Delegate>, object>();
        }
    }

    public sealed class Cont<T, T2> : Trampoline<T2>, ITrampoline
    {
        public readonly Trampoline<T> Next;
        public readonly Func<T, Trampoline<T2>> Transform;

        public Cont(Trampoline<T> next, Func<T, Trampoline<T2>> transform)
        {
            Next = next;
            Transform = transform;
        }

        public IEither<Tuple<ITrampoline, Delegate>, object> RunStep()
        {
            return Tuple.Create<ITrampoline, Delegate>((ITrampoline)Next, Transform).AsLeft<Tuple<ITrampoline, Delegate>, object>();
        }
    }

    public sealed class Done<T> : Trampoline<T>, ITrampoline
    {
        public readonly T Value;

        public Done(T value)
        {
            Value = value;
        }

        public IEither<Tuple<ITrampoline, Delegate>, object> RunStep()
        {
            return Value.AsRight<Tuple<ITrampoline, Delegate>, object>();
        }
    }
}