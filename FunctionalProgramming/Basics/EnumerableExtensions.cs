using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;
using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Basics
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Sequence takes a list of computations and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the sequence for IMaybe computations
        /// </summary>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="maybeTs">The list of computations</param>
        /// <returns>A computation thqt yields a sequence of values of type 'T</returns>
        public static IMaybe<IEnumerable<T>> Sequence<T>(this IEnumerable<IMaybe<T>> maybeTs)
        {
            var initial = ConsList.Nil<T>().ToMaybe();
            return maybeTs.Aggregate(initial, (current, maybe) => current.SelectMany(ts => maybe.Select(t => t.Cons(ts)))).Select(xs => xs.AsEnumerable());
        }

        /// <summary>
        /// Traverse maps each value in a sequence to a computation, and then sequences those computations
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the traverse for IMaybe computations
        /// </summary>
        /// <typeparam name="T1">The type of values in the sequence</typeparam>
        /// <typeparam name="T2">The type of value that the computation will yield</typeparam>
        /// <param name="xs">The sequence of values</param>
        /// <param name="f">The function that lifts values from 'T1 to computations that yield 'T2</param>
        /// <returns>A computation that yields a sequence of values of type T2</returns>
        public static IMaybe<IEnumerable<T2>> Traverse<T1, T2>(this IEnumerable<T1> xs, Func<T1, IMaybe<T2>> f)
        {
            return xs.Select(f).Sequence();
        }

        /// <summary>
        /// Sequence takes a list of computations and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the sequence for Io computations
        /// </summary>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="ioTs">The list of computations</param>
        /// <returns>A computation thqt yields a sequence of values of type 'T</returns>
        public static Io<IEnumerable<T>> Sequence<T>(this IEnumerable<Io<T>> ioTs)
        {
            var initial = Io.Apply(() => ConsList.Nil<T>());
            return ioTs.Aggregate(initial, (current, io) => current.SelectMany(ts => io.Select(t => t.Cons(ts)))).Select(ios => ios.AsEnumerable());
        }

        /// <summary>
        /// Traverse maps each value in a sequence to a computation, and then sequences those computations
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the traverse for Io computations
        /// </summary>
        /// <typeparam name="T1">The type of values in the sequence</typeparam>
        /// <typeparam name="T2">The type of value that the computation will yield</typeparam>
        /// <param name="xs">The sequence of values</param>
        /// <param name="f">The function that lifts values from 'T1 to computations that yield 'T2</param>
        /// <returns>A computation that yields a sequence of values of type T2</returns>
        public static Io<IEnumerable<T2>> Traverse<T1, T2>(this IEnumerable<T1> xs, Func<T1, Io<T2>> f)
        {
            return xs.Select(f).Sequence();
        }

        /// <summary>
        /// Do not USE!!!!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskTs"></param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> Sequence<T>(this IEnumerable<Task<T>> taskTs)
        {
            var initial = ConsList.Nil<T>().FromResult();
            return taskTs.Aggregate(initial, (current, task) => current.SelectMany(ts => task.Select(t => t.Cons(ts)))).Select(tasks => tasks.AsEnumerable().Reverse());
        }

        public static Task<IEnumerable<T2>> Traverse<T1, T2>(this IEnumerable<T1> xs, Func<T1, Task<T2>> f)
        {
            return xs.Select(f).Sequence();
        }

        /// <summary>
        /// Sequence takes a list of computations and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the sequence for State computations
        /// </summary>
        /// <typeparam name="TState">Type of associated state</typeparam>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="states">The list of computations</param>
        /// <returns>A computation thqt yields a sequence of values of type 'T</returns>
        public static State<TState, IEnumerable<T>> Sequence<TState, T>(this IEnumerable<State<TState, T>> states)
        {
            var initial = ConsList.Nil<T>().Insert<TState, IConsList<T>>();
            return states.Aggregate(initial, (current, s) => current.SelectMany(ts => s.Select(t => t.Cons(ts)))).Select(x => x.AsEnumerable());
        }

        /// <summary>
        /// Traverse maps each value in a sequence to a computation, and then sequences those computations
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the traverse for State computations
        /// </summary>
        /// <typeparam name="TState">The type of associated state</typeparam>
        /// <typeparam name="T1">The type of values in the sequence</typeparam>
        /// <typeparam name="T2">The type of value that the computation will yield</typeparam>
        /// <param name="xs">The sequence of values</param>
        /// <param name="f">The function that lifts values from 'T1 to computations that yield 'T2</param>
        /// <returns>A computation that yields a sequence of values of type T2</returns>
        public static State<TState, IEnumerable<T2>> Traverse<TState, T1, T2>(this IEnumerable<T1> xs, Func<T1, State<TState, T2>> f)
        {
            return xs.Select(f).Sequence();
        }

        /// <summary>
        /// Sequence takes a list of computations and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the sequence for Try computations
        /// </summary>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="tryTs">The list of computations</param>
        /// <returns>A computation thqt yields a sequence of values of type 'T</returns>
        public static Try<IEnumerable<T>> Sequence<T>(this IEnumerable<Try<T>> tryTs)
        {
            var initial = Try.Attempt(() => ConsList.Nil<T>());
            return tryTs.Aggregate(initial, (current, aTry) => current.SelectMany(ts => aTry.Select(t => t.Cons(ts)))).Select(tries => tries.AsEnumerable());
        }

        /// <summary>
        /// Traverse maps each value in a sequence to a computation, and then sequences those computations
        /// 
        /// Note that due to C#s lack of higher kinded types, this must be specified for every type of computation
        /// This is the traverse for Try computations
        /// </summary>
        /// <typeparam name="T1">The type of values in the sequence</typeparam>
        /// <typeparam name="T2">The type of value that the computation will yield</typeparam>
        /// <param name="xs">The sequence of values</param>
        /// <param name="f">The function that lifts values from 'T1 to computations that yield 'T2</param>
        /// <returns>A computation that yields a sequence of values of type T2</returns>
        public static Try<IEnumerable<T2>> Traverse<T1, T2>(this IEnumerable<T1> xs, Func<T1, Try<T2>> f)
        {
            return xs.Select(f).Sequence();
        }

        /// <summary>
        /// ZipWithIndex takes a collection and pairs each element with its index in the collection
        /// </summary>
        /// <typeparam name="T">The type of elements in the IEnumerable</typeparam>
        /// <param name="xs">The IEnumerable to zip</param>
        /// <returns>An IEnumerable of Tuples where the first element of the tuple is the corresponding element from `xs` and the second element of the tuple is the index of that element</returns>
        public static IEnumerable<Tuple<T, int>> ZipWithIndex<T>(this IEnumerable<T> xs)
        {
            var i = 0;
            foreach (var x in xs)
            {
                yield return new Tuple<T, int>(x, i);
                i++;
            }
        }

        /// <summary>
        /// Helper function that lifts a value to the category IEnumerable
        /// </summary>
        /// <typeparam name="T">The type of value to lift</typeparam>
        /// <param name="t">The value to lift</param>
        /// <returns>The value lifted to the category IEnumerable</returns>
        public static IEnumerable<T> LiftEnumerable<T>(this T t)
        {
            return new[] {t};
        }

        /// <summary>
        /// Helper function that is the dual of the implicit conversion string -> IEnumerable 'char
        /// </summary>
        /// <param name="chars">A sequence of chars to represent as a string</param>
        /// <returns>A string that is the result of concatenating the characters together</returns>
        public static string MkString(this IEnumerable<char> chars)
        {
            var sm = StringMonoid.Only;
            return chars.Select(c => c.ToString(CultureInfo.InvariantCulture)).Aggregate(sm.MZero, sm.MAppend);
        }

        /// <summary>
        /// Type-safe version of LINQs 'First' and 'FirstOrDefualt' function that returns a Maybe 
        /// instead of possibly throwing an exception or returning null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">An enumerable of elements that we want the first element of</param>
        /// <returns>Just the first value in the enumerable, or nothing if no such element exists</returns>
        public static IMaybe<T> MaybeFirst<T>(this IEnumerable<T> ts) where T : class
        {
            return ts.MaybeFirst(BasicFunctions.Const<T, bool>(true));
        }

        /// <summary>
        /// Type-safe version of LINQs 'First' and 'FirstOrDefault' function that accepts a predicate, returning
        /// a Maybe instead of possibly throwing an exception or returning null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">An enumerable from which we want the first element that satisfies the provided predicate</param>
        /// <param name="predicate">A predicate for which the first element to satisfy will be the return value</param>
        /// <returns>Just the first value in the enumerable that satisfies the predicate, or nothing if no such element exists</returns>
        public static IMaybe<T> MaybeFirst<T>(this IEnumerable<T> ts, Func<T, bool> predicate) where T : class
        {
            return ts.FirstOrDefault(predicate).ToMaybe();
        }

        public static IMaybe<T> MaybeFirst<T>(this IQueryable<T> ts, Expression<Func<T, bool>> predicate)
            where T : class
        {
            return ts.FirstOrDefault(predicate).ToMaybe();
        }

        public static IMaybe<T> MaybeFirst<T>(this IQueryable<T> ts) where T : class
        {
            return ts.MaybeFirst(t => true);
        } 
    }
}
