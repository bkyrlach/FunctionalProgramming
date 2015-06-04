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
                from x in xs.ToIoStateEnumerable<Tuple<int, int>, int>()
                from _1 in State.Mod<Tuple<int, int>>(pair => Tuple.Create(pair.Item1 + 1, pair.Item2)).ToIoStateEnumerable()
                where x > 3
                from _2 in State.Mod<Tuple<int, int>>(pair => Tuple.Create(pair.Item1, pair.Item2 + 1)).ToIoStateEnumerable()
                from _3 in Io.Apply(() => Console.WriteLine(x)).ToIoStateEnumerable<Tuple<int, int>, Unit>()
                select x + 1;

            var result = program.Out().UnsafePerformIo().Run(Tuple.Create(0, 0));

            Assert.AreEqual(expectedState, result.Item1);
            Assert.AreEqual(expectedResult, result.Item2);
        }
    }
}
