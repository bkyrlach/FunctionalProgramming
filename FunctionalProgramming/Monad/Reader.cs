using System;

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

    public static class ReaderExtensions
    {
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
            return from initial in m
                from result in f(initial)
                select selector(initial, result);
        }
    }
}
