using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using F = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Monad.Parsing
{
    /// <summary>
    /// This interface represents a parser combinator. Parser combinators parse TInput to TOutput, and offer composition
    /// through monadic operations.
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TOutput">The type of result that will be generated</typeparam>
    public interface IParser<TInput, TOutput>
    {
        /// <summary>
        /// Apply takes a sequence of TInput and generated an IParseResult. The implementation determines how 
        /// the parsing will take place, as well as whether or not the input is consumed.
        /// </summary>
        /// <param name="input">The sequence of input to attempt to parse</param>
        /// <returns>A parse result that represents the successful parsing of input into a TOutput, or the failure to do so</returns>
        IParseResult<TInput, TOutput> Apply(IEnumerable<TInput> input);

        /// <summary>
        /// Lifts a parser 'TInput => 'TOutput to the parser 'TInput => IEnumerable 'TOutput
        /// This parser only fails if the sequence cannot be matched at all. Otherwise, it will continue to match
        /// until the end of the input sequence, or the input sequence no longer matches.
        /// </summary>
        /// <returns>The lifted parser</returns>
        IParser<TInput, IEnumerable<TOutput>> Repeat();

        /// <summary>
        /// Combines this parser with another parser such that this ~ otherParser, the resultant parser
        /// now giving a result tuple combining the two results.
        /// </summary>
        /// <typeparam name="T">The output type of the other parser</typeparam>
        /// <param name="otherParser">The parser to combine with this parser</param>
        /// <returns>A parser that returns a tuple of the results of this parser and otherParser</returns>
        IParser<TInput, Tuple<TOutput, T>> FollowedBy<T>(IParser<TInput, T> otherParser);

        /// <summary>
        /// Combines this parser with another parser such that this ~> otherParser, the resultant parser
        /// will only yield the result from the parser on the right, while still requiring that both parsers match
        /// against the input.
        /// </summary>
        /// <typeparam name="T">The output type of the other parser</typeparam>
        /// <param name="keeper">The parser whos output will be kept</param>
        /// <returns>A new parser that returns the parse result of keeper only if both parsers succeed</returns>
        IParser<TInput, T> FollowedByDiscardingLeft<T>(IParser<TInput, T> keeper);

        /// <summary>
        /// Lifts a parser 'TInput => 'TOutput tp the parser 'TInput => 'IMaybe 'TOutput
        /// This parser doesn't fail, but instead yields a None 'TOutput on a failure result, or a Just 'TOutput
        /// on success.
        /// </summary>
        /// <returns>The lifted parser</returns>
        IParser<TInput, IMaybe<TOutput>> Possibly();

        /// <summary>
        /// Combines this parser with another parser such that the first parser to match is the result that will be
        /// utilized. First, this parser will attempt to match the input sequence. If that succeeds, then that is the
        /// result that is returned. If it fails, the second parser will attempt to match the input sequence. That
        /// result will be returned on failure or success.
        /// </summary>
        /// <param name="otherParser">The parser to try if this parser fails.</param>
        /// <returns>A parser that combines this parser with the other parser, trying them in sequence</returns>
        IParser<TInput, TOutput> Or(IParser<TInput, TOutput> otherParser);

        /// <summary>
        /// Transforms this parser into a parser that doesn't consume the matched sequence from the input, allowing
        /// another parser to match on that input.
        /// </summary>
        /// <returns>A parser that doesn't consume the matched input</returns>
        IParser<TInput, TOutput> WithoutConsuming();
    }

    /// <summary>
    /// Combines two parsers parser1 ~ parser2 to generate a result Tuple 'TOut1 'TOut2
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TOut1">The result type of parser1</typeparam>
    /// <typeparam name="TOut2">The result type of parser2</typeparam>
    public class CombinationParser<TInput, TOut1, TOut2> : Parser<TInput, Tuple<TOut1, TOut2>>
    {
        private readonly IParser<TInput, TOut1> _p1;
        private readonly IParser<TInput, TOut2> _p2;

        public CombinationParser(IParser<TInput, TOut1> p1, IParser<TInput, TOut2> p2)
        {
            _p1 = p1;
            _p2 = p2;
        }

        public override IParseResult<TInput, Tuple<TOut1, TOut2>> Apply(IEnumerable<TInput> reader)
        {
            return _p1.Apply(reader).Match(
                success: (val1, r1) => _p2.Apply(r1).Match<IParseResult<TInput, Tuple<TOut1, TOut2>>>(
                    success: (val2, r2) => new SuccessResult<TInput, Tuple<TOut1, TOut2>>(new Tuple<TOut1, TOut2>(val1, val2), r2),
                    failure: (error2, r2) => new FailureResult<TInput, Tuple<TOut1, TOut2>>(error2, r2)),
                failure: (error1, r1) => new FailureResult<TInput, Tuple<TOut1, TOut2>>(error1, r1));
        }
    }

    /// <summary>
    /// An abstract parser that implements all of the combination and modification operators.
    /// </summary>
    /// <typeparam name="TInput">The type of input to be parsed</typeparam>
    /// <typeparam name="TOutput">The output generated by this parser</typeparam>
    public abstract class Parser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public abstract IParseResult<TInput, TOutput> Apply(IEnumerable<TInput> reader);

        public IParser<TInput, Tuple<TOutput, T1>> FollowedBy<T1>(IParser<TInput, T1> otherParser)
        {
            return new CombinationParser<TInput, TOutput, T1>(this, otherParser);
        }

        public IParser<TInput, T> FollowedByDiscardingLeft<T>(IParser<TInput, T> keeper)
        {
            return new DiscardingPreviousParser<TInput, T, TOutput>(this, keeper);
        }

        public IParser<TInput, IEnumerable<TOutput>> Repeat()
        {
            return new RepeatingParser<TInput, TOutput>(this);
        }

        public IParser<TInput, IMaybe<TOutput>> Possibly()
        {
            return new MaybeParser<TInput, TOutput>(this);
        }

        public IParser<TInput, TOutput> Or(IParser<TInput, TOutput> otherParser)
        {
            return new ConditionalParser<TInput, TOutput>(this, otherParser);
        }

        public IParser<TInput, TOutput> WithoutConsuming()
        {
            return new NonConsumingParser<TInput, TOutput>(this);
        }
    }

    /// <summary>
    /// This parser doesn't consume tokens from the input on a successful match.
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TOutput">The output generated by this parser</typeparam>
    public class NonConsumingParser<TInput, TOutput> : Parser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _parser;

        public NonConsumingParser(IParser<TInput, TOutput> parser)
        {
            _parser = parser;
        }

        public override IParseResult<TInput, TOutput> Apply(IEnumerable<TInput> reader)
        {
            return _parser.Apply(reader).Match<IParseResult<TInput, TOutput>>(
                success: (output, inputs) => new SuccessResult<TInput, TOutput>(output, reader),
                failure: (s, inputs) => new FailureResult<TInput, TOutput>(s, reader));
        }
    }

    /// <summary>
    /// This parser combines two parsers parser1 ~> parser2 such that the output from parser1 is discarded, but the
    /// input sequence it matches is still required.
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TOutput">The type of output generated by this parser</typeparam>
    /// <typeparam name="TDiscard">The type of output generated by the parser that we wish to discard</typeparam>
    public class DiscardingPreviousParser<TInput, TOutput, TDiscard> : Parser<TInput, TOutput>
    {
        private readonly IParser<TInput, TDiscard> _discardedParser;
        private readonly IParser<TInput, TOutput> _keptParser;

        public DiscardingPreviousParser(IParser<TInput, TDiscard> discardedParser, IParser<TInput, TOutput> keptParser)
        {
            _discardedParser = discardedParser;
            _keptParser = keptParser;
        }

        public override IParseResult<TInput, TOutput> Apply(IEnumerable<TInput> reader)
        {
            return _discardedParser.Apply(reader).Match(
                success: (discard, inputs) => _keptParser.Apply(inputs).Match<IParseResult<TInput, TOutput>>(
                    success: (output, enumerable) => new SuccessResult<TInput, TOutput>(output, enumerable),
                    failure: (keepError, enumerable) => new FailureResult<TInput, TOutput>(keepError, enumerable)),
                failure: (discaredError, inputs) => new FailureResult<TInput, TOutput>(discaredError, inputs));
        }
    }

    /// <summary>
    /// This parser doesn't fail, but rather generates Just 'TOutput or None based on success or failure, respectively
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TOutput">The type of output that this parser will generated, lifted to IMaybe</typeparam>
    public class MaybeParser<TInput, TOutput> : Parser<TInput, IMaybe<TOutput>>
    {
        private readonly IParser<TInput, TOutput> _parser;

        public MaybeParser(IParser<TInput, TOutput> parser)
        {
            _parser = parser;
        }

        public override IParseResult<TInput, IMaybe<TOutput>> Apply(IEnumerable<TInput> reader)
        {
            return _parser.Apply(reader).Match(
                success: (output, reader1) => new SuccessResult<TInput, IMaybe<TOutput>>(output.ToMaybe(), reader1),
                failure: (s, reader1) => new SuccessResult<TInput, IMaybe<TOutput>>(MaybeExtensions.Nothing<TOutput>(), reader));
        }
    }

    /// <summary>
    /// This parser matches an input sequence one or more times, generating a TOutput per match, and failing once
    /// end of input is reached, or a match failure occurs.
    /// </summary>
    /// <typeparam name="TInput">The type of input to be matched on</typeparam>
    /// <typeparam name="TOutput">The type of output generated by this parser, lifted to IEnumerable</typeparam>
    public class RepeatingParser<TInput, TOutput> : Parser<TInput, IEnumerable<TOutput>>
    {
        private readonly IParser<TInput, TOutput> _parser;
        public RepeatingParser(IParser<TInput, TOutput> parser)
        {
            _parser = parser;
        }

        public override IParseResult<TInput, IEnumerable<TOutput>> Apply(IEnumerable<TInput> reader)
        {
            IParseResult<TInput, IEnumerable<TOutput>> retval;
            var results = Enumerable.Empty<TOutput>().ToList();
            var r = reader;
            var done = false;
            while (r.Any() && !done)
            {
                _parser.Apply(r).Match(
                    success: (output, reader1) =>
                    {
                        r = reader1;
                        results.Add(output);
                        return 0;
                    },
                    failure: (s, reader1) =>
                    {
                        done = true;
                        r = reader1;
                        return 0;
                    });
            }
            if (results.Any())
            {
                retval = new SuccessResult<TInput, IEnumerable<TOutput>>(results, r);
            }
            else
            {
                retval = new FailureResult<TInput, IEnumerable<TOutput>>("No repetitions found.", r);
            }
            return retval;
        }
    }

    /// <summary>
    /// This parser matches on a single TInput value, specified by either a constant value or a predicate
    /// function, and yields the matched input.
    /// </summary>
    /// <typeparam name="TInput">The type of input to parse, yielding a value of this type as output on success</typeparam>
    public class ElementaryParser<TInput> : Parser<TInput, TInput>
    {
        private readonly string _expectation;
        private readonly Func<TInput, bool> _predicate;

        /// <summary>
        /// This constructor creates an instance of ElementaryParser that matches on a constant, by lifting that constant
        /// to a predicate function.
        /// </summary>
        /// <param name="constant"></param>
        public ElementaryParser(TInput constant)
            : this(constant.ToString(), input => input.Equals(constant))
        {
        }

        /// <summary>
        /// This constructor creates an instance of ElementaryParser that matches based on a predicate.
        /// </summary>
        /// <param name="expectation"></param>
        /// <param name="predicate"></param>
        public ElementaryParser(string expectation, Func<TInput, bool> predicate)
        {
            _expectation = expectation;
            _predicate = predicate;
        }

        public override IParseResult<TInput, TInput> Apply(IEnumerable<TInput> reader)
        {
            return F.If<IParseResult<TInput, TInput>>(reader.Any() && _predicate(reader.First()),
                () => new SuccessResult<TInput, TInput>(reader.First(), reader.Skip(1)),
                () =>
                    new FailureResult<TInput, TInput>(
                        string.Format("Expected {0} but got {1}", _expectation, reader.Any() ? reader.First().ToString() : "{empty sequence}"), reader));
        }
    }

    /// <summary>
    /// This parser matches on the end of the input sequence, and yields Unit.
    /// </summary>
    /// <typeparam name="TInput">The type of input to parse</typeparam>
    public class EndOfInputParser<TInput> : Parser<TInput, Unit>
    {
        public override IParseResult<TInput, Unit> Apply(IEnumerable<TInput> reader)
        {
            return F.If<IParseResult<TInput, Unit>>(reader.Any(),
                () =>
                    new FailureResult<TInput, Unit>("Excepted {empty seqeunce} but found {non empty sequence}", reader),
                () => new SuccessResult<TInput, Unit>(Unit.Only, reader));
        }
    }

    /// <summary>
    /// This parser lifts a parser 'TInput => 'TOutput to a parser 'TInput => 'TResult using the provided 
    /// morphism 'TOutput => 'TResult
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TOutput">The type of output of the original parser</typeparam>
    /// <typeparam name="TResult">The type of output of the lifted parser</typeparam>
    public class MappingParser<TInput, TOutput, TResult> : Parser<TInput, TResult>
    {
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Func<TOutput, TResult> _f;

        public MappingParser(IParser<TInput, TOutput> parser, Func<TOutput, TResult> f)
        {
            _parser = parser;
            _f = f;
        }

        public override IParseResult<TInput, TResult> Apply(IEnumerable<TInput> reader)
        {
            return _parser.Apply(reader).Match<IParseResult<TInput, TResult>>(
                success: (output, reader1) => new SuccessResult<TInput, TResult>(_f(output), reader1),
                failure: (s, reader1) => new FailureResult<TInput, TResult>(s, reader1));
        }
    }

    /// <summary>
    /// This parser combines two parsers such that parser1 | parser2, yielding a parser that will
    /// first attempt to match the input using parser1, and, if failing, will then try to match
    /// the input using parser2.
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TOutput">The output generated by this parser</typeparam>
    public class ConditionalParser<TInput, TOutput> : Parser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _a;
        private readonly IParser<TInput, TOutput> _b;

        public ConditionalParser(IParser<TInput, TOutput> a, IParser<TInput, TOutput> b)
        {
            _a = a;
            _b = b;
        }

        public override IParseResult<TInput, TOutput> Apply(IEnumerable<TInput> reader)
        {
            return _a.Apply(reader).Match(
                success: (output, inputs) => new SuccessResult<TInput, TOutput>(output, inputs),
                failure: (aError, inputs) => _b.Apply(reader).Match<IParseResult<TInput, TOutput>>(
                    success: (output, enumerable) => new SuccessResult<TInput, TOutput>(output, enumerable),
                    failure: (bError, enumerable) => new FailureResult<TInput, TOutput>(bError, enumerable)));
        }
    }

    /// <summary>
    /// Static class that contains monadic operations for parser combinators.
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Lifts a moprhism 'TOutput => 'TResult from the category universe to the category IParser, and applies
        /// it to the provided parser.
        /// </summary>
        /// <typeparam name="TInput">The type of input being parsed</typeparam>
        /// <typeparam name="TOutput">The type of result generated by the parser</typeparam>
        /// <typeparam name="TResult">The type of result generated by the lifted morphism applied to the parser</typeparam>
        /// <param name="parser">The parser to apply the lifted morphism to</param>
        /// <param name="f">The morphism to be lifted</param>
        /// <returns>The parser with the lifted morphism applied</returns>
        public static IParser<TInput, TResult> Select<TInput, TOutput, TResult>(this IParser<TInput, TOutput> parser,
            Func<TOutput, TResult> f)
        {
            return new MappingParser<TInput, TOutput, TResult>(parser, f);
        }
    }
}
