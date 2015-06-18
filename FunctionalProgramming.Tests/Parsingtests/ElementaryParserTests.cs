﻿using System;
using System.Diagnostics;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Parsing;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

namespace FunctionalProgramming.Tests.Parsingtests
{
    [TestFixture]
    public sealed class ElementaryParserTests
    {
        [Test]
        public void TestSingleElement()
        {
            const char input = 'a';
            var a = Parser.Elem(input);
            var result = a.Parse('a'.ToString());
            Assert.AreEqual(input.ToString(), result.Match(
                left: err => err,
                right: c => c.ToString()));
        }

        [Test]
        public void TestManyElements()
        {
            var bs = Enumerable.Repeat('b', 10000);
            var bString = bs.MkString();
            var bParser = bs.Traverse(Parser.Elem);
            var result = bParser.Parse(bString).Select(chars => chars.MkString());
            Assert.AreEqual(bString, result.Match(
                left: err => err,
                right: res => res));
        }

        [Test]
        public void TestOr()
        {
            var a = Parser.Elem('a');
            var b = Parser.Elem('b');
            var ab = a.Or(b);

            var res1 = ab.Parse("a").Match(
                left: err => err,
                right: c => c.ToString());

            Assert.AreEqual("a", res1);

            var res2 = ab.Parse("b").Match(
                left: err => err,
                right: c => c.ToString());

            Assert.AreEqual("b", res2);

            var digit = Parser.ElemWhere<char>(c => c >= '0' && c <= '9', "[0-9]");
            var twoDigits = digit.Repeat(2).Select(chars => chars.MkString());
            var twoOrOneDigits = twoDigits.Or(digit.Select(c => c.ToString()));

            var res3 = twoOrOneDigits.Parse("12").Match(
                left: err => err,
                right: val => val);

            Assert.AreEqual("12", res3);

            var res4 = twoOrOneDigits.Parse("1").Match(
                left: err => err,
                right: val => val);

            Assert.AreEqual("1", res4);
        }
    }
}