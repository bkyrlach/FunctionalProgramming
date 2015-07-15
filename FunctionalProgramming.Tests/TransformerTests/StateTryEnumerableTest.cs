using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestFixture]
    class StateTryEnumerableTest
    {
        private static bool AreEqual<T>(IEnumerable<T> xs, IEnumerable<T> ys)
        {
            var xsList = xs.ToList();
            var ysList = ys.ToList();
            return
                xsList.Count == ysList.Count &&
                xsList.Zip(ysList, Tuple.Create).Aggregate(true, (r, pair) => r && pair.Item1.Equals(pair.Item2));
        }

        [Test]
        public void TestStateTryEnumerable()
        {
            var expected = new[] {2, 3, 4, 5, 6, 7};
            var xs = new[] {1, 2, 3, 4, 5, 6};
            var @try = xs.Traverse(x => Try.Attempt(() => x));
            var state = @try.Insert<int, Try<IEnumerable<int>>>();
            var initial = state.ToStateTryEnumerable();
            var program =
                (from x in initial
                 from addend in State.Get<int, int>(BasicFunctions.Identity).ToStateTryEnumerable()
                 select x + addend)
                .Out();

            var actual = program.Eval(1).Match(
                success: BasicFunctions.Identity,
                failure: exception => Enumerable.Empty<int>());

            Assert.IsTrue(AreEqual(expected, actual));
        }
    }
}
