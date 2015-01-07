using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Outlaws;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.MonadTests
{
    [TestFixture]
    public sealed class TaskTests
    {
        [Test]
        public void TestUnit()
        {
            var task = 1.FromResult();
            Assert.AreEqual(task.Result, 1);
        }

        [Test]
        public void TestSelect()
        {
            var task = 1.FromResult().Select(n => n + 1);
            Assert.AreEqual(task.Result, 2);
        }

        [Test]
        public void TestSelectMany()
        {
            var t1 = 1.FromResult();
            var t2 = 2.FromResult();
            var task = t1.SelectMany(a => t2.Select(b => a + b));
            Assert.AreEqual(task.Result, 3);
        }

        [Test]
        public void TestLinq()
        {
            var task =
                from a in 1.FromResult()
                from b in 2.FromResult()
                select a + b;

            Assert.AreEqual(task.Result, 3);
        }

        [Test]
        public void TestSequence()
        {
            var expected = new[] {1, 2, 3};
            var task = expected.Select(n => n.FromResult()).Sequence();
            Assert.AreEqual(task.Result, expected);
        }

        private static async Task<int> DelayedInt(int delay, int n)
        {
            await Task.Delay(delay);
            Console.WriteLine("Generating {0} after {1} delay", n, delay);
            return n;
        }

        [Test]
        public void TestDelay()
        {
            var task =
                from a in 1.FromResult()
                from b in DelayedInt(10000, 3)
                select a + b;
            
            Assert.AreEqual(task.Result, 4);
        }

        private static readonly Random R = new Random();

        private static Task<int> Rng()
        {            
            return DelayedInt(R.Next(5000, 10001), R.Next(1, 20));
        }

        [Test]
        public void TestWhenAny()
        {
            var sw = Stopwatch.StartNew();

            var taskList = new HashSet<Task<int>>
            {
                Rng(),
                Rng(),
                Rng(),
                Rng(),
                Rng(),
                Rng(),
                Rng(),
                Rng(),
                Rng(),
                Rng()
            };

            while (taskList.Any())
            {
                var task = Task.WhenAny(taskList);
                var originalTask = task.Result;
                taskList.Remove(originalTask);
                Console.WriteLine("Got {0} with {1} on the clock", originalTask.Result, sw.ElapsedMilliseconds);
            }

            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
