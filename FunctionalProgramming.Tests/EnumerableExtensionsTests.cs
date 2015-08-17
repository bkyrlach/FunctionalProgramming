using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public sealed class EnumerableExtensionsTests
    {
        private static IMaybe<T> KeepIf<T>(Func<T, bool> predicate, T t)
        {
            return t.ToMaybe().Where(predicate);
        }

        [TestCase(new []{5,6,7,8}, true, new[]{5,6,7,8})]
        public void TestTraverseMaybe(int[] testData, bool isSome, int[] expected)
        {
            Func<int, bool> greaterThanFour = i => i > 4;
            var keepGreaterThanFour = FuncExtensions.Curry<Func<int, bool>, int, IMaybe<int>>(KeepIf)(greaterThanFour);
            var result = testData.Traverse(keepGreaterThanFour);
            Assert.AreEqual(isSome, !result.IsEmpty);
            if (isSome)
            {
                (result.Match(
                    just: actual => Io.Apply(() => Assert.AreEqual(expected, actual)),
                    nothing: () => Io.Apply(() => Assert.Fail("Expected to get a result for input {0} but got nothing instead", testData)))).UnsafePerformIo();
            }
        }

        [Test]
        public void TestTraverseIo()
        {
            var xs = new[] {1, 2, 3, 4, 5};
            var result = xs.Traverse(x => from _1 in Io.Apply(() => Console.WriteLine(x)) select x).UnsafePerformIo();
            Assert.AreEqual(xs, result);
        }


        private static StateIo<int, Unit> Count(int n)
        {
            return
                from _1 in State.Mod<int>(i => i + 1).ToStateIo()
                from _2 in Io.Apply(() => Console.WriteLine(n)).ToStateIo<int, Unit>()
                select Unit.Only;
        }

        [Test]
        public void TestTraverseStateIo()
        {
            var xs = new[] {1, 2, 3, 4, 5};

            var result = xs.Select(Count).Sequence().RunIo(0).UnsafePerformIo();
            Assert.AreEqual(5, result.Item1);           
        }
    }
}
