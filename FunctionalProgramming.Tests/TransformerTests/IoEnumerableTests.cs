using System.Collections.Generic;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestFixture]
    public sealed class IoEnumerableTests
    {
        [TestCase(new[] { 1, 2, 3 }, new[] { 2, 3, 4 }, 3)]
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
