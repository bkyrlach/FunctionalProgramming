using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.MonadTests
{
    [TestFixture]
    public class TaskTests
    {
        private static Task<int> DelayedInt(int n)
        {
            var r = new Random();
            var delay = r.Next(1500, 5000);
            return new Task<int>(() =>
            {
                Thread.Sleep(delay);
                return n;
            });
        }

        [Test]
        public void TestFMap()
        {
            var task = new Task<int>(() => 5).Select(n => n + 1);
            Assert.AreEqual(6, task.Result);
        }

        [Test]
        public void TestSequence()
        {
            var futureInts = Enumerable.Range(1, 5).Select(DelayedInt);

            var task = futureInts.Sequence();
            task.Result.ToList().ForEach(Console.WriteLine);
        }
    }
}
