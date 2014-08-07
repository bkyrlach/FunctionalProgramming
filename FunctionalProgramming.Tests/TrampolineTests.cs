using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FunctionalProgramming.Monad;
using NUnit.Framework;

using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public class TrampolineTests
    {
        [Test]       
        public void TestBigRecursion()
        {
            var sw = new Stopwatch();
            sw.Start();
            var xs = Repeat("bob", 10000000).Run();
            Console.WriteLine(sw.ElapsedMilliseconds / 1000d);
            Assert.IsTrue(Even(xs).Run());
            Console.WriteLine(sw.ElapsedMilliseconds / 1000d);
            sw.Stop();
        }

        [Test]
        public void TestBigSelect()
        {
            var s2 = new Stopwatch();
            s2.Start();
            var build = Enumerable.Repeat("bob", 10000000).ToList();
            Console.WriteLine(s2.ElapsedMilliseconds / 1000d);
            Assert.AreEqual(3, build.Select(x => x.Length).ToList().First());
            Console.WriteLine(s2.ElapsedMilliseconds / 1000d);
            s2.Stop();
            var sw = new Stopwatch();
            sw.Start();
            var xs = Repeat("bob", 10000000).Run();
            Console.WriteLine(sw.ElapsedMilliseconds / 1000d);
            Assert.AreEqual(3,xs.Select(x => x.Length).Head.GetOrError(() => new Exception("Failed to get head of list")));
            Console.WriteLine(sw.ElapsedMilliseconds / 1000d);
            sw.Stop();
        }

        private static Trampoline<IConsList<T>> Repeat<T>(T t, int n)
        {
            return n <= 0
                ? new Done<IConsList<T>>(ConsListExtensions.Nil<T>())
                : new More<IConsList<T>>(() => Repeat(t, n - 1)).Select(ts => t.Cons(ts));
        }

        private static Trampoline<bool> Even<T>(IConsList<T> xs)
        {
            return xs.Match<Trampoline<bool>>(
                cons: (h, t) => new More<bool>(() => Odd(t)),
                nil: () => new Done<bool>(true));
        }

        private static Trampoline<bool> Odd<T>(IConsList<T> xs)
        {
            return xs.Match<Trampoline<bool>>(
                cons: (h, t) => new More<bool>(() => Even(t)),
                nil: () => new Done<bool>(false));
        }
    }
}
