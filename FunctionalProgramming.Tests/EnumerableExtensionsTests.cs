using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace FunctionalProgramming.Tests
{
    [TestFixture]
    public sealed class EnumerableExtensionsTests
    {
        private static IMaybe<T> KeepIf<T>(Func<T, bool> predicate, T t)
        {
            return t.ToMaybe().Where(predicate);
        }

        [TestCase(new[] { 5, 6, 7, 8 }, true, new[] { 5, 6, 7, 8 })]
        public void TestTraverseMaybe(int[] testData, bool isSome, int[] expected)
        {
            Func<int, bool> greaterThanFour = i => i > 4;
            var keepGreaterThanFour = FuncExtensions.Curry<Func<int, bool>, int, IMaybe<int>>(KeepIf)(greaterThanFour);
            var result = testData.Traverse(keepGreaterThanFour);
            Assert.AreEqual(isSome, !result.IsEmpty);
            if (isSome)
            {
                result.Match(
                    just: actual => Io.Apply(() => Assert.AreEqual(expected, actual)),
                    nothing: () => Io.Apply(() => Assert.Fail("Expected to get a result for input {0} but got nothing instead", testData))).UnsafePerformIo();
            }
        }

        [Test]
        public void TestTraverseIo()
        {
            var xs = new[] { 1, 2, 3, 4, 5 };
            var result = xs.Traverse(x => from _1 in Io.Apply(() => Console.WriteLine(x)) select x).UnsafePerformIo();
            Assert.AreEqual(xs, result);
        }


        private static StateIo<int, Unit> Count(int n)
        {
            return
                from _1 in State.Mod<int>(i => i + 1).ToStateIo()
                from _2 in Io.Apply(() => Console.WriteLine(n)).ToStateIo<int, Unit>()
                select Unit.Only;
        }

        [Test]
        public void TestTraverseStateIo()
        {
            var xs = new[] { 1, 2, 3, 4, 5 };

            var result = xs.Select(Count).Sequence().RunIo(0).UnsafePerformIo();
            Assert.AreEqual(5, result.Item1);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyEnumerableOfClassWithoutPredicateReturnsNothing()
        {
            var result = new List<TestUser>().MaybeFirst();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyEnumerableOfClassWithPredicateReturnsNothing()
        {
            var result = new List<TestUser>().MaybeFirst(x => x.Email == string.Empty);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleEnumerableOfClassWithoutPredicateReturnsSomething()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.MaybeFirst();
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleEnumerableOfClassWithPredicateReturnsSomething()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.MaybeFirst(x => x.Email == string.Empty);
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyQueryablOfClasseWithoutPredicateReturnsNothing()
        {
            var result = new List<TestUser>().AsQueryable().MaybeFirst();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyQueryableOfClassWithPredicateReturnsNothing()
        {
            var result = new List<TestUser>().AsQueryable().MaybeFirst(x => x.Email == string.Empty);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleQueryableOfClassWithoutPredicateReturnsSomething()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.AsQueryable().MaybeFirst();
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleQueryableOfClassWithPredicateReturnsSomething()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.AsQueryable().MaybeFirst(x => x.Email == string.Empty);
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyEnumerableOfPrimitiveWithoutPredicateReturnsNothing()
        {
            var result = new List<int>().MaybeFirst();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyEnumerableOfPrimitiveWithPredicateReturnsNothing()
        {
            var result = new List<int>().MaybeFirst(x => x == 1);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleEnumerableOfPrimitiveWithoutPredicateReturnsSomething()
        {
            var result = new List<int> { 1, 1 }.MaybeFirst();
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleEnumerableOfPrimitiveWithPredicateReturnsSomething()
        {
            var result = new List<int> { 1, 1 }.MaybeFirst(x => x == 1);
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyQueryablOfPrimitiveeWithoutPredicateReturnsNothing()
        {
            var result = new List<int>().AsQueryable().MaybeFirst();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_EmptyQueryableOfPrimitiveWithPredicateReturnsNothing()
        {
            var result = new List<int>().AsQueryable().MaybeFirst(x => x == 1);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleQueryableOfPrimitiveWithoutPredicateReturnsSomething()
        {
            var result = new List<int> { 1, 1 }.AsQueryable().MaybeFirst();
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeFirstTest_MultipleQueryableOfPrimitiveWithPredicateReturnsSomething()
        {
            var result = new List<int> { 1, 1 }.AsQueryable().MaybeFirst(x => x == 1);
            Assert.IsFalse(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyEnumerableOfClassWithoutPredicateReturnsNothing()
        {
            var result = new List<TestUser>().MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyEnumerableOfClassWithPredicateReturnsNothing()
        {
            var result = new List<TestUser>().MaybeSingle(x => x.Email == string.Empty);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleEnumerableOfClassWithoutPredicateReturnsNothing()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleEnumerableOfClassWithPredicateReturnsNothing()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.MaybeSingle(x => x.Email == string.Empty);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyQueryableOfClassWithoutPredicateReturnsNothing()
        {
            var result = new List<TestUser>().AsQueryable().MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyQueryableOfClassWithPredicateReturnsNothing()
        {
            var result = new List<TestUser>().AsQueryable().MaybeSingle(x => x.Email == string.Empty);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleQueryableOfClassWithoutPredicateReturnsNothing()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.AsQueryable().MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleQueryableOfClassWithPredicateReturnsNothing()
        {
            var result = new List<TestUser> { new TestUser { Email = string.Empty }, new TestUser { Email = string.Empty } }.AsQueryable().MaybeSingle(x => x.Email == string.Empty);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyEnumerableOfPrimitiveWithoutPredicateReturnsNothing()
        {
            var result = new List<int>().MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyEnumerableOfPrimitiveWithPredicateReturnsNothing()
        {
            var result = new List<int>().MaybeSingle(x => x == 1);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleEnumerableOfPrimitiveWithoutPredicateReturnsNothing()
        {
            var result = new List<int> { 1, 1 }.MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleEnumerableOfPrimitiveWithPredicateReturnsNothing()
        {
            var result = new List<int> { 1, 1 }.MaybeSingle(x => x == 1);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyQueryableOfPrimitiveWithoutPredicateReturnsNothing()
        {
            var result = new List<int>().AsQueryable().MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_EmptyQueryableOfPrimitiveWithPredicateReturnsNothing()
        {
            var result = new List<int>().AsQueryable().MaybeSingle(x => x == 1);
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleQueryableOfPrimitiveWithoutPredicateReturnsNothing()
        {
            var result = new List<int> { 1, 1 }.AsQueryable().MaybeSingle();
            Assert.IsTrue(result.IsEmpty);
        }

        [TestCase]
        public void MaybeSingleTest_MultipleQueryableOfPrimitiveWithPredicateReturnsNothing()
        {
            var result = new List<int> { 1, 1 }.AsQueryable().MaybeSingle(x => x == 1);
            Assert.IsTrue(result.IsEmpty);
        }

        public class TestUser : IPrincipal
        {
            public string Email { get; set; }

            public bool IsInRole(string role)
            {
                throw new NotImplementedException();
            }

            public IIdentity Identity => new GenericIdentity(Email);
        }
    }
}
