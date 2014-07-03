using System.Collections.Generic;
using System.Linq;

namespace FunctionalProgramming.Basics
{
    /// <summary>
    /// A monoid is a semi-group for which an identity value can be defined such that MAppend MZero 'T -> 'T
    /// </summary>
    /// <typeparam name="T">The set for which this monoid is defined</typeparam>
    public interface IMonoid<T>
    {
        /// <summary>
        /// The identity value for the semigroup T
        /// </summary>
        T MZero { get; }

        /// <summary>
        /// A binary operation for the semigroup T that respects the MZero relationship
        /// </summary>
        /// <param name="t1">A value from semigroup T</param>
        /// <param name="t2">Another value from semigroup T</param>
        /// <returns></returns>
        T MAppend(T t1, T t2);
    }

    /// <summary>
    /// A monoid for sequences whereby MZero is an empty sequence and MAppend is concatenation
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence</typeparam>
    public class EnumerableMonoid<T> : IMonoid<IEnumerable<T>>
    {
        /// <summary>
        /// As this represents a mathematical capability and not a value, there should not be more than
        /// one of these things ever.
        /// </summary>
        public static IMonoid<IEnumerable<T>> Only = new EnumerableMonoid<T>(); 

        private EnumerableMonoid()
        {
        }

        public IEnumerable<T> MZero { get { return Enumerable.Empty<T>(); } }

        public IEnumerable<T> MAppend(IEnumerable<T> t1, IEnumerable<T> t2)
        {
            return t1.Concat(t2);
        }
    }
}
