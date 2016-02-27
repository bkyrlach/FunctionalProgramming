using System;
using System.Threading;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Streaming;
using FunctionalProgramming.Tests.Util;
using NUnit.Framework;
using Process = FunctionalProgramming.Streaming.Process;

namespace FunctionalProgramming.Tests.StreamingTests
{
    [TestFixture]
    public sealed class StreamingTests
    {
        [Test]
        public void TestProcess()
        {
            var expected = new[] {1, 2, 3, 4, 5};
            var p1 = Process.Apply(1, 2, 3, 4, 5);
            var result = p1.RunLog();
            Assert.IsTrue(TestUtils.AreEqual(expected, result));
        }

        [Test]
        public void TestSink()
        {
            var p1 = Process.Apply(1, 2, 3, 4, 5);
            var p2 = Process.Sink<int>(n => Console.WriteLine(n));
            var p3 = p1.Pipe(p2);
            p3.Run();
        }

        private static Process1<IEither<T, T2>, IEither<T, T2>> Tee<T, T2>(bool right = true)
        {
            return new Await1<IEither<T, T2>, IEither<T, T2>>(
                () => BasicFunctions.EIf(right, () => default(T2), () => default(T)),
                errorOrValue => errorOrValue.Match<Process1<IEither<T, T2>, IEither<T, T2>>>(
                    left: e => new Halt1<IEither<T, T2>, IEither<T, T2>>(e),
                    right: either => new Emit1<IEither<T, T2>, IEither<T, T2>>(either, Tee<T, T2>(!right))));
        }

        [Test]
        public void TestTee()
        {
            var p1 = Process.Apply(1, 2, 3, 4, 5);
            var p2 = Process.Apply("a", "b", "c", "d", "e");
            var p3 = Process.Sink<IEither<int, string>>(either => Console.WriteLine(either));
            var p4 = p1.Tee(p2, Tee<int, string>());

            var results = p4.Pipe(p3).Run();            
        }

        [Test]
        public void TestSelectMany()
        {
            var p1 = Process.Apply(1);
            var p2 = Process.Apply(2);
            var p3 = p1.SelectMany(n => p2);
            var results = p3.RunLog();
            results.ForEach(Console.WriteLine);
        }

        public Process<int> DelayCount(int cur)
        {
            return Await<int>.Create(() =>
            {
                Thread.Sleep(1000);
                return cur;
            }, either => either.Match<Process<int>>(
                left: ex => new Halt<int>(ex),
                right: n => new Emit<int>(n, DelayCount(n + 1))));
        }

        [Test]
        public void TestWye()
        {
            var stopAfter30 = Process.Delay(3000);
            var count = DelayCount(1);
            var combined = Process.Wye(count, stopAfter30);
            var sink = Process.Sink<IEither<int, Unit>>(either => Console.WriteLine(either));
            var result = combined.Pipe(sink);
            result.Run();
        }

        [Test]
        public void TestRepeatUntil()
        {
            var x = 0;
            var p1 = Process.Eval(() => x++, new Emit<int>(x)).RepeatUntil(() =>
            {
                Console.WriteLine($"{x} > 9? {x > 9}");
                return x > 9;
            });
            var results = p1.RunLog();
        }
    }
}
