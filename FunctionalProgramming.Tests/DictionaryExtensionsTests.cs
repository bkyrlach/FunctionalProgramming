using System.Collections.Generic;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using NUnit.Framework;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public sealed class DictionaryExtensionsTests
    {
        [Test]
        public void TestMissingKey()
        {
            var data = new Dictionary<string, string>
            {
                {"a", "1"},
                {"b", "2"},
                {"d", "4"}
            };

            var expected = Maybe.Nothing<string>();
            var actual = data.Get("c");
            Assert.AreEqual(expected, actual);
        }
    }
}
