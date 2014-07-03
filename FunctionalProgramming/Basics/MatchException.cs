using System;

namespace FunctionalProgramming.Basics
{
    /// <summary>
    /// Represents an exception that is thrown as a result of someone adding a value to a given algebra without the requisite disambiguation function
    /// </summary>
    public class MatchException : Exception
    {
        /// <summary>
        /// Constructor that takes the type information to aid in debugging which Set is incompletely mapped
        /// </summary>
        /// <param name="sumType">The type of set in question</param>
        /// <param name="attemptedToMatch">A value (which is a type) from the set that is not represented</param>
        public MatchException(Type sumType, Type attemptedToMatch)
            : base(string.Format("Match for {0} non exhaustive. Attempted to match against {1}!", sumType, attemptedToMatch))
        {

        }
    }
}
