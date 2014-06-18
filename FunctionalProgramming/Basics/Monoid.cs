using System.Collections.Generic;
using System.Linq;

namespace FunctionalProgramming.Basics
{
    public interface IMonoid<T>
    {
        T MZero { get; }
        T MAppend(T t1, T t2);
    }

    public class EnumerableMonoid<T> : IMonoid<IEnumerable<T>>
    {
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
