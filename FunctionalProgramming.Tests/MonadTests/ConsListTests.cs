﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FunctionalProgramming.Monad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests.MonadTests
{
    [TestClass]
    public class ConsListTests
    {
        private static IConsList<T> MkList<T>(IEnumerable<T> ts)
        {
            return ts.Reverse().Aggregate(ConsList.Nil<T>(), (lst, t) => t.Cons(lst));
        }

        [TestMethod]
        public void TestSingleItem()
        {
            var xs = 1.LiftList();
            Console.WriteLine(xs);
        }

        [DataTestMethod]
        [DataRow(new int[] { }, 1)]
        [DataRow(new int[] { 1, 2, 3 }, 2)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, 3)]
        public void TestAsEnumerable(int[] xs, int fake)
        {
            var expected = xs.AsEnumerable();
            var list = MkList(xs);
            Console.WriteLine(list);
            var result = list.AsEnumerable();
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow(new int[] { }, new int[] { }, 1)]
        [DataRow(new int[] { }, new int[] { 1, 2, 3 }, 2)]
        [DataRow(new int[] { 1, 2, 3 }, new int[] { }, 3)]
        [DataRow(new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 }, 4)]
        public void TestConcat(int[] fst, int[] snd, int fake)
        {
            var expected = fst.Concat(snd);
            var list1 = MkList(fst);
            var list2 = MkList(snd);
            var result = list1.Concat(list2).AsEnumerable();
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow(new[] { 1, 2, 3 }, 1)]
        public void TestAsEnumerable(IEnumerable<int> expected, int fake)
        {
            var actual = 1.Cons(2.Cons(3.Cons(ConsList.Nil<int>()))).AsEnumerable();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAsEnumerable()
        {
            var expected = Enumerable.Range(0, 10000);
            IConsList<int> xs = expected.ToConsList();
            var sw = Stopwatch.StartNew();
            var enumerable = xs.AsEnumerable();
            Assert.AreEqual(enumerable, expected);
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
