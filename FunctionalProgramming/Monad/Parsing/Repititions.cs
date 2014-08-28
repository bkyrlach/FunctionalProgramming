using System;

namespace FunctionalProgramming.Monad.Parsing
{
    /// <summary>
    /// An algebraic representation of the concept of repititions
    /// </summary>
    public abstract class Repititions
    {
        /// <summary>
        /// Pattern matching function to disambiguate reptition values
        /// </summary>
        /// <typeparam name="T">The type of result to unify to</typeparam>
        /// <param name="zeroOrMoreReps">An expression to be evaluated if this represents an unbounded repition with no minimum</param>
        /// <param name="oneOrMoreReps">An expression to be evaluated if this represents an unbounded repitition with at least one repitition</param>
        /// <param name="nReps">An expression to be evaluated if this represents fixed N repititions</param>
        /// <returns></returns>
        public abstract T Match<T>(Func<T> zeroOrMoreReps, Func<T> oneOrMoreReps, Func<int, T> nReps);
    }

    /// <summary>
    /// A value that represents something that repeats zero or more times
    /// </summary>
    public sealed class ZeroOrMoreRepititions : Repititions
    {
        /// <summary>
        /// This is a singleton as there should only be one inhabitant of this type
        /// </summary>
        public static ZeroOrMoreRepititions Only = new ZeroOrMoreRepititions();

        /// <summary>
        /// Constructor is private to prevent arbitrary instatiations
        /// </summary>
        private ZeroOrMoreRepititions()
        {

        }

        public override T Match<T>(Func<T> zeroOrMoreReps, Func<T> oneOrMoreReps, Func<int, T> nReps)
        {
            return zeroOrMoreReps();
        }

        public override string ToString()
        {
            return "ZeroOrMoreRepititions";
        }
    }

    /// <summary>
    /// A value that represents something that repeats one or more times
    /// </summary>
    public sealed class OneOrMoreRepititions : Repititions
    {
        /// <summary>
        /// This is a singleton as there should only be one inhabitant of this type
        /// </summary>
        public static OneOrMoreRepititions Only = new OneOrMoreRepititions();

        /// <summary>
        /// Constructor is private to prevent arbitary instantiations
        /// </summary>
        private OneOrMoreRepititions()
        {

        }

        public override T Match<T>(Func<T> zeroOrMoreReps, Func<T> oneOrMoreReps, Func<int, T> nReps)
        {
            return oneOrMoreReps();
        }

        public override string ToString()
        {
            return "OneOrMoreRepititions";
        }
    }

    /// <summary>
    /// Represents a fixed number of repititions
    /// </summary>
    public sealed class NRepititions : Repititions
    {
        private readonly int _n;

        public NRepititions(int n)
        {
            _n = n;
        }

        public override T Match<T>(Func<T> zeroOrMoreReps, Func<T> oneOrMoreReps, Func<int, T> nReps)
        {
            return nReps(_n);
        }

        public override string ToString()
        {
            return string.Format("NRepititions({0})", _n);
        }
    }

}
