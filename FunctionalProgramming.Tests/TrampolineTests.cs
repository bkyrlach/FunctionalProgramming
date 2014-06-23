using System;
using FunctionalProgramming.Monad;
using NUnit.Framework;

using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public class TrampolineTests
    {
        [Test]       
        public void TestBigRecursionNoTrampoline()
        {
            //var xs = Repeat("bob", 10000);
            //try
            //{
            //    BadEven(xs);
            //    Assert.Fail("This should blow the stack.");
            //}
            //catch (StackOverflowException soe)
            //{

            //}
            //Assert.IsTrue(true);
        }

        private static IConsList<T> Repeat<T>(T t, int n)
        {
            return BF.If(n <= 0,
                ConsListExtensions.Nil<T>,
                () => t.Cons(Repeat(t, n - 1)));
        }

        private static bool BadEven<T>(IConsList<T> xs)
        {
            return xs.Match(
                cons: (h, t) => BadOdd(t),
                nil: () => true);
        }

        private static bool BadOdd<T>(IConsList<T> xs)
        {
            return xs.Match(
                cons: (h, t) => BadEven(t),
                nil: () => false);
        }
    }
}
