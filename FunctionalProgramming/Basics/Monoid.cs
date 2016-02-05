using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Helpers;
using FunctionalProgramming.Monad;

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
    /// Proof that strings are a monoid
    /// </summary>
    public sealed class StringMonoid : IMonoid<string>
    {
        /// <summary>
        /// This proof is represented as a singleton (as proofs should not have multiple instances)
        /// </summary>
        public static IMonoid<string> Only = new StringMonoid();

        /// <summary>
        /// Default constructor marked as private to prevent multiple instances of this proof
        /// </summary>
        private StringMonoid()
        {
            
        }

        /// <summary>
        /// The identity value for the string monoid is the empty string
        /// </summary>
        public string MZero { get { return string.Empty; } }

        /// <summary>
        /// The binary operation for the string monoid is concatenation.
        /// 
        /// Monoid property: Mappend("anyString", MZero) == "anyString"
        /// </summary>
        /// <param name="s1">Any value from the monoid string</param>
        /// <param name="s2">Any value from the monoid string</param>
        /// <returns>'s2' appended to 's1'</returns>
        public string MAppend(string s1, string s2)
        {
            return string.Format("{0}{1}", s1, s2);
        }
    }

    /// <summary>
    /// Proof that ints are a monoid
    /// </summary>
    public sealed class IntMonoid : IMonoid<int>
    {
        /// <summary>
        /// This proof is represented as a singleton (as proofs should not have multiple instances)
        /// </summary>
        public static IMonoid<int> Only = new IntMonoid();

        /// <summary>
        /// Default constructor marked as private to prevent multiple instances of this proof from
        /// being created
        /// </summary>
        private IntMonoid()
        {
            
        }

        /// <summary>
        /// The identity value for the int monoid is zero
        /// </summary>
        public int MZero { get { return 0; } }

        /// <summary>
        /// The binary operation for the int monoid is addition
        /// 
        /// Monoid property: MAppend(13, MZero) == 13
        /// </summary>
        /// <param name="a">Any value from the monoid int</param>
        /// <param name="b">Any value from the monoid int</param>
        /// <returns>'a' + 'b'</returns>
        public int MAppend(int a, int b)
        {
            return a + b;
        }
    }

    /// <summary>
    /// A monoid for sequences whereby MZero is an empty sequence and MAppend is concatenation
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence</typeparam>
    public sealed class EnumerableMonoid<T> : IMonoid<IEnumerable<T>>
    {
        /// <summary>
        /// As this represents a mathematical capability and not a value, there should not be more than
        /// one of these things ever.
        /// </summary>
        public static IMonoid<IEnumerable<T>> Only = new EnumerableMonoid<T>(); 

        private EnumerableMonoid()
        {
        }

        /// <summary>
        /// The identity value for the enumerable monoid is 'Enumerable.Empty'
        /// </summary>
        public IEnumerable<T> MZero { get { return Enumerable.Empty<T>(); } }

        /// <summary>
        /// The binary operation for enumerables is concatenation
        /// 
        /// Monoid property: MAppend(new [] {1,2,3}, MZero) == [1, 2, 3]
        /// </summary>
        /// <param name="t1">Any value from the monoid enumerable</param>
        /// <param name="t2">Any value from the monoid enumerable</param>
        /// <returns>'t1'.Concat('t2')</returns>
        public IEnumerable<T> MAppend(IEnumerable<T> t1, IEnumerable<T> t2)
        {
            return t1.Concat(t2);
        }
    }

    public sealed class BooleanAndMonoid : IMonoid<bool>
    {
        public static IMonoid<bool> Only = new BooleanAndMonoid();

        private BooleanAndMonoid()
        {
        }

        public bool MZero { get { return true; } }

        public bool MAppend(bool t1, bool t2)
        {
            return t1 && t2;
        }
    }

    public sealed class FuncMonoid<T> : IMonoid<Func<T, T>>
    {
        public static IMonoid<Func<T, T>> Only = new FuncMonoid<T>();

        private FuncMonoid()
        {
        }

        public Func<T, T> MZero { get { return BasicFunctions.Identity; } }
        public Func<T, T> MAppend(Func<T, T> f, Func<T, T> g)
        {
            return g.Compose(f);
        }
    }

    public sealed class ConsListMonoid<T> : IMonoid<IConsList<T>>
    {
        public static IMonoid<IConsList<T>> Only = new ConsListMonoid<T>();

        private ConsListMonoid() { } 

        public IConsList<T> MZero { get { return ConsList.Nil<T>(); } }
        public IConsList<T> MAppend(IConsList<T> t1, IConsList<T> t2)
        {
            return t1.Concat(t2);
        }
    }
}
