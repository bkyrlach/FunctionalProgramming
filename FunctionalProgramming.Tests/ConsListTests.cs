using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Monad;
using NUnit.Framework;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public class ConsListTests
    {
        private static IConsList<T> MkList<T>(IEnumerable<T> ts)
        {
            return ts.Reverse().Aggregate(ConsListOps.Nil<T>(), (lst, t) => t.Cons(lst));
        }
            
        [TestCase(new int[] { }, 1)]
        [TestCase(new int[] { 1, 2, 3 }, 2)]
        [TestCase(new int[] { 1, 2, 3, 4, 5, 6 }, 3)]
        public void TestAsEnumerable(int[] xs, int fake)
        {
            var expected = xs.AsEnumerable();
            var result = MkList(xs).AsEnumerable();
            Assert.AreEqual(expected, result);
        }

        [TestCase(new int[] { }, new int[] { }, 1)]
        [TestCase(new int[] { }, new int[] { 1, 2, 3 }, 2)]
        [TestCase(new int[] { 1, 2, 3 }, new int[] { }, 3)]
        [TestCase(new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 }, 4)]
        public void TestConcat(int[] fst, int[] snd, int fake)
        {
            var expected = fst.Concat(snd);
            var list1 = MkList(fst);
            var list2 = MkList(snd);
            var result = list1.Concat(list2).AsEnumerable();
            Assert.AreEqual(expected, result);
        }
    }
}
