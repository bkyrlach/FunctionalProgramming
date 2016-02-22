using System;
using System.Threading.Tasks;

namespace FunctionalProgramming.Monad.Outlaws
{
    public static class TaskExtensions
    {
        public static T SafeRun<T>(this Task<T> t)
        {
            t.SafeStart();
            return t.Result;
        }

        public static void SafeStart<T>(this Task<T> t)
        {
            if (t.Status == TaskStatus.Created)
            {
                t.Start();
            }
        }

        public static Task<T2> Select<T, T2>(this Task<T> t, Func<T, T2> f)
        {
            return new Task<T2>(() => f(t.SafeRun()));
        }

        public static Task<T2> SelectMany<T, T2>(this Task<T> t, Func<T, Task<T2>> f)
        {
            return new Task<T2>(() => f(t.SafeRun()).SafeRun());
        }

        public static Task<IEither<T1, T2>> WhenEither<T1, T2>(Task<T1> t1, Task<T2> t2)
        {
            return new Task<IEither<T1, T2>>(() =>
            {
                IEither<T1, T2> result;
                t1.SafeStart();
                t2.SafeStart();
                var tresult = Task.WhenAny(t1, t2).SafeRun();
                if (tresult == t1)
                {
                    result = t1.Result.AsLeft<T1, T2>();
                }
                else
                {
                    result = t2.Result.AsRight<T1, T2>();
                }
                return result;
            });
        }        
    }
}
