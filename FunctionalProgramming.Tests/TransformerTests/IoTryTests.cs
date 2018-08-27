using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;
using FunctionalProgramming.Monad.Transformer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestClass]
    class IoTryTests
    {
        [TestMethod]
        public void TestComposition()
        {
            var p1 = Io.Apply(() => Try.Pure(1)).ToIoTry();
            var p2 = Io.Apply(() => Try.Pure(2)).ToIoTry();
            var p3 = Io.Apply(() => Try.Pure(2)).ToIoTry();

            var program =
                from a in p1
                from b in p2
                from c in p3
                select a + b + c;

            var result = program.Out.UnsafePerformIo().Match(
                success: BasicFunctions.Identity,
                failure: ex => -1);

            Assert.AreEqual(6, result);
        }
    }
}
