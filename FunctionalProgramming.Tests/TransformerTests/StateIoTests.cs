using System;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestFixture]
    class StateIoTests
    {
        [Test]
        public void TestIt()
        {
            var pgm =
                from n in State.Get<int, int>(BasicFunctions.Identity).ToStateIo()
                from _1 in Io.Apply(() => Console.WriteLine("Before modification {0}", n)).ToStateIo<int, Unit>()
                from _2 in (n + 1).Put().ToStateIo()
                from o in State.Get<int, int>(BasicFunctions.Identity).ToStateIo()
                from _3 in Io.Apply(() => Console.WriteLine("After modifications {0}", o)).ToStateIo<int, Unit>()
                select _3;

            pgm.EvalIo(5).UnsafePerformIo();
        }
    }
}
