using FunctionalProgramming.Monad;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.MonadTests
{
    [TestFixture]
    public sealed class EitherTests
    {
        [Test]
        public void TestOr()
        {
            var e1 = "boo".AsLeft<string, int>();
            var e2 = 2.AsRight<string, int>();

            Assert.AreEqual(2, e1.Or(e2).Match(
                left: err => -1,
                right: val => val));

            var e3 = "baz".AsLeft<string, int>();

            Assert.AreEqual("baz", e1.Or(e3).Match(
                left: err => err,
                right: val => "not the value you're looking for"));
        }
    }
}
