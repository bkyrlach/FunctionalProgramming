using System;
using System.Threading.Tasks;

namespace FunctionalProgramming.Monad
{
    public static class TaskExtensions
    {
        public static Task<T> ToTask<T>(this T t)
        {
            return Task.FromResult(t);
        }

        public static async Task<TResult> Select<TInitial, TResult>(this Task<TInitial> t, Func<TInitial, TResult> f)
        {
            return f(await t);
        }

        public static async Task<TResult> SelectMany<TInitial, TResult>(this Task<TInitial> t, Func<TInitial, Task<TResult>> f)
        {
            return await f(await t);
        }
    }
}
