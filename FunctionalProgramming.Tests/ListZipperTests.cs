﻿using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests
{
    [TestClass]
    public sealed class ListZipperTests
    {
        [TestMethod]
        public void TestEmptySet()
        {
            var expected = 1.LiftList();
            var zipper = ConsListZipper<int>.ToZipper(ConsList.Nil<int>());
            var result = zipper.Set(1).ToList();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestUnmodifiedList()
        {
            var expected = (new[] {1, 2, 3, 4, 5}).ToConsList();
            var zipper = ConsListZipper<int>.ToZipper(expected);
            var result = zipper.ToList();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
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
