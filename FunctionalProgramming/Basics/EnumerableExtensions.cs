using System.Net.NetworkInformation;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using FunctionalProgramming.Monad.Parsing;
using FunctionalProgramming.Monad.Transformer;

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
            return maybeTs.Reverse().Aggregate(initial, (current, maybe) => current.SelectMany(ts => maybe.Select(t => t.Cons(ts)))).Select(xs => xs.AsEnumerable());
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
            return ioTs.Aggregate(initial, (current, io) => current.SelectMany(ts => io.Select(t => t.Cons(ts)))).Select(ios => ios.AsEnumerable().Reverse());
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
            return taskTs.Reverse().Aggregate(initial, (current, task) => current.SelectMany(ts => task.Select(t => t.Cons(ts)))).Select(tasks => tasks.AsEnumerable());
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
            return states.Aggregate(initial, (current, s) => current.SelectMany(ts => s.Select(t => t.Cons(ts)))).Select(x => x.AsEnumerable().Reverse());
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
        /// <param name="tries">The list of computations</param>
        /// <returns>A computation thqt yields a sequence of values of type 'T</returns>
        public static Try<IEnumerable<T>> Sequence<T>(this IEnumerable<Try<T>> tries)
        {
            var initial = Try.Attempt(() => ConsList.Nil<T>());
            return tries.Reverse().Aggregate(initial, (current, aTry) => current.SelectMany(ts => aTry.Select(t => t.Cons(ts)))).Select(ts => ts.AsEnumerable());
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

        public static StateEither<TState, TLeft, IEnumerable<TRight>>  Sequence<TState, TLeft, TRight>(this IEnumerable<StateEither<TState, TLeft, TRight>> stateTs)
        {
            return new StateEither<TState, TLeft, IEnumerable<TRight>>(new State<TState, IEither<TLeft, IEnumerable<TRight>>>(s =>
            {
                var retval = ConsList.Nil<TRight>().AsRight<TLeft, IConsList<TRight>>();
                foreach (var state in stateTs)
                {
                    var res = state.Out.Run(s);
                    s = res.Item1;
                    retval =
                        from xs in retval
                        from x in res.Item2
                        select x.Cons(xs);
                }
                return Tuple.Create(s, retval.Select(xs => xs.AsEnumerable().Reverse()));
            }));

            //var initial = ConsList.Nil<TRight>().InsertRight<TState, TLeft, IConsList<TRight>>();
            //return stateTs.Aggregate(initial, (current, aStateEither) => current.SelectMany(ts => aStateEither.Select(t => t.Cons(ts)))).Select(ts => ts.AsEnumerable().Reverse());
        }

        public static StateEither<TState, TLeft, IEnumerable<TResult>> Traverse<TState, TLeft, TRight, TResult>(
            this IEnumerable<TRight> stateTs, Func<TRight, StateEither<TState, TLeft, TResult>> f)
        {
            return stateTs.Select(f).Sequence();
        }

        public static IEither<TLeft, IEnumerable<TRight>> Sequence<TLeft, TRight>(this IEnumerable<IEither<TLeft, TRight>> xs)
        {
            var initial = ConsList.Nil<TRight>().AsRight<TLeft, IConsList<TRight>>();
            return xs.Aggregate(initial, (current, anEither) => current.SelectMany(ts => anEither.Select(t => t.Cons(ts)))).Select(eithers => eithers.AsEnumerable().Reverse());
        }

        public static StateIo<TState, IEnumerable<TValue>> Sequence<TState, TValue>(
            this IEnumerable<StateIo<TState, TValue>> stateTs)
        {
            var initial = ConsList.Nil<TValue>().ToStateIo<TState, IConsList<TValue>>();
            return
                stateTs.Aggregate(initial,
                    (current, anIoState) => current.SelectMany(ts => anIoState.Select(t => t.Cons(ts))))
                    .Select(ioStates => ioStates.AsEnumerable().Reverse());
        }

        public static StateIo<TState, IEnumerable<TValue>> Traverse<TState, TInitial, TValue>(
            this IEnumerable<TInitial> xs, Func<TInitial, StateIo<TState, TValue>> f)
        {
            return xs.Select(f).Sequence();
        }

        public static IoTry<IEnumerable<T>> Sequence<T>(this IEnumerable<IoTry<T>> ioTs)
        {
            var initial = Io.Apply(() => ConsList.Nil<T>()).ToIoTry();
            return ioTs.Aggregate(initial, (current, maybe) => current.SelectMany(ts => maybe.Select(t => t.Cons(ts)))).Select(xs => xs.AsEnumerable());
        }

        public static IoTry<IEnumerable<T2>> Traverse<T1, T2>(this IEnumerable<T1> ioTs, Func<T1, IoTry<T2>> f)
        {
            return ioTs.Select(f).Sequence();
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
            return new[] { t };
        }

        /// <summary>
        /// Helper function that is the dual of the implicit conversion string -> IEnumerable 'char
        /// </summary>
        /// <param name="chars">A sequence of chars to represent as a string</param>
        /// <returns>A string that is the result of concatenating the characters together</returns>
        public static string MkString(this IEnumerable<char> chars)
        {
            return new string(chars.ToArray());
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

        /// <summary>
        /// Type-safe version of LINQs 'Single' and 'SingleOrDefault' function that accepts a predicate, returning
        /// a Maybe instead of possibly throwing an exception or returning null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">An enumerable from which we want the only element that satisfies the provided predicate</param>
        /// <param name="predicate">A predicate for which only one element will satisfy to be the return value</param>
        /// <returns>The only value in the enumerable that satisfies the predicate, or nothing if no such element exists or multiple elements satisifying the predicate exist</returns>
        public static IMaybe<T> MaybeSingle<T>(this IEnumerable<T> ts, Func<T, bool> predicate) where T : class
        {
            return Try.Attempt(() => ts.SingleOrDefault(predicate).ToMaybe()).AsMaybe().Join();
        }

        /// <summary>
        /// Type-safe version of LINQs 'Single' and 'SingleOrDefault' function that returns a Maybe 
        /// instead of possibly throwing an exception or returning null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">An enumerable of elements that we want the only element of</param>
        /// <returns>The only value in the enumerable, or nothing if no such element exists or multiple elements exist</returns>
        public static IMaybe<T> MaybeSingle<T>(this IEnumerable<T> ts) where T : class
        {
            return ts.MaybeSingle(BasicFunctions.Const<T, bool>(true));
        }

        /// <summary>
        /// Type-safe version of LINQs 'Single' and 'SingleOrDefault' function that accepts a predicate, returning
        /// a Maybe instead of possibly throwing an exception or returning null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">A queryable from which we want the only element that satisfies the provided predicate</param>
        /// <param name="predicate">A predicate for which only one element will satisfy to be the return value</param>
        /// <returns>The only value in the queryable that satisfies the predicate, or nothing if no such element exists or multiple elements satisifying the predicate exist</returns>
        public static IMaybe<T> MaybeSingle<T>(this IQueryable<T> ts, Expression<Func<T, bool>> predicate)
            where T : class
        {
            return Try.Attempt(() => ts.SingleOrDefault(predicate).ToMaybe()).AsMaybe().Join();
        }

        /// <summary>
        /// Type-safe version of LINQs 'Single' and 'SingleOrDefault' function that returns a Maybe 
        /// instead of possibly throwing an exception or returning null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">A queryable of elements that we want the only element of</param>
        /// <returns>The only value in the queryable, or nothing if no such element exists or multiple elements exist</returns>
        public static IMaybe<T> MaybeSingle<T>(this IQueryable<T> ts) where T : class
        {
            return ts.MaybeFirst(t => true);
        }
    }
}
