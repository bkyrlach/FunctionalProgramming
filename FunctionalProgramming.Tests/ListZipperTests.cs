using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using NUnit.Framework;
using System;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    class ListZipperTests
    {
        [Test]
        public void TestEmptySet()
        {
            var expected = 1.LiftList();
            var zipper = ConsListZipper<int>.ToZipper(ConsListOps.Nil<int>());
            var result = zipper.Set(1).ToList();
            Assert.AreEqual(expected,result);
        }

        [Test]
        public void TestUnmodifiedList()
        {
            var expected = (new[] {1, 2, 3, 4, 5}).ToConsList();
            var zipper = ConsListZipper<int>.ToZipper(expected);
            var result = zipper.ToList();
            Assert.AreEqual(expected,result);
        }

        [Test]
        public void TestLast()
        {
            var initial = (new[] {1, 2, 3, 4, 5}).ToConsList();
            var expected = (new[] {1, 2, 3, 4, 6}).ToConsList();
            var zipper = ConsListZipper<int>.ToZipper(initial);
            var last = zipper.Last();
            var lastVal = last.Get().GetOrError(() => new Exception("Unable to get last value of zipper."));
            var result = last.Set(lastVal + 1).ToList();
            Assert.AreEqual(expected,result);
        }
    }
}
