using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalProgramming.Monad.Parsing
{
    public abstract class Repititions
    {
        public abstract T Match<T>(Func<T> zeroOrMoreReps, Func<T> oneOrMoreReps, Func<int, T> nReps);
    }

    public sealed class ZeroOrMoreRepititions : Repititions
    {
        public static ZeroOrMoreRepititions Only = new ZeroOrMoreRepititions();

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

    public sealed class OneOrMoreRepititions : Repititions
    {
        public static OneOrMoreRepititions Only = new OneOrMoreRepititions();

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
