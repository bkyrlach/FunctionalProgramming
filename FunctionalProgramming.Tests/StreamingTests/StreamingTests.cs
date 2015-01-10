using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;
using FunctionalProgramming.Streaming;
using NUnit.Framework;

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
                () => BasicFunctions.EIf(right, () => default(T2), () => default(T)),
                errorOrValue => errorOrValue.Match<Process<IEither<T, T2>, IEither<T, T2>>>(
                    left: e => new Halt<IEither<T, T2>, IEither<T, T2>>(e),
                    right: either => new Emit<IEither<T, T2>, IEither<T, T2>>(either, Tee<T, T2>(!right))));
        }

        private Process<T, T> ListToProcess<T>(IEnumerable<T> xs)
        {
            return BasicFunctions.If<Process<T, T>>(xs.Any(),
                () => new Await<T, T>(() => xs.First(), either => either.Match<Process<T, T>>(
                    left: e => new Halt<T, T>(e),
                    right: x => new Emit<T, T>(x, ListToProcess(xs.Skip(1))))),
                Process.Halt1<T, T>);
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

        private Process<T, IEither<T1, T2>> NonDet<T, T1, T2>(Process<T, T1> p1, Process<T, T2> p2)
        {
            return p1.Match(
                halt: e => new Halt<T, IEither<T1, T2>>(e),
                emit: (h, t) => new Emit<T, IEither<T1, T2>>(h.AsLeft<T1, T2>(), NonDet(t, p2)),
                cont: cw => new Cont<T, IEither<T1, T2>>(() => NonDet(cw, p2)),
                eval: (effect, next) => null,
                await: (reql, recvl) => p2.Match<Process<T, IEither<T1, T2>>>(
                    halt: e => new Halt<T, IEither<T1, T2>>(e),
                    emit: (h, t) => new Emit<T, IEither<T1, T2>>(h.AsRight<T1, T2>(), NonDet(new Await<T, T1>(reql, recvl), t)),
                    cont: cw => new Cont<T, IEither<T1, T2>>(() => NonDet(p1, cw)), 
                    eval: (effect, next) => null,
                    await: (reqr, recvr) =>
                    {
                        var isRight = false;
                        return new Await<T, IEither<T1, T2>>(() =>
                        {
                            var t1 = Task.Run(reql);
                            var t2 = Task.Run(reqr);
                            var task = Task.WhenAny(new[] {t1, t2});
                            var result = task.Await();
                            isRight = result.Equals(reqr);
                            return result.Await();
                        }, x => x.Match(
                            left: e => new Halt<T, IEither<T1, T2>>(e),
                            right: i => BasicFunctions.If(isRight,
                                () => NonDet(new Await<T, T1>(reql, recvl), recvr(i.AsRight<Exception, T>())),
                                () => NonDet(recvl(i.AsRight<Exception, T>()), new Await<T, T2>(reqr, recvr)))));
                    }));
        }

        [Test]
        public void TestBoringNonDet()
        {
            var nums = ListToProcess(new[] { 1, 2, 3, 4, 5 });
            var letters = ListToProcess(new[] { "a", "b", "c", "d", "e" });
            var process = NonDet(nums, nums);
            var results = process.RunLog();
            results.ToList().ForEach(Console.WriteLine);
        }
    }
}
