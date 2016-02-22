using System;

namespace FunctionalProgramming.Helpers
{
    public static class FuncExtensions
    {
        public static Func<T1, T3> Select<T1, T2, T3>(this Func<T1, T2> m, Func<T2, T3> f)
        {
            return f.Compose(m);
        }

        public static Func<T2> Select<T1, T2>(this Func<T1> m, Func<T1, T2> f)
        {
            return () => f(m());
        } 

        /// <summary>
        /// Predicate combinator using boolean AND
        /// </summary>
        /// <typeparam name="T">The type for which 'f' and 'g' are propositions</typeparam>
        /// <param name="f">A proposition about 'T</param>
        /// <param name="g">Another proposition about 'T</param>
        /// <returns>A predicate that is satisfied IFF 'f' and 'g' are satisfied</returns>
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

        /// <summary>
        /// Transforms a function (a, b) -> c to the form a -> b -> c
        /// </summary>
        /// <typeparam name="T1">The type of the first input of the function 'f'</typeparam>
        /// <typeparam name="T2">The type of the second input of the function 'f'</typeparam>
        /// <typeparam name="T3">The type of output of the function 'f'</typeparam>
        /// <param name="f">A function to curry</param>
        /// <returns>An n (2nd) order function that otherwise represents the same relationship as 'f'</returns>
        public static Func<T1,Func<T2,T3>> Curry<T1, T2, T3>(this Func<T1,T2,T3> f)
        {
            return t1 => t2 => f(t1, t2);
        }

        /// <summary>
        /// Transforms a function (a, b, c) -> d to the form a -> b -> c -> d
        /// </summary>
        /// <typeparam name="T1">The type of the first input of the function 'f'</typeparam>
        /// <typeparam name="T2">The type of the second input of the function 'f'</typeparam>
        /// <typeparam name="T3">The type of the third input of the function 'f'</typeparam>
        /// <typeparam name="T4">The type of output of the function 'f'</typeparam>
        /// <param name="f">A function to curry</param>
        /// <returns>An n (3rd) order function that otherwise represents the same relationship as 'f'</returns>
        public static Func<T1,Func<T2,Func<T3,T4>>> Curry<T1,T2,T3,T4>(this Func<T1,T2,T3,T4> f)
        {
            return t1 => t2 => t3 => f(t1, t2, t3);
        }

        /// <summary>
        /// Transforms the function (a, b, c, d) -> e to the form a -> b -> c -> d -> e
        /// </summary>
        /// <typeparam name="T1">The type of the first input of the function 'f'</typeparam>
        /// <typeparam name="T2">The type of the second input of the function 'f'</typeparam>
        /// <typeparam name="T3">The type of the third input of the function 'f'</typeparam>
        /// <typeparam name="T4">The type of the fourth input of the function 'f'</typeparam>
        /// <typeparam name="T5">The type of output of the function 'f'</typeparam>
        /// <param name="f">A function to curry</param>
        /// <returns>An n (4th) order function that otherwise represents the same relationship as 'f'</returns>
        public static Func<T1, Func<T2, Func<T3, Func<T4, T5>>>> Curry<T1, T2, T3, T4, T5>(
            this Func<T1, T2, T3, T4, T5> f)
        {
            return t1 => t2 => t3 => t4 => f(t1, t2, t3, t4);
        }
    }
}
