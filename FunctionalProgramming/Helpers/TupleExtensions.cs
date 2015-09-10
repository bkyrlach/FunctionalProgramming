using System;

namespace FunctionalProgramming.Helpers
{
    static class TupleExtensions
    {
        public static TResult Apply<T1, T2, TResult>(this Tuple<T1, T2> tuple, Func<T1, T2, TResult> f)
        {
            return f(tuple.Item1, tuple.Item2);
        }

        public static TResult Apply<T1, T2, T3, TResult>(this Tuple<T1, T2, T3> tuple, Func<T1, T2, T3, TResult> f)
        {
            return f(tuple.Item1, tuple.Item2, tuple.Item3);
        }

        public static TResult Apply<T1, T2, T3, T4, TResult>(this Tuple<T1, T2, T3, T4> tuple, Func<T1, T2, T3, T4, TResult> f)
        {
            return f(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        }

        public static TResult Apply<T1, T2, T3, T4, T5, TResult>(this Tuple<T1, T2, T3, T4, T5> tuple, Func<T1, T2, T3, T4, T5, TResult> f)
        {
            return f(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
        }

        public static TResult Apply<T1, T2, T3, T4, T5, T6, TResult>(this Tuple<T1, T2, T3, T4, T5, T6> tuple, Func<T1, T2, T3, T4, T5, T6, TResult> f)
        {
            return f(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
        }

        public static TResult Apply<T1, T2, T3, T4, T5, T6, T7, TResult>(this Tuple<T1, T2, T3, T4, T5, T6, T7> tuple, Func<T1, T2, T3, T4, T5, T6, T7, TResult> f)
        {
            return f(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7);
        }
    }
}
