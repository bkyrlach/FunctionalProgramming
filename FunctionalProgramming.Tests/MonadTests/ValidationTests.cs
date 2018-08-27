using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;
using FunctionalProgramming.Monad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalProgramming.Tests.MonadTests
{
    [TestClass]
    public sealed class ValidationTests
    {
        private class Person
        {
            public static Person MakePerson(string firstName, string lastName, int age)
            {
                return new Person()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Age = age
                };
            }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }

        private class Failure
        {
            public int ErrorCode { get; set; }
            public string Message { get; set; }
        }

        [DataTestMethod]
        [DataRow(null, "Kyrlach", 33, new [] {1})]
        [DataRow("Ben", "Kyrlach", 33, new int [] {})]
        [DataRow("Bo", "Jackson", -1, new [] {3, 2})]
        public void TestApplicative(string firstName, string lastName, int age, int[] expectedErrors)
        {
            var m = EnumerableMonoid<Failure>.Only;
            Func<string, string, int, Person> makePerson = Person.MakePerson;

            var personOrError =
                ValidateStringNotNull(firstName).SelectMany(ValidateNameLength)
                    .Select(makePerson.Curry())
                    .Apply(ValidateStringNotNull(lastName).SelectMany(ValidateNameLength), m)
                    .Apply(ValidateAgeInRange(age), m);

            var errorCodes = personOrError.Match(
                success: p => ConsList.Nil<int>(),
                failure: errors => errors.Select(error => error.ErrorCode).ToConsList());
            var expectedErrorList = expectedErrors.ToConsList();

            Assert.IsTrue(expectedErrors.ToConsList().Equals(errorCodes));
        }

        private Validation<IEnumerable<Failure>, string> ValidateNameLength(string name)
        {
            return name.Length >= 3 && name.Length <= 50
                ? name.AsSuccessWithFailureList<Failure, string>()
                : new Failure()
                {
                    ErrorCode = 3,
                    Message = "Name must be between 3 and 50 characters."
                }.AsFailureList<Failure, string>();
        }

        private Validation<IEnumerable<Failure>, string> ValidateStringNotNull(string s)
        {
            return s == null
                ? new Failure
                {
                    ErrorCode = 1,
                    Message = "Input must not be null"
                }.AsFailureList<Failure, string>()
                : s.AsSuccessWithFailureList<Failure, string>();
        }

        private Validation<IEnumerable<Failure>, int> ValidateAgeInRange(int age)
        {
            return age > 0 && age < 99
                ? age.AsSuccessWithFailureList<Failure, int>()
                : new Failure()
                {
                    ErrorCode = 2,
                    Message = "Age must be within the range 1 < age < 99"
                }.AsFailureList<Failure, int>();
        }
    }
}
