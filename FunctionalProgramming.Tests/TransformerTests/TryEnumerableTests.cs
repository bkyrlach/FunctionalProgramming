using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Outlaws;
using FunctionalProgramming.Monad.Transformer;
using FunctionalProgramming.Tests.Util;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.TransformerTests
{
    [TestFixture]
    class TryEnumerableTests
    {
        [Test]
        public void TestTryEnumerable()
        {
            var expected = new[] {2, 3, 4, 5, 6};
            var initital = new[] {1, 2, 3, 4, 5};
            var @try = Try.Attempt(() => initital.AsEnumerable()).ToTryEnumerable();
            var temp =
                (from num in @try
                select num + 1)
                .Out();
            var result = temp.Match(
                success: BasicFunctions.Identity,
                failure: ex => Enumerable.Empty<int>());

            Assert.IsTrue(TestUtils.AreEqual(result, expected));

        }
    }
}
