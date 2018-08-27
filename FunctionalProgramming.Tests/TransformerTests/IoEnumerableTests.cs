using System.Collections.Generic;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestClass]
    public sealed class IoEnumerableTests
    {
        [DataTestMethod]
        [DataRow(new[] { 1, 2, 3 }, new[] { 2, 3, 4 }, 3)]
        public void TestEffectualLoop(IEnumerable<int> inputSequence, IEnumerable<int> expectedResult,
            int expectedEffectualResult)
        {
            var actualEffectualResult = 0;
            var actual =
                (from i in inputSequence.ToIoEnumerable()
                from _ in Io.Apply(() => actualEffectualResult += 1).ToIoEnumerable()
                select i + 1).Out.UnsafePerformIo();

            Assert.AreEqual(expectedEffectualResult, actualEffectualResult);
            Assert.AreEqual(expectedResult, actual);
        }
    }
}
