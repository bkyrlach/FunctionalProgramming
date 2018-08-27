using System;
using System.Linq;
using System.Threading;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Streaming;
using FunctionalProgramming.Tests.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Process = FunctionalProgramming.Streaming.Process;

namespace FunctionalProgramming.Tests.StreamingTests
{
    [TestClass]
    public sealed class StreamingTests
    {
        [TestMethod]
        public void TestProcess()
        {
            var expected = new[] {1, 2, 3, 4, 5};
            var p1 = Process.Apply(1, 2, 3, 4, 5);
            var result = p1.RunLog();
            Assert.IsTrue(TestUtils.AreEqual(expected, result));
        }

        [TestMethod]
        public void TestConcat()
        {
            var p1 = Process.Emit(1);
            var p2 = Process.Emit(2);
            var p3 = p1.Concat(() => p2);
            p3.RunLog().ToList().ForEach(Console.WriteLine);
        }

        [TestMethod]
        public void TestSink()
        {
            var p1 = Process.Apply(1, 2, 3, 4, 5);
            var p2 = Process.Lift1<int, Unit>(n => { Console.WriteLine(n); return Unit.Only; });
            var p3 = p2.Concat(() => p2);
            var p4 = p1.Pipe(p3);
            p4.Run();
        }

        [TestMethod]
        public void TestTee()
        {
            var p1 = Process.Apply(1, 2, 3, 4, 5).Select(n => n.ToString());
            var p2 = Process.Apply("a", "b", "c", "d", "e");
            var p3 = Process.Interleave(p1, p2);
            var p4 = Process.Sink<string>(s => Console.WriteLine(s));

            var results = p3.Pipe(p4).Run();            
        }

        [TestMethod]
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

        [TestMethod]
        public void TestWye()
        {
            var stopAfter30 = Process.Delay(3000);
            var count = DelayCount(1);
            var combined = Process.Wye(count, stopAfter30);
            var sink = Process.Sink<IEither<int, Unit>>(either => Console.WriteLine(either));
            var result = combined.Pipe(sink);
            result.Run();
        }

        [TestMethod]
        public void TestRepeatUntil()
        {
            //var x = 0;
            //var p1 = Process.Eval(() => x++, new Emit<int>(x)).RepeatUntil(() =>
            //{
            //    Console.WriteLine($"{x} > 9? {x > 9}");
            //    return x > 9;
            //});
            //var results = p1.RunLog();
        }
    }
}
