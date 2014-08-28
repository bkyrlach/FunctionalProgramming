using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using FunctionalProgramming.Basics;
using System;
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
        IParseResult<TInput, TOutput> Apply(IStream<TInput> input);

        /// <summary>
        /// Lifts a parser 'TInput => 'TOutput to the parser 'TInput => IEnumerable 'TOutput
        /// This parser fails if the input sequence doesn't satisfy the required repitions.
        /// </summary>
        /// <param name="r">A repitition pattern</param>
        /// <returns>A parser that will match r repititions of the input sequence</returns>
        IParser<TInput, IConsList<TOutput>> Repeat(Repititions r);
        
        /// <summary>
        /// Lifts a parser 'TInput => 'TOutput to the parser 'TInput => IEnumerable 'TOutput
        /// where the input seqeunce is delimited
        /// 
        /// Note that the delimter values are discarded
        /// </summary>
        /// <typeparam name="T">The type of value parsed by the delimiter parser</typeparam>
        /// <param name="r">A value indicating how to repeat</param>
        /// <param name="delimiter">A parser that matches the delimeter</param>
        /// <returns>A parser that will match r repititions of the delimited input sequence</returns>
        //TODO Needs to be implemented
        //IParser<TInput, IConsList<TOutput>> Repeat<T>(Repititions r, IParser<TInput, T> delimiter);  

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

        /// <summary>
        /// Combines this parser with another parser such that the value produced by this parser is then parsed
        /// by the other parser, only succeeding if the other parser also succeeds
        /// </summary>
        /// <typeparam name="T">The output type of the other parser</typeparam>
        /// <param name="otherParser">The parser to use to attempt to parse the output of this parser</param>
        /// <returns>A parser that will attempt to parse the output with another parser</returns>
        IParser<TInput, T> ParseResultWith<T>(IParser<TOutput, T> otherParser);
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

        public override IParseResult<TInput, Tuple<TOut1, TOut2>> Apply(IStream<TInput> reader)
        {
            return _p1.Apply(reader).Match(
                success: (val1, r1) => _p2.Apply(r1).Match<IParseResult<TInput, Tuple<TOut1, TOut2>>>(
                    success: (val2, r2) => new SuccessResult<TInput, Tuple<TOut1, TOut2>>(new Tuple<TOut1, TOut2>(val1, val2), r2),
                    failure: (error2, r2) => new FailureResult<TInput, Tuple<TOut1, TOut2>>(error2, r2)),
                failure: (error1, r1) => new FailureResult<TInput, Tuple<TOut1, TOut2>>(error1, r1));
        }
    }

    public class ParseResultParser<TInput, TOutput, TResult> : Parser<TInput, TResult>
    {
        private readonly IParser<TInput, TOutput> _p1;
        private readonly IParser<TOutput, TResult> _p2;

        public ParseResultParser(IParser<TInput, TOutput> p1, IParser<TOutput, TResult> p2)
        {
            _p1 = p1;
            _p2 = p2;
        }

        public override IParseResult<TInput, TResult> Apply(IStream<TInput> reader)
        {
            return _p1.Apply(reader).Match(
                success: (val1, r1) => _p2.Apply(val1.LiftStream()).Match<IParseResult<TInput, TResult>>(
                    success: (val2, r2) => new SuccessResult<TInput, TResult>(val2, r1),
                    failure: (e2, r2) => new FailureResult<TInput, TResult>(e2, r1)),
                failure: (e1, r1) => new FailureResult<TInput, TResult>(e1, r1));
        }
    }

    /// <summary>
    /// An abstract parser that implements all of the combination and modification operators.
    /// </summary>
    /// <typeparam name="TInput">The type of input to be parsed</typeparam>
    /// <typeparam name="TOutput">The output generated by this parser</typeparam>
    public abstract class Parser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public abstract IParseResult<TInput, TOutput> Apply(IStream<TInput> reader);

        public IParser<TInput, Tuple<TOutput, T1>> FollowedBy<T1>(IParser<TInput, T1> otherParser)
        {
            return new CombinationParser<TInput, TOutput, T1>(this, otherParser);
        }

        public IParser<TInput, T> FollowedByDiscardingLeft<T>(IParser<TInput, T> keeper)
        {
            return new DiscardingPreviousParser<TInput, T, TOutput>(this, keeper);
        }

        public IParser<TInput, IConsList<TOutput>> Repeat(Repititions r)
        {
            return new RepeatingParser<TInput, TOutput>(this, r);
        }

        public IParser<TInput, IConsList<TOutput>> Repeat<T>(Repititions r, IParser<TInput, T> delimiter)
        {
            throw new NotImplementedException("IParser<TInput, IEnumerable<TOutput>> Repeat<T>(Repititions r, IParser<TInput, T> delimiter) not yet implemented.");
            //return new RepeatingDelimitedParser<TInput, TOutput>(this, r, delimiter);
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

        public IParser<TInput, T> ParseResultWith<T>(IParser<TOutput, T> otherParser)
        {
            return new ParseResultParser<TInput, TOutput, T>(this, otherParser);
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

        public override IParseResult<TInput, TOutput> Apply(IStream<TInput> reader)
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

        public override IParseResult<TInput, TOutput> Apply(IStream<TInput> reader)
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

        public override IParseResult<TInput, IMaybe<TOutput>> Apply(IStream<TInput> reader)
        {
            return _parser.Apply(reader).Match(
                success: (output, reader1) => new SuccessResult<TInput, IMaybe<TOutput>>(output.ToMaybe(), reader1),
                failure: (s, reader1) => new SuccessResult<TInput, IMaybe<TOutput>>(Maybe.Nothing<TOutput>(), reader));
        }
    }

    public class RepeatingParser<TInput, TOutput> : Parser<TInput, IConsList<TOutput>>
    {
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Repititions _r;

        public RepeatingParser(IParser<TInput, TOutput> parser, Repititions r)
        {
            _parser = parser;
            _r = r;
        }

        public override IParseResult<TInput, IConsList<TOutput>> Apply(IStream<TInput> reader)
        {
            return Apply(reader, _r);
        }

        private IParseResult<TInput, IConsList<TOutput>> Apply(IStream<TInput> reader, Repititions r)
        {
            var reps = r.Match(
                zeroOrMoreReps: () => -1,
                oneOrMoreReps: () => -2,
                nReps: n => n);
            var resultList = new List<TOutput>();
            var dontStop = true;
            var remainder = reader;
            while (reps != 0 && dontStop && remainder.Any)
            {
                var result = _parser.Apply(remainder);
                var parseResult = result.Match(
                    success: (v, rest) => Tuple.Create(v, rest).AsRight<IStream<TInput>, Tuple<TOutput, IStream<TInput>>>(),
                    failure: (err, rest) => rest.AsLeft<IStream<TInput>, Tuple<TOutput, IStream<TInput>>>());
                remainder = parseResult.Match(
                    right: tuple => tuple.Item2,
                    left: rest => rest);
                if (parseResult.IsRight)
                {
                    resultList.Add(parseResult.Match(
                        right: tuple => tuple.Item1,
                        left: rest => default(TOutput)));
                }
                else
                {
                    dontStop = false;
                }
                if (reps > 0)
                {
                    reps = reps - 1;
                }
            }
            IParseResult<TInput, IConsList<TOutput>> finalResult;

            if (reps > 0 || (reps == -2 && !resultList.Any()))
            {
                finalResult = new FailureResult<TInput, IConsList<TOutput>>("Need good error text", remainder);
            }
            else
            {
                finalResult = new SuccessResult<TInput, IConsList<TOutput>>(resultList.AsEnumerable().Reverse().Aggregate(ConsListOps.Nil<TOutput>(), (list, e) => e.Cons(list)), remainder);
            }
            return finalResult;
            //return r.Match(
            //    zeroOrMoreReps: () => _parser.Apply(reader).Match(
            //        success: (v, rest) => Apply(rest, r).Match(
            //            success: (vs, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.Cons(vs), remaining),
            //            failure: (error, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.LiftList(), remaining)),
            //        failure: (e, rest) => new SuccessResult<TInput, IConsList<TOutput>>(ConsListExtensions.Nil<TOutput>(), rest)),
            //    oneOrMoreReps: () => _parser.Apply(reader).Match<IParseResult<TInput, IConsList<TOutput>>>(
            //        success: (v, rest) => Apply(rest, r).Match(
            //                        success: (vs, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.Cons(vs), remaining),
            //                        failure: (e, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.LiftList(), remaining)),
            //        failure: (e, rest) => new FailureResult<TInput, IConsList<TOutput>>(string.Format("Expected one or more of ({0}) but got {1} instead", _parser.ToString(), e),rest)),
            //    nReps: n => n <= 0 ? new SuccessResult<TInput, IConsList<TOutput>>(ConsListExtensions.Nil<TOutput>(), reader) : _parser.Apply(reader).Match(
            //                    success: (v, rest) => Apply(rest, new NRepititions(n - 1)).Match<IParseResult<TInput, IConsList<TOutput>>>(
            //                            success: (vs, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.Cons(vs), remaining),
            //                            failure: (e, remaining) => new FailureResult<TInput, IConsList<TOutput>>(e, remaining)),
            //                    failure: (e, rest) => new FailureResult<TInput, IConsList<TOutput>>(string.Format("Expected {0} or more of ({1}) but got {2} instead", n,_parser.ToString(), e),rest)));
            //                            failure: (error, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.LiftList(), remaining)),
            //            failure: (e, rest) => new SuccessResult<TInput, IConsList<TOutput>>(ConsListExtensions.Nil<TOutput>(), rest)),
            //        oneOrMoreReps: () => _parser.Apply(reader).Match<IParseResult<TInput, IConsList<TOutput>>>(
            //            success: (v, rest) => Apply(rest, r).Match(
            //                            success: (vs, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.Cons(vs), remaining),
            //                            failure: (e, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.LiftList(), remaining)),
            //            failure: (e, rest) => new FailureResult<TInput, IConsList<TOutput>>(string.Format("Expected one or more of ({0}) but got {1} instead", _parser.ToString(), e), rest)),
            //        nReps:
            //            n => F.If(n <= 0,
            //                    () => new SuccessResult<TInput, IConsList<TOutput>>(ConsListExtensions.Nil<TOutput>(), reader),
            //                    () => _parser.Apply(reader).Match(
            //                        success: (v, rest) => Apply(rest, new NRepititions(n - 1)).Match<IParseResult<TInput, IConsList<TOutput>>>(
            //                                success: (vs, remaining) => new SuccessResult<TInput, IConsList<TOutput>>(v.Cons(vs), remaining),
            //                                failure: (e, remaining) => new FailureResult<TInput, IConsList<TOutput>>(e, remaining)),
            //                        failure: (e, rest) => new FailureResult<TInput, IConsList<TOutput>>(string.Format("Expected {0} or more of ({1}) but got {2} instead", n,_parser.ToString(), e), rest))))
            //    : r.Match(
            //        zeroOrMoreReps: () => new SuccessResult<TInput, IConsList<TOutput>>(ConsListExtensions.Nil<TOutput>(), reader),
            //        oneOrMoreReps: () => new FailureResult<TInput, IConsList<TOutput>>("Not enough reps", reader),
            //        nReps: n => n < 1
            //            ? (IParseResult<TInput, IConsList<TOutput>>)new SuccessResult<TInput, IConsList<TOutput>>(ConsListExtensions.Nil<TOutput>(), reader)
            //            : new FailureResult<TInput, IConsList<TOutput>>("Not enough reps", reader));

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

        public override IParseResult<TInput, TInput> Apply(IStream<TInput> reader)
        {
            return F.If<IParseResult<TInput, TInput>>(reader.Any && reader.Head.Select(_predicate).GetOrElse(() => false),
                () => new SuccessResult<TInput, TInput>(reader.Head.GetOrError(() => new Exception("Unexpected end of input stream!")), reader.Drop(1)),
                () =>
                    new FailureResult<TInput, TInput>(
                        string.Format("Expected {0} but got {1}", _expectation, reader.Head.Select(x => x.ToString()).GetOrElse(() => "{empty sequence}")), reader));
        }
    }

    /// <summary>
    /// This parser matches on the end of the input sequence, and yields Unit.
    /// </summary>
    /// <typeparam name="TInput">The type of input to parse</typeparam>
    public class EndOfInputParser<TInput> : Parser<TInput, Unit>
    {
        public override IParseResult<TInput, Unit> Apply(IStream<TInput> reader)
        {
            return F.If<IParseResult<TInput, Unit>>(reader.Any,
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

        public override IParseResult<TInput, TResult> Apply(IStream<TInput> reader)
        {
            return _parser.Apply(reader).Match<IParseResult<TInput, TResult>>(
                success: (output, reader1) => new SuccessResult<TInput, TResult>(_f(output), reader1),
                failure: (s, reader1) => new FailureResult<TInput, TResult>(s, reader1));
        }
    }

    public class BindingParser<TInput, TOutput, TResult> : Parser<TInput, TResult>
    {
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Func<TOutput, IParser<TInput, TResult>> _f;

        public BindingParser(IParser<TInput, TOutput> parser, Func<TOutput, IParser<TInput, TResult>> f)
        {
            _parser = parser;
            _f = f;
        }

        public override IParseResult<TInput, TResult> Apply(IStream<TInput> reader)
        {
            return _parser.Apply(reader).Match(
                success: (output, stream) => _f(output).Apply(stream),
                failure: (err, stream) => new FailureResult<TInput, TResult>(err, stream));
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

        public override IParseResult<TInput, TOutput> Apply(IStream<TInput> reader)
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

        public static IParser<TInput, TResult> SelectMany<TInput, TOutput, TResult>(
            this IParser<TInput, TOutput> parser, Func<TOutput, IParser<TInput, TResult>> f)
        {
            return new BindingParser<TInput, TOutput, TResult>(parser, f);
        }
    }
}
