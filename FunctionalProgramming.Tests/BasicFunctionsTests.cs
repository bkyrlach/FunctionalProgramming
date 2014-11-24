using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using NUnit.Framework;
using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public sealed class BasicFunctionsTests
    {
        [Test]
        public void TestEIf()
        {
            var test1 = BF.EIf(false, () => "abc", () => 3);
            Assert.AreEqual("abc", test1.Match(left: n => n.ToString(), right: BF.Identity));
            var test2 = BF.EIf(false, () => test1, () => 19);
            Assert.AreEqual("abc", test2.Match(left: n => n.ToString(), right: BF.Identity));
            var test3 = BF.EIf(false, () => "abc", () => test1);
            Assert.AreEqual("abc", test3.Match(left: n => n.ToString(), right: BF.Identity));
        }

        [Test]
        public void TestTraverse()
        {
            var expected = new[] {1, 2, 3};
            var actual = expected.Traverse(n => Io<int>.Apply(() => n)).UnsafePerformIo();

            Assert.AreEqual(expected, actual);
        }
    }
}
