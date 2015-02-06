using System;
using System.Threading.Tasks;

namespace FunctionalProgramming.Monad.Outlaws
{
    //TODO Fix horrible brokenness. Perhaps replace with implementation of 'Futures
    /// <summary>
    /// DO NOT USE
    /// </summary>
    public static class TaskExtensions
    {
        public static Task<T> FromResult<T>(this T t)
        {
            return Task.FromResult(t);
        }

        public static async Task<TResult> Select<TInitial, TResult>(this Task<TInitial> m, Func<TInitial, TResult> f)
        {
            return f(await m);
        }

        public static async Task<TResult> SelectMany<TInitial, TResult>(this Task<TInitial> m,
            Func<TInitial, Task<TResult>> f)
        {
            return await f(await m);
        }

        public static Task<TSelect> SelectMany<TInitial, TResult, TSelect>(this Task<TInitial> m,
            Func<TInitial, Task<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => selector(a, b).FromResult()));
        }

        public static T Await<T>(this Task<T> t)
        {
            if (t.Status == TaskStatus.Created)
            {
                t.Start();
            }
            return t.Result;
        }
    }
}
