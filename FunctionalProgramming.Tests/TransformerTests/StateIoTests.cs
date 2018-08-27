using System;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestClass]
    class StateIoTests
    {
        [TestMethod]
        public void TestIt()
        {
            var pgm =
                from n in State.Get<int, int>(BasicFunctions.Identity).ToStateIo()
                from _1 in Io.Apply(() => Console.WriteLine("Before modification {0}", n)).ToStateIo<int, Unit>()
                from _2 in (n + 1).Put().ToStateIo()
                from o in State.Get<int, int>(BasicFunctions.Identity).ToStateIo()
                from _3 in Io.Apply(() => Console.WriteLine("After modifications {0}", o)).ToStateIo<int, Unit>()
                select _3;

            pgm.Out.Eval(5).UnsafePerformIo();
        }
    }
}
