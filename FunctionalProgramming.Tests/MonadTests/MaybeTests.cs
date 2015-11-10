using System;
using System.Linq;
using FunctionalProgramming.Monad;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.MonadTests
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
        public void TestSelectJust()
        {
            var result = 5.ToMaybe().Select(n => n == 5).GetOrElse(() => false);
            Assert.IsTrue(result);
        }
            
        [Test]
        public void TestSelectManyNothingResult()
        {
            var expected = Maybe.Nothing<int>();
            var result = from a in SafeDivide(10)
                from b in SafeDivide(a)
                select b;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestSelectManyJustResult()
        {
            var expected = 5.ToMaybe();
            var result = from a in SafeDivide(20)
                         from b in SafeDivide(a)
                         select b;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestEqualityJust()
        {
            var expected = 5.ToMaybe();
            var result = 4.ToMaybe().Select(n => n + 1);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestInequalityJust()
        {
            var expected = 4.ToMaybe();
            var result = 4.ToMaybe().Select(n => n + 1);
            Assert.AreNotEqual(expected, result);
        }

        [Test]
        public void TestJustNothingInequality()
        {
            var expected = 4.ToMaybe();
            var result = Maybe.Nothing<int>();
            Assert.AreNotEqual(expected, result);
        }

        [Test]
        public void TestKeepSome()
        {
            var expected = new[] {1, 2, 3, 4};
            var test = new[]
            {
                1.ToMaybe()
            };

            var result = test.KeepSome();
            result.ToList().ForEach(Console.WriteLine);
        }
    }
}
