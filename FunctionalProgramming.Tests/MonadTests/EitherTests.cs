using FunctionalProgramming.Monad;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.MonadTests
{
    [TestFixture]
    public sealed class EitherTests
    {
        [Test]
        public void TestOr()
        {
            var e1 = "boo".AsLeft<string, int>();
            var e2 = 2.AsRight<string, int>();

            Assert.AreEqual(2, e1.Or(e2).Match(
                left: err => -1,
                right: val => val));

            var e3 = "baz".AsLeft<string, int>();

            Assert.AreEqual("baz", e1.Or(e3).Match(
                left: err => err,
                right: val => "not the value you're looking for"));
        }

        private static IEither<string, string> ValidateString(string s)
        {
            IEither<string, string> result;
            if (s == null)
            {
                result = "Input was null".AsLeft<string, string>();
            }
            else if (s.Length > 5)
            {
                result = "Input length greater than 5".AsLeft<string, string>();
            }
            else
            {
                result = s.AsRight<string, string>();
            }
            return result;
        }

        private static IEither<string, int> ValidateInt(int i)
        {
            IEither<string, int> result;
            if (i < 1)
            {
                result = "Number less than recommended value".AsLeft<string, int>();
            }
            else if (i > 9)
            {
                result = "Number greater than recommended value".AsLeft<string, int>();
            }
            else
            {
                result = i.AsRight<string, int>();
            }
            return result;
        }

        private static IEither<string, bool> ValidateBool(bool? b)
        {
            return b.HasValue ? b.Value.AsRight<string, bool>() : "Boolean value not present".AsLeft<string, bool>();
        }
            
        [TestCase("a", 1, true, true)]
        public void TestApplicative(string foo, int bar, bool? fooBar, bool expected)
        {
            var dto = ValidateString(foo)
                .With(ValidateInt(bar))
                .With(ValidateBool(fooBar)).Apply(Dto.Apply);

            Assert.AreEqual(expected, dto.Match(
                left: error => false,
                right: val => true));
        }
    }

    class Dto
    {
        public static Dto Apply(string foo, int bar, bool fooBar)
        {
            return new Dto
            {
                Foo = foo,
                Bar = bar,
                FooBar = fooBar
            };
        }

        public string Foo { get; set; }
        public int Bar { get; set; }
        public bool FooBar { get; set; }
    }
}
