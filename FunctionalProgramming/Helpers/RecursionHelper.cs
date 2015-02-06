using System;

namespace FunctionalProgramming.Helpers
{
    public sealed class Continue<T> : Exception
    {
        public readonly T State;
        
        public Continue(T state)
        {
            State = state;
        } 
    }
    
    public static class RecursionHelper
    {
        [ThreadStatic]
        public static int CallCount;

        public static void Check<TState>(TState currentState)
        {
            if (CallCount > 50)
            {
                CallCount = 0;
                throw new Continue<TState>(currentState);
            }
            else
            {
                CallCount++;
            }
        } 

        public static TResult Recur<TState, TResult>(TState initial, Func<TState, TResult> work)
        {
            var isDone = false;
            var result = default(TResult);
            var currentState = initial;
            while (!isDone)
            {
                try
                {
                    result = work(currentState);
                    isDone = true;
                }
                catch (Continue<TState> cont)
                {
                    currentState = cont.State;
                }
            }
            CallCount = 0;
            return result;
        }
    }
}
