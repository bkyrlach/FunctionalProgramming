namespace FunctionalProgramming.Basics
{
    /// <summary>
    /// This class represents the Unit value from typed lambda calculus. It is used to represent the return value or input
    /// value of functions that perform side effects.
    /// </summary>
    public sealed class Unit
    {
        /// <summary>
        /// There can only be on inhabitant of the type Unit
        /// </summary>
        public static readonly Unit Only = new Unit();

        private Unit()
        {
        }
    }
}
