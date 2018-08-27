using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestClass]
    public sealed class StateEnumerableTests
    {
        [DataTestMethod]
        [DataRow(new[] {1, 2, 3}, new[] {2, 3, 4}, 3)]
        public void TestSumState(IEnumerable<int> testSequence, IEnumerable<int> expectedResult, int expectedState)
        {
            var program =
                (from i in testSequence.ToStateEnumerable<int, int>()
                from _ in State.Mod<int>(sum => sum + 1).ToStateEnumerable()
                select i + 1).Out;

            var actual = program.Run(0);
            Console.WriteLine(actual.Item2.Select(n => n.ToString()).Aggregate((r, s) => string.Format("{0}, {1}", r, s)));
            Assert.AreEqual(expectedState, actual.Item1);
            Assert.AreEqual(expectedResult, actual.Item2);
        }
    }
}
