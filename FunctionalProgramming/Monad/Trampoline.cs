using System;

namespace FunctionalProgramming.Monad
{
    public abstract class Trampoline<T>
    {
        public abstract TResult Match<TResult>(Func<Func<Trampoline<T>>, TResult> more, Func<T, TResult> done);

        public T Run()
        {
            return Match(
                more: k => k().Run(),
                done: v => v);
        }

        private class More : Trampoline<T>
        {
            private readonly Func<Trampoline<T>> _k; 
            public More(Func<Trampoline<T>> k)
            {
                _k = k;
            }

            public override TResult Match<TResult>(Func<Func<Trampoline<T>>, TResult> more, Func<T, TResult> done)
            {
                return more(_k);
            }
        }

        private class Done : Trampoline<T>
        {
            private readonly T _t;

            public Done(T t)
            {
                _t = t;
            }

            public override TResult Match<TResult>(Func<Func<Trampoline<T>>, TResult> more, Func<T, TResult> done)
            {
                return done(_t);
            }
        }
    }
}
