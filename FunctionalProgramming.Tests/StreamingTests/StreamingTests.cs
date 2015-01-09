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
        public void TestOneProcess()
        {
            var transducer = Process.Lift<int, int>(x => x*2);
            var source = new Emit<int, int>(1);
            var process = source.Pipe(transducer);
            var result = process.Run();
            Assert.AreEqual(2, result.First());
        }

        [Test]
        public void TestTenProcess()
        {
            var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Select(x => x * 2);
            var transducer = Process.Lift<int, int>(x => x*2);
            var source = new Emit<int, int>(1, new Emit<int, int>(2, new Emit<int, int>(3, new Emit<int, int>(4, new Emit<int, int>(5, new Emit<int, int>(6, new Emit<int, int>(7, new Emit<int, int>(8, new Emit<int, int>(9, new Emit<int, int>(10))))))))));
            var process = source.Pipe(transducer);
            var result = process.Run();
            result.ToList().ForEach(Console.WriteLine);
            Assert.AreEqual(expected, result);
        }

        private Process<IEither<T, T2>, IEither<T, T2>> Tee<T, T2>(bool right = true)
        {
            return new Await<IEither<T, T2>, IEither<T, T2>>(
                new Task<IEither<T, T2>>(() => BasicFunctions.EIf(right, () => default(T2), () => default(T))),
                errorOrValue => errorOrValue.Match<Process<IEither<T, T2>, IEither<T, T2>>>(
                    left: e => new Halt<IEither<T, T2>, IEither<T, T2>>(e),
                    right: either => new Emit<IEither<T, T2>, IEither<T, T2>>(either, Tee<T, T2>(!right))));
        }

        private Process<T, T> ListToProcess<T>(IEnumerable<T> xs)
        {
            return BasicFunctions.If<Process<T, T>>(xs.Any(),
                () => new Await<T, T>(new Task<T>(() => xs.First()), either => either.Match<Process<T, T>>(
                    left: e => new Halt<T, T>(e),
                    right: x => new Emit<T, T>(x, ListToProcess(xs.Skip(1))))),
                () => new Halt<T, T>(new End()));
        }

        [Test]
        public void TestTee()
        {
            var nums = ListToProcess(new[] {1, 2, 3, 4, 5});
            var letters = ListToProcess(new[] {"a", "b", "c", "d", "e"});
            var combined = nums.Tee(nums, Tee<int, int>());
            var results = combined.Run();
            results.ToList().ForEach(Console.WriteLine);
        }

        private Process<T, IEither<T1, T2>> NonDet<T, T1, T2>(Process<T, T1> p1, Process<T, T2> p2)
        {
            return p1.Match(
                halt: e => new Halt<T, IEither<T1, T2>>(e),
                emit: (h, t) => new Emit<T, IEither<T1, T2>>(h.AsLeft<T1, T2>(), NonDet(t, p2)),
                await: (reql, recvl) => p2.Match<Process<T, IEither<T1, T2>>>(
                    halt: e => new Halt<T, IEither<T1, T2>>(e),
                    emit:
                        (h, t) =>
                            new Emit<T, IEither<T1, T2>>(h.AsRight<T1, T2>(), NonDet(new Await<T, T1>(reql, recvl), t)),
                    await: (reqr, recvr) =>
                    {
                        var isRight = false;
                        return new Await<T, IEither<T1, T2>>(new Task<T>(() =>
                        {
                            if (reql.Status == TaskStatus.Created)
                            {
                                reql.Start();   
                            }
                            if (reqr.Status == TaskStatus.Created)
                            {
                                reqr.Start();   
                            }                            
                            var task = Task.WhenAny(new[] {reql, reqr});
                            var result = task.Await();
                            isRight = result.Equals(reqr);
                            return result.Await();
                        }), x => x.Match(
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
            var results = process.Run();
            results.ToList().ForEach(Console.WriteLine);
        }
    }
}
