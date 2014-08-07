using System;
using System.Collections.Generic;
using FunctionalProgramming.Basics;

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
            var result = MaybeExtensions.Nothing<T>();
            while (result.IsEmpty)
            {
                step.Match(
                    more: k =>
                    {
                        step = k();
                        return Unit.Only;
                    },
                    cont: (t, f) =>
                    {
                        step = t;
                        stack.Enqueue(f);
                        return Unit.Only;
                    },
                    done: t =>
                    {
                        if (stack.Count == 0)
                        {
                            result = t.ToMaybe();
                        }
                        else
                        {
                            var c = stack.Dequeue();
                            step = c(t);
                        }
                        return Unit.Only;
                    });
            }
            return result.GetOrError(() => new Exception("Impossibru!"));
            //var step = this.AsLeft<Trampoline<T>, T>();
            //while (!step.IsRight)
            //{
            //    step = step.Match(
            //        left: trampoline => trampoline.Match(
            //            more: k => k().AsLeft<Trampoline<T>, T>(),
            //            done: t => t.AsRight<Trampoline<T>, T>()),
            //        right: t => t.AsRight<Trampoline<T>, T>());
            //}
            //return step.Match(left: trampoline => { throw new Exception("Impossibru!"); }, right: t => t);
        }
    }

    public sealed class More<T> : Trampoline<T>
    {
        private readonly Func<Trampoline<T>> _k;
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
        private readonly Trampoline<T> _t;
        private readonly Func<T, Trampoline<T>> _f;

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
        private readonly T _t;

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