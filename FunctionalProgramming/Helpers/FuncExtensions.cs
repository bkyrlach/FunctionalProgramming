using System;

namespace FunctionalProgramming.Helpers
{
    public static class FuncExtensions
    {
        public static Func<T, bool> And<T>(this Func<T, bool> f, Func<T, bool> g)
        {
            return t => f(t) && g(t);
        }

        public static Func<T, T2> AndThen<T, T1, T2>(this Func<T, T1> f, Func<T1, T2> g)
        {
            return t => g(f(t));
        } 

        /// <summary>
        /// This method does type safe functional composition
        /// </summary>
        /// <typeparam name="TInitial">Input for g</typeparam>
        /// <typeparam name="TIntermediate">Output for g, input for f</typeparam>
        /// <typeparam name="TResult">Output of composed function</typeparam>
        /// <param name="f">Function to apply Second</param>
        /// <param name="g">Function to apply First</param>
        /// <returns>f . g</returns>
        public static Func<TInitial, TResult> Compose<TInitial, TIntermediate, TResult>(
            this Func<TIntermediate, TResult> f, Func<TInitial, TIntermediate> g)
        {
            return tInitial => f(g(tInitial));
        }

        public static Func<T1,Func<T2,T3>> Curry<T1, T2, T3>(this Func<T1,T2,T3> f)
        {
            return t1 => t2 => f(t1, t2);
        }

        public static Func<T1,Func<T2,Func<T3,T4>>> Curry<T1,T2,T3,T4>(this Func<T1,T2,T3,T4> f)
        {
            return t1 => t2 => t3 => f(t1, t2, t3);
        }

        public static Func<T1, Func<T2, Func<T3, Func<T4, T5>>>> Curry<T1, T2, T3, T4, T5>(
            this Func<T1, T2, T3, T4, T5> f)
        {
            return t1 => t2 => t3 => t4 => f(t1, t2, t3, t4);
        }
    }
}
