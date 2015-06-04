using System;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public class Reader<TEnvironment, TResult>
    {
        private readonly Func<TEnvironment, TResult> f;

        public Reader(Func<TEnvironment, TResult> f)
        {
            this.f = f;
        }

        public TResult Run(TEnvironment t1)
        {
            return f(t1);
        }
    }

    public static class Reader
    {
        public static Reader<TEnvironment, TResult> Pure<TEnvironment, TResult>(TResult t) where TResult : struct
        {
            return new Reader<TEnvironment, TResult>(BasicFunctions.Const<TEnvironment, TResult>(t));
        }

        public static Reader<TEnvironment, TNewResult> Select<TEnvironment, TResult, TNewResult>(
            this Reader<TEnvironment, TResult> r, Func<TResult, TNewResult> f)
        {
            return new Reader<TEnvironment, TNewResult>(environment => f(r.Run(environment)));
        }

        public static Reader<TEnvironment, TNewResult> SelectMany<TEnvironment, TResult, TNewResult>(
            this Reader<TEnvironment, TResult> r, Func<TResult, Reader<TEnvironment, TNewResult>> f)
        {
            return new Reader<TEnvironment, TNewResult>(environment => f(r.Run(environment)).Run(environment));
        }

        public static Reader<TEnvironment, TSelect> SelectMany<TEnvironment, TInitial, TResult, TSelect>(
            this Reader<TEnvironment, TInitial> m, Func<TInitial, Reader<TEnvironment, TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => new Reader<TEnvironment, TSelect>(environment => selector(a, b))));
        }
    }
}
