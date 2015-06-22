using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestFixture]
    public sealed class EitherEnumerabletests
    {
        private static bool AreEqual<T>(IEnumerable<T> xs, IEnumerable<T> ys)
        {
            var xsList = xs.ToList();
            var ysList = ys.ToList();
            return
                xsList.Count == ysList.Count &&
                xs.Zip(ys, Tuple.Create).Aggregate(true, (r, pair) => r && pair.Item1.Equals(pair.Item2));
        }

        [Test]
        public void TestIt()
        {
            var initial = new[] {1, 2, 3};
            var expected = initial.Select(x => x + 1);
            var xs = new EitherEnumerable<string, int>(initial.AsRight<string, IEnumerable<int>>());
            var result = xs.Select(x => x + 1).Out();

            Assert.IsTrue(result.Match(
                left: err => false,
                right: actual => AreEqual(expected, actual)));
        }

        [Test]
        public void Test2()
        {
            var initial = new[] {1, 2, 3};
            var next = new[] {4, 5, 6};

            var val1 = new EitherEnumerable<string, int>(initial.AsRight<string, IEnumerable<int>>());
            var val2 = new EitherEnumerable<string, int>(next.AsRight<string, IEnumerable<int>>());

            var result =
                (from x in val1
                 from y in val2
                 select x + y)
                .Out();

            var expected =
                from x in initial
                from y in next
                select x + y;

            Assert.IsTrue(result.Match(
                left: l => false,
                right: actual => AreEqual(expected, actual)));
        }
    }
}
