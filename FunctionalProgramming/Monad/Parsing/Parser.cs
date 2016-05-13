using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Transformer;

namespace FunctionalProgramming.Monad.Parsing
{
    public sealed class ParserState<T>
    {
        public static readonly Lens<ParserState<T>, T[]> Data = new Lens<ParserState<T>, T[]>((state, data) => state.Copy(data: data), state => state._data);
        public static readonly Lens<ParserState<T>, uint> Index = new Lens<ParserState<T>, uint>((state, index) => state.Copy(index: index), state => state._index);

        public static StateEither<ParserState<T>, string, T> Peek()
        {
            return
                from i in Index.GetS().ToStateEither<ParserState<T>, string, uint>()
                from data in Data.GetS().ToStateEither<ParserState<T>, string, T[]>()
                from @byte in i >= data.Length
                    ? "Unexpected end of input sequence".InsertLeft<ParserState<T>, string, T>()
                    : data[i].InsertRight<ParserState<T>, string, T>()
                select @byte;
        }

        public static StateEither<ParserState<T>, string, uint> MoveNext()
        {
            return
                Index.ModS(n => n + 1).ToStateEither<ParserState<T>, string, uint>();
        }

        public static StateEither<ParserState<T>, string, bool> IsEoF()
        {
            return
                (from i in Index.GetS()
                 from data in Data.GetS()
                 select (i >= data.Length).AsRight<string, bool>())
                .ToStateEither();
        }

        private readonly T[] _data;
        private readonly uint _index;

        public ParserState(T[] data)
        {
            _data = data;
            _index = 0;
        }

        private ParserState(T[] data, uint index)
        {
            _data = data;
            _index = index;
        }

        public ParserState<T> Copy(T[] data = null, uint? index = null)
        {
            return new ParserState<T>(data ?? _data, index ?? _index);
        }
    }

    public sealed class Parser<TInput, TOutput>
    {
        public readonly StateEither<ParserState<TInput>, string, TOutput> Out;

        public Parser(StateEither<ParserState<TInput>, string, TOutput> self)
        {
            Out = self;
        }

        public IEither<string, TOutput> Parse(IEnumerable<TInput> input)
        {
            return
                (from parseResult in Out
                 from allDone in ParserState<TInput>.IsEoF()
                 from _ in BasicFunctions.EIf(allDone, () => parseResult, () => "Parser didn't parse all available input").ToStateEither<ParserState<TInput>, string, TOutput>()
                 select _)
                .Out
                .Run(new ParserState<TInput>(input.ToArray()))
                .Item2;
        }

        public Tuple<ParserState<TInput>, IEither<string, TOutput>> ParseSome(IEnumerable<TInput> input)
        {
            return Out.Out.Run(new ParserState<TInput>(input.ToArray()));
        }

        public Parser<TInput, TOutput> CombineTakeLeft<TRight>(Parser<TInput, TRight> other)
        {
            return new Parser<TInput, TOutput>(Out.CombineTakeLeft(other.Out));
        }

        public Parser<TInput, TRight> CombineTakeRight<TRight>(Parser<TInput, TRight> other)
        {
            return new Parser<TInput, TRight>(Out.CombineTakeRight(other.Out));
        }
    }

    public static class Parser
    {

        public static Parser<TInput, TInput> Elem<TInput>(TInput expected)
        {
            return new Parser<TInput, TInput>(
                from next in ParserState<TInput>.Peek()
                from result in next.Equals(expected)
                    ? from _1 in ParserState<TInput>.MoveNext()
                      select next
                    : $"Expected {expected} but got --> {next} <--".InsertLeft<ParserState<TInput>, string, TInput>()
                select result);
        }

        public static Parser<TInput, TInput> ElemWhere<TInput>(Func<TInput, bool> predicate, string expectation)
        {
            return new Parser<TInput, TInput>(
                from next in ParserState<TInput>.Peek()
                from result in predicate(next)
                    ? from _1 in ParserState<TInput>.MoveNext()
                      select next
                    : $"Expected {expectation} but got --> {next} <--".InsertLeft<ParserState<TInput>, string, TInput>()
                select result);
        }

        public static Parser<TInput, IEnumerable<TOutput>> Repeat<TInput, TOutput>(this Parser<TInput, TOutput> parser, int n)
        {
            return new Parser<TInput, IEnumerable<TOutput>>(Enumerable.Repeat(parser.Out, n).Sequence());
        }

        public static Parser<TInput, TOutput> Pure<TInput, TOutput>(TOutput val)
        {
            return new Parser<TInput, TOutput>(val.InsertRight<ParserState<TInput>, string, TOutput>());
        }

        public static Parser<TInput, IMaybe<TOutput>> MakeOptional<TInput, TOutput>(this Parser<TInput, TOutput> parser)
        {
            return new Parser<TInput, IMaybe<TOutput>>(parser.Out.Select(val => val.ToMaybe()).Or(Pure<TInput, IMaybe<TOutput>>(Maybe.Nothing<TOutput>()).Out));
        }

        public static Parser<TInput, Unit> EoF<TInput>()
        {
            return new Parser<TInput, Unit>(
                from isEoF in ParserState<TInput>.IsEoF()
                from result in BasicFunctions.EIf(isEoF, () => Unit.Only, () => "Expected EoF but more input remains").ToStateEither<ParserState<TInput>, string, Unit>()
                select result);
        }

        public static Parser<TInput, TOutput> WithoutConsuming<TInput, TOutput>(this Parser<TInput, TOutput> parser)
        {
            return new Parser<TInput, TOutput>(
                from i in ParserState<TInput>.Index.GetS().ToStateEither<ParserState<TInput>, string, uint>()
                from result in parser.Out
                from _1 in ParserState<TInput>.Index.SetS(i).ToStateEither<ParserState<TInput>, string, Unit>()
                select result);
        }

        public static Parser<TInput, IEnumerable<TOutput>> Sequence<TInput, TOutput>(this IEnumerable<Parser<TInput, TOutput>> xs)
        {
            return new Parser<TInput, IEnumerable<TOutput>>(xs.Select(x => x.Out).Sequence());
        }

        public static Parser<TInput, IEnumerable<TOutput>> Traverse<TInitial, TInput, TOutput>(
            this IEnumerable<TInitial> xs, Func<TInitial, Parser<TInput, TOutput>> f)
        {
            return xs.Select(f).Sequence();
        }

        public static Parser<TInput, TOutput> Or<TInput, TOutput>(this Parser<TInput, TOutput> a,
            Parser<TInput, TOutput> b)
        {
            return new Parser<TInput, TOutput>(a.Out.Or(b.Out));
        }

        public static Parser<TInput, TResult> Select<TInput, TOutput, TResult>(this Parser<TInput, TOutput> m,
            Func<TOutput, TResult> f)
        {
            return new Parser<TInput, TResult>(m.Out.Select(f));
        }

        public static Parser<TInput, IEnumerable<TOutput>> Many<TInput, TOutput>(this Parser<TInput, TOutput> p)
        {
            return new Parser<TInput, IEnumerable<TOutput>>(p.Out.Many());
        }

        public static Parser<TInput, IEnumerable<TOutput>> Many1<TInput, TOutput>(this Parser<TInput, TOutput> p)
        {
            return new Parser<TInput, IEnumerable<TOutput>>(p.Out.Many1());
        }

        public static Parser<TInput, TResult> SelectMany<TInitial, TInput, TResult>(this Parser<TInput, TInitial> m, Func<TInitial, Parser<TInput, TResult>> f)
        {
            return new Parser<TInput, TResult>(m.Out.SelectMany(a => f(a).Out));
        }

        public static Parser<TInput, TSelect> SelectMany<TInitial, TInput, TResult, TSelect>(
            this Parser<TInput, TInitial> m, Func<TInitial, Parser<TInput, TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => Pure<TInput, TSelect>(selector(a, b))));
        }
    }
}
