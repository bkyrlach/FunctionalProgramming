using System;
using System.Net.Sockets;

namespace FunctionalProgramming.Monad
{
    public abstract class Trampoline<T>
    {
        public abstract bool IsDone { get; }

        public abstract TResult Match<TResult>(Func<Func<Trampoline<T>>, TResult> more, Func<T, TResult> done);

        public T Run()
        {
            var step = this;
            var retval = default(T);
            while (!step.IsDone)
            {
                step = step.Match(
                    more: k => k(),
                    done: t =>
                    {
                        retval = t;
                        return new Done<T>(t);
                    });
            }
            return retval;
        }
    }

    public sealed class More<T> : Trampoline<T>
    {
        private readonly Func<Trampoline<T>> _k;
        public More(Func<Trampoline<T>> k)
        {
            _k = k;
        }

        public override bool IsDone
        {
            get { return false; }
        }

        public override TResult Match<TResult>(Func<Func<Trampoline<T>>, TResult> more, Func<T, TResult> done)
        {
            return more(_k);
        }
    }

    public sealed class Done<T> : Trampoline<T>
    {
        private readonly T _t;

        public Done(T t)
        {
            _t = t;
        }

        public override bool IsDone
        {
            get { return true; }
        }

        public override TResult Match<TResult>(Func<Func<Trampoline<T>>, TResult> more, Func<T, TResult> done)
        {
            return done(_t);
        }
    }

    public static class TrampolineExtensions
    {
        public static Trampoline<TResult> Select<TValue, TResult>(this Trampoline<TValue> m, Func<TValue, TResult> f)
        {
            return m.Match<Trampoline<TResult>>(
                more: k => new More<TResult>(() => k().Select<TValue, TResult>(f)),
                done: t => new Done<TResult>(f(t)));
        }
    }
}
