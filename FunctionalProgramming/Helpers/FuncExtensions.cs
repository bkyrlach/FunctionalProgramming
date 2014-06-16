using System;

namespace FunctionalProgramming.Helpers
{
    public static class FuncExtensions
    {
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
    }
}
