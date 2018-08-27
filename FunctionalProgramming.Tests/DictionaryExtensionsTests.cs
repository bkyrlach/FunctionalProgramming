using System.Collections.Generic;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests
{
    [TestClass]
    public sealed class DictionaryExtensionsTests
    {
        [TestMethod]
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
