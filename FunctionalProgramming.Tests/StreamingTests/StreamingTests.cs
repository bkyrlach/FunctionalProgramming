using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Streaming;
using NUnit.Framework;
using Process = FunctionalProgramming.Streaming.Process;

namespace FunctionalProgramming.Tests.StreamingTests
{
    [TestFixture]
    public sealed class StreamingTests
    {
        [Test]
        public void TestConcat()
        {
            var expected = new[] {1, 2};
            var _1 = new Emit<int, int>(1);
            var _2 = new Emit<int, int>(2);
            var process = _1.Concat(() => _2);
            var result = process.RunLog();
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestOneProcess()
        {
            var transducer = Process.Lift<int, int>(x => x*2);
            var source = new Emit<int, int>(1);
            var process = source.Pipe(transducer);
            var result = process.Run();
            Assert.AreEqual(2, result);
        }

        [Test]
        public void TestTenProcess()
        {
            var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Select(x => x * 2);
            var transducer = Process.Lift<int, int>(x => x*2);
            var source = new Emit<int, int>(1, new Emit<int, int>(2, new Emit<int, int>(3, new Emit<int, int>(4, new Emit<int, int>(5, new Emit<int, int>(6, new Emit<int, int>(7, new Emit<int, int>(8, new Emit<int, int>(9, new Emit<int, int>(10))))))))));
            var process = source.Pipe(transducer);
            var result = process.RunLog();
            result.ToList().ForEach(Console.WriteLine);
            Assert.AreEqual(expected, result);
        }

        private Process<IEither<T, T2>, IEither<T, T2>> Tee<T, T2>(bool right = true)
        {
            return new Await<IEither<T, T2>, IEither<T, T2>>(
                Io.Apply(() => BasicFunctions.EIf(right, () => default(T2), () => default(T))),
                errorOrValue => errorOrValue.Match<Process<IEither<T, T2>, IEither<T, T2>>>(
                    left: e => new Halt<IEither<T, T2>, IEither<T, T2>>(e),
                    right: either => new Emit<IEither<T, T2>, IEither<T, T2>>(either, Tee<T, T2>(!right))));
        }

        private Process<T, T> ListToProcess<T>(IEnumerable<T> xs)
        {
            return (xs.Any()
                ? new Await<T, T>(Io.Apply(() => xs.First()), either => either.Match<Process<T, T>>(
                    left: e => new Halt<T, T>(e),
                    right: x => new Emit<T, T>(x, ListToProcess(xs.Skip(1)))))
                : Process.Halt1<T, T>());
        }

        [Test]
        public void TestTee()
        {
            var nums = ListToProcess(new[] {1, 2, 3, 4, 5});
            var letters = ListToProcess(new[] {"a", "b", "c", "d", "e"});
            var combined = nums.Tee(nums, Tee<int, int>());
            var results = combined.RunLog();
            results.ToList().ForEach(Console.WriteLine);
        }        

        [Test]
        public void TestBoringNonDet()
        {
            var nums = ListToProcess(new[] { 1, 2, 3, 4, 5 });
            var nums2 = ListToProcess(new[] {1, 2, 3});
            var letters = ListToProcess(new[] { "a", "b", "c", "d", "e" });
            var process = Process.Wye(nums, nums2);
            var results = process.RunLog();
            results.ToList().ForEach(Console.WriteLine);
        }

        private static readonly Random R = new Random();

        private Process<T, T> Delayed<T>(IEnumerable<T> ints)
        {
            return ints.Any() 
                ? Process.Delay<T, T>((uint)R.Next(25, 101)).Concat(() => new Await<T, T>(
                    Io.Apply(() => ints.First()), 
                    either => either.Match<Process<T, T>>(
                        left: e => new Halt<T, T>(e),
                        right: x => new Emit<T, T>(x))).Concat(() => Delayed(ints.Skip(1))))
                : Process.Halt1<T, T>();
        }

        [Test]
        public void TestNonDet()
        {
            var nums = Delayed(Enumerable.Range(1, 100));
            var sink = Process.Sink<int>(n => Console.WriteLine(n));
            var stopAfter = Process.Delay<int, int>(30000);
            var process = Process.Wye(nums.Pipe(sink), stopAfter);
            process.Run();
        }
    }
}
