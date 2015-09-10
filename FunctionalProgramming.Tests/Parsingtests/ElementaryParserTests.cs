using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Parsing;
using FunctionalProgramming.Monad.Transformer;
using NUnit.Framework;

using UnitParser = FunctionalProgramming.Monad.Transformer.StateEither<FunctionalProgramming.Monad.Parsing.ParserState<char>, string, FunctionalProgramming.Basics.Unit>;
using CharParser = FunctionalProgramming.Monad.Transformer.StateEither<FunctionalProgramming.Monad.Parsing.ParserState<char>, string, char>;
using StringParser = FunctionalProgramming.Monad.Transformer.StateEither<FunctionalProgramming.Monad.Parsing.ParserState<char>, string, string>;
using DictionaryParser = FunctionalProgramming.Monad.Transformer.StateEither<FunctionalProgramming.Monad.Parsing.ParserState<char>, string, FunctionalProgramming.Monad.IMaybe<System.Collections.Generic.IDictionary<string, string>>>;
using HardwareDescriptorParser = FunctionalProgramming.Monad.Transformer.StateEither<FunctionalProgramming.Monad.Parsing.ParserState<char>, string, FunctionalProgramming.Monad.IMaybe<FunctionalProgramming.Tests.Parsingtests.HardwareDescriptor>>;
using HardwareDescriptorsParser = FunctionalProgramming.Monad.Transformer.StateEither<FunctionalProgramming.Monad.Parsing.ParserState<char>, string, System.Collections.Generic.IEnumerable<FunctionalProgramming.Monad.IMaybe<FunctionalProgramming.Tests.Parsingtests.HardwareDescriptor>>>;

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
            var sw = Stopwatch.StartNew();
            var bs = Enumerable.Repeat('b', 1000000);
            bs.ToArray();
            Console.WriteLine(sw.ElapsedMilliseconds);
            var bString = bs.MkString();
            Console.WriteLine(sw.ElapsedMilliseconds);
            var bParser = bs.Traverse(Parser.Elem);
            Console.WriteLine("Traveresed time: {0}", sw.ElapsedMilliseconds);
            var result = bParser.Parse(bString).Select(chars => chars.MkString());
            Console.WriteLine(sw.ElapsedMilliseconds);
            Assert.AreEqual(bString, result.Match(
                left: err => err,
                right: res => res));
            Console.WriteLine(sw.ElapsedMilliseconds);
            var sw2 = Stopwatch.StartNew();
            Assert.IsFalse(bs.Any(c => c != 'b'));
            Console.WriteLine(sw2.ElapsedMilliseconds);
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

        [Test]
        public void TestSome()
        {
            var someAs = Parser.Elem('a').Many();
            var input = "";
            Assert.IsTrue(someAs.Parse(input).Match(
                right: r => true,
                left: err => false
            ));
        }

        private static readonly Func<char, bool> NotComma = c => ',' != c;
        private static readonly Func<char, bool> NotNewline = c => '\n' != c;
        private static readonly Func<char, bool> NotCarriageReturn = c => '\r' != c;

        private static readonly CharParser NotCrAndNewline = Parser.ElemWhere(NotNewline.And(NotCarriageReturn), "[^\\n^\\r]");
        private static readonly CharParser NotCommaAndCrAndNewline = Parser.ElemWhere(NotNewline.And(NotCarriageReturn).And(NotComma), "[^,^\\n^\\r]");
        private static readonly CharParser Comma = Parser.Elem(',');
        private static readonly UnitParser Newline = Parser.Elem('\n').Select(c => Unit.Only);
        private static readonly UnitParser Cr = Parser.Elem('\r').Select(c => Unit.Only);
        private static readonly StringParser RowPart = NotCommaAndCrAndNewline.Many().Select(list => list.MkString());

        private static readonly DictionaryParser HardwareProperties =
            NotCrAndNewline.Many()
                .Select(chrs => chrs.MkString())
                .Select(jsonString => jsonString.Any() ? jsonString : "{}")
                .Select(json => new Dictionary<string, string>().ToMaybe().Select(d => (IDictionary<string, string>)d));

        private static readonly UnitParser CrNl =
            from _1 in Cr
            from _2 in Newline
            select Unit.Only;

        private static readonly HardwareDescriptorParser Row = 
            (from id in RowPart
             from _1 in Comma
             from alias in RowPart
             from _2 in Comma
             from properties in HardwareProperties
             from _3 in CrNl.Or(Newline).Or(Parser.EoF<char>())
             select new HardwareDescriptor
             {
                 Id = id,
                 Alias = alias,
                 Properties = properties.Select(d => "{}")
             }).MakeOptional();

        private static IEnumerable<IMaybe<HardwareDescriptor>> ParseCsv(string csv)
        {
            var retval = new List<IMaybe<HardwareDescriptor>>();
            while (csv.Any())
            {
                var result = Row.ParseSome(csv);
                retval.Add(result.Item2.Match(
                    left: err => Maybe.Nothing<HardwareDescriptor>(),
                    right: BasicFunctions.Identity));
                csv = ParserState<char>.Data.Get(result.Item1).MkString().Substring((int)ParserState<char>.Index.Get(result.Item1));
            }
            return retval;
        }  

        [TestCase("1,,")]
        public void TestCsvParsing(string csv)
        {
            csv = Enumerable.Range(1, 10000).Select(n => string.Format("{0},,", n)).Aggregate((res, s) => res + "\n" + s);
            Console.WriteLine(ParseCsv(csv).Select(c => c.ToString()).Aggregate("", (s, hd) => s + "\n" + hd));
        }
    }

    public sealed class HardwareDescriptor
    {
        public string Id { get; set; }
        public string Alias { get; set; }
        public IMaybe<string> Properties { get; set; }

        public override string ToString()
        {
            return string.Format("HardwareDescriptor({0},{1},{2})", Id, Alias, Properties.GetOrElse(() => "{}"));
        }
    }
}
