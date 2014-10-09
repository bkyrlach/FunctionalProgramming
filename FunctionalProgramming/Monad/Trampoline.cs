using System;
using System.Collections.Generic;

namespace FunctionalProgramming.Monad
{
    public abstract class Trampoline<T> 
    {
        public abstract TResult Match<TResult>(
            Func<Func<Trampoline<T>>, TResult> more,
            Func<Trampoline<T>, Func<T, Trampoline<T>>, TResult> cont, 
            Func<T, TResult> done);

        public T Run()
        {
            var step = this;
            var stack = new Queue<Func<T, Trampoline<T>>>();
            var result = Maybe.Nothing<T>();
            while (result.IsEmpty)
            {
                if (step is More<T>)
                {
                    var m = (step as More<T>);
                    step = m._k();
                }
                else if (step is Cont<T>)
                {
                    var c = (step as Cont<T>);
                    step = c._t;
                    stack.Enqueue(c._f);
                }
                else if (step is Done<T>)
                {
                    var d = (step as Done<T>);
                    if (stack.Count == 0)
                    {
                        result = d._t.ToMaybe();
                    }
                    else
                    {
                        step = stack.Dequeue()(d._t);
                    }
                }
                
            }
            return result.GetOrError(() => new Exception("Impossibru!"));
        }
    }

    public sealed class More<T> : Trampoline<T>
    {
        public readonly Func<Trampoline<T>> _k;
        public More(Func<Trampoline<T>> k)
        {
            _k = k;
        }

        public override TResult Match<TResult>(
            Func<Func<Trampoline<T>>, TResult> more,
            Func<Trampoline<T>, Func<T, Trampoline<T>>, TResult> cont, 
            Func<T, TResult> done)
        {
            return more(_k);
        }
    }

    public sealed class Cont<T> : Trampoline<T>
    {
        public readonly Trampoline<T> _t;
        public readonly Func<T, Trampoline<T>> _f;

        public Cont(Trampoline<T> t, Func<T, Trampoline<T>> f)
        {
            _t = t;
            _f = f;
        }

        public override TResult Match<TResult>(
            Func<Func<Trampoline<T>>, TResult> more, 
            Func<Trampoline<T>, Func<T, Trampoline<T>>, TResult> cont, 
            Func<T, TResult> done)
        {
            return cont(_t, _f);
        }
    }

    public sealed class Done<T> : Trampoline<T>
    {
        public readonly T _t;

        public Done(T t)
        {
            _t = t;
        }

        public override TResult Match<TResult>(
            Func<Func<Trampoline<T>>, TResult> more,
            Func<Trampoline<T>, Func<T, Trampoline<T>>, TResult> cont, 
            Func<T, TResult> done)
        {
            return done(_t);
        }
    }

    public static class TrampolineExtensions
    {
        public static Trampoline<T> Select<T>(this Trampoline<T> m, Func<T, T> f)
        {
            return m.SelectMany(a => new More<T>(() => new Done<T>(f(a))));
        } 

        public static Trampoline<T> SelectMany<T>(this Trampoline<T> m,
            Func<T, Trampoline<T>> f)
        {
            return new Cont<T>(m, f);
        }
    }
}