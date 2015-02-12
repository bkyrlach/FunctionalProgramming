using System;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestFixture]
    public sealed class IoStateEnumerableTests
    {
        [Test]
        public void BasicSequenceTest()
        {
            var xs = new[] {1, 2, 3, 4, 5};
            var expectedResult = new[] {2, 3, 4, 5, 6};
            var expectedState = 5;

            var program =
                from x in xs.ToIoStateEnumerable<int, int>()
                from _1 in State.Mod<int>(i => i + 1).ToIoStateEnumerable()
                from _2 in Io.Apply(() => Console.WriteLine(x)).ToIoStateEnumerable<int, Unit>()
                select x + 1;

            var result = program.Out().UnsafePerformIo().Run(0);

            Assert.AreEqual(expectedState, result.Item1);
            Assert.AreEqual(expectedResult, result.Item2);
        }
    }
}
