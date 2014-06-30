using FunctionalProgramming.Monad;
using NUnit.Framework;

using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public class TrampolineTests
    {
        [Test]       
        public void TestBigRecursionNoTrampoline()
        {
            var xs = Repeat("bob", 1000000000).Run();
            Assert.IsTrue(Even(xs).Run());
        }

        private static Trampoline<IConsList<T>> Repeat<T>(T t, int n)
        {
            return n <= 0 ? (Trampoline<IConsList<T>>)new Done<IConsList<T>>(ConsListExtensions.Nil<T>()) : new More<IConsList<T>> (() => Repeat(t, n - 1).Select(ts => t.Cons(ts)));
        }
 

        private static Trampoline<bool> Even<T>(IConsList<T> xs)
        {
            return xs.Match<Trampoline<bool>>(
                cons: (h, t) => new More<bool>(() => Odd(t)),
                nil: () => new Done<bool>(true));
        }

        private static Trampoline<bool> Odd<T>(IConsList<T> xs)
        {
            return xs.Match<Trampoline<bool>>(
                cons: (h, t) => new More<bool>(() => Even(t)),
                nil: () => new Done<bool>(false));
        }
    }
}
