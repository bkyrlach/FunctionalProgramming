using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalProgramming.Tests.Util
{
    public static class TestUtils
    {
        public static bool AreEqual<T>(IEnumerable<T> xs, IEnumerable<T> ys)
        {
            var xsList = xs.ToList();
            var ysList = ys.ToList();
            return
                xsList.Count == ysList.Count &&
                xsList.Zip(ysList, Tuple.Create).Aggregate(true, (r, pair) => r && pair.Item1.Equals(pair.Item2));
        }
    }
}
