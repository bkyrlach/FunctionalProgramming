using FunctionalProgramming.Monad;
using NUnit.Framework;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public class MaybeTests
    {
        private static IMaybe<int> SafeDivide(int i)
        {
            return i%2 == 0
                ? (i/2).ToMaybe()
                : Maybe.Nothing<int>();
        }
            
        [Test]
        public void TestSelectMany()
        {
            var result = from a in SafeDivide(10)
                from b in SafeDivide(a)
                select b;

            Assert.AreEqual(Maybe.Nothing<int>(), result);
        }
    }
}
