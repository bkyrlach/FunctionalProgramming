using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FunctionalProgramming.Monad;

using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Basics
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Sequence takes a list of computations of type IMaybe 'T, and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// </summary>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="maybeTs">The list of computations</param>
        /// <returns>A single IMaybe computation of type IEnumerable 'T</returns>
        public static IMaybe<IEnumerable<T>> Sequence<T>(this IEnumerable<IMaybe<T>> maybeTs)
        {
            return BF.If(maybeTs.Any(),
                () =>
                    maybeTs.First()
                        .SelectMany(t => maybeTs.Skip(1).Sequence().SelectMany(ts => ((new[] {t}).Concat(ts)).ToMaybe())),
                () => Enumerable.Empty<T>().ToMaybe());
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
        /// Sequence takes a list of computations of type Io 'T, and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// </summary>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="ioTs">The list of computations</param>
        /// <returns>A single Io computation of type IEnumerable 'T</returns>
        public static Io<IEnumerable<T>> Sequence<T>(this IEnumerable<Io<T>> ioTs)
        {
            return BF.If(ioTs.Any(),
                () =>
                    ioTs.First()
                        .SelectMany(
                            t =>
                                ioTs.Skip(1)
                                    .Sequence()
                                    .SelectMany(ts => Io<IEnumerable<T>>.Apply(() => (new[] {t}).Concat(ts)))),
                () => Io<IEnumerable<T>>.Apply(() => Enumerable.Empty<T>()));
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
            //TODO This should use a string monoid
            return chars.Aggregate(string.Empty, (str, c) => str + c.ToString(CultureInfo.InvariantCulture));
        }
    }
}
