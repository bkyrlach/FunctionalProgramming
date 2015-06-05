using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Transformer;

namespace FunctionalProgramming.Monad.Parsing
{
    public static class Parser
    {
        public static IEither<string, TOutput> Parse<TInput, TOutput>(this StateEither<IStream<TInput>, string, TOutput> parser, IStream<TInput> input)
        {
            var res = parser.Out().Run(input);
            return BasicFunctions.If(res.Item1.Any,
                () => "Parser didn't parse all available input".AsLeft<string, TOutput>(), 
                () => res.Item2);
        }

        public static IEither<string, TOutput> Parse<TInput, TOutput>(this StateEither<IStream<TInput>, string, TOutput> parser, IEnumerable<TInput> input)
        {
            return parser.Parse(input.ToStream());
        }

        public static Tuple<IStream<TInput>, IEither<string, TOutput>> ParseSome<TInput, TOutput>(
            this StateEither<IStream<TInput>, string, TOutput> parser, IStream<TInput> input)
        {
            return parser.Out().Run(input);
        }

        public static Tuple<IStream<TInput>, IEither<string, TOutput>> ParseSome<TInput, TOutput>(this StateEither<IStream<TInput>, string, TOutput> parser, IEnumerable<TInput> input)
        {
            return parser.ParseSome(input.ToStream());
        }

        public static StateEither<IStream<TInput>, string, TInput> Elem<TInput>(TInput expected)
        {
            return 
                from xs in State.Get<IStream<TInput>, IStream<TInput>>(BasicFunctions.Identity).ToStateEither<IStream<TInput>, string, IStream<TInput>>()
                from _ in xs.Match(
                    cons: (h, t) => BasicFunctions.If(h.Equals(expected), 
                        () => from _1 in t.Put().ToStateEither<IStream<TInput>, string, Unit>()
                              select h,
                        () => string.Format("Expected {0} but got {1}", expected, h).InsertLeft<IStream<TInput>, string, TInput>()),
                    nil: () => string.Format("Expected {0} but reached end of input sequence", expected).InsertLeft<IStream<TInput>, string, TInput>())
                select _;
        }

        public static StateEither<IStream<TInput>, string, TInput> ElemWhere<TInput>(Func<TInput, bool> predicate, string expectation)
        {
            return
                from xs in State.Get<IStream<TInput>, IStream<TInput>>(BasicFunctions.Identity).ToStateEither<IStream<TInput>, string, IStream<TInput>>()
                from _ in
                    xs.Match(
                        cons: (h, t) => BasicFunctions.If(predicate(h),
                        () => from _1 in t.Put().ToStateEither<IStream<TInput>, string, Unit>()
                              select h,
                        () => string.Format("Expected {0} but got {1}", expectation, h).InsertLeft<IStream<TInput>, string, TInput>()),
                        nil: () => string.Format("Expected {0} but reached end of input sequence", expectation).InsertLeft<IStream<TInput>, string, TInput>())
                select _;
        }

        public static StateEither<IStream<TInput>, string, IEnumerable<TOutput>> Repeat<TInput, TOutput>(
            this StateEither<IStream<TInput>, string, TOutput> parser, int n)
        {
            return Enumerable.Repeat(parser, n).Sequence();
        }

        public static StateEither<IStream<TInput>, string, Tuple<TFirst, TSecond>> FollowedBy<TInput, TFirst, TSecond>(
            this StateEither<IStream<TInput>, string, TFirst> p1, StateEither<IStream<TInput>, string, TSecond> p2)
        {
            return
                from first in p1
                from second in p2
                select Tuple.Create(first, second);
        }
    }
}
