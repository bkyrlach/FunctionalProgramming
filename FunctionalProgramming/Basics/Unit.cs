using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalProgramming.Basics
{
    /// <summary>
    /// This class represents the Unit value from typed lambda calculus. It is used to represent the return value or input
    /// value of functions that perform side effects.
    /// </summary>
    public sealed class Unit
    {
        public static readonly Unit Only = new Unit();

        private Unit()
        {
        }
    }
}
