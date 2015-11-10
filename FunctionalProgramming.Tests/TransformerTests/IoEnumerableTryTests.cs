using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestFixture]
    public sealed class IoEnumerableTryTests
    {
        [Test]
        public void Test1()
        {
            var xs = Io.Apply(() => Enumerable.Range(0, 10000).Select(n => n%8).Select(n => Try.Attempt(() =>
                {
                    if (n == 0)
                    {
                        throw new Exception("Ruh roh");
                    }
                    return n;
                }))).ToIoEnumerableTry();

            var pgm = (from x in xs
                from _ in PutStrLn(x).ToIoEnumerableTry()
                select x + 3).Out;

            var res = pgm.UnsafePerformIo();
        }

        public static Io<Unit> PutStrLn(object s)
        {
            return Io.Apply(() => Console.WriteLine(s));
        }
    }   
}
