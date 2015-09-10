using System;
using System.Collections.Generic;
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

    public static class Parser
    {
        public static IEither<string, TOutput> Parse<TInput, TOutput>(this StateEither<ParserState<TInput>, string, TOutput> parser, IEnumerable<TInput> input)
        {
            return
                (from parseResult in parser
                 from allDone in ParserState<TInput>.IsEoF()
                 from _ in BasicFunctions.EIf(allDone, () => parseResult, () => "Parser didn't parse all available input").ToStateEither<ParserState<TInput>, string, TOutput>()
                 select _)
                .Out
                .Run(new ParserState<TInput>(input.ToArray()))
                .Item2;
        }

        public static Tuple<ParserState<TInput>, IEither<string, TOutput>> ParseSome<TInput, TOutput>(this StateEither<ParserState<TInput>, string, TOutput> parser, IEnumerable<TInput> input)
        {
            return parser.Out.Run(new ParserState<TInput>(input.ToArray()));
        }

        public static StateEither<ParserState<TInput>, string, TInput> Elem<TInput>(TInput expected)
        {
            return
                from next in ParserState<TInput>.Peek()
                from result in next.Equals(expected)
                    ? from _1 in ParserState<TInput>.MoveNext()
                      select next
                    : string.Format("Expected {0} but got --> {1} <--", expected, next).InsertLeft<ParserState<TInput>, string, TInput>()
                select result;
        }

        public static StateEither<ParserState<TInput>, string, TInput> ElemWhere<TInput>(Func<TInput, bool> predicate, string expectation)
        {
            return
                from next in ParserState<TInput>.Peek()
                from result in predicate(next)
                    ? from _1 in ParserState<TInput>.MoveNext()
                      select next
                    : string.Format("Expected {0} but got --> {1} <--", expectation, next).InsertLeft<ParserState<TInput>, string, TInput>()
                select result;
        }

        public static StateEither<ParserState<TInput>, string, IEnumerable<TOutput>> Repeat<TInput, TOutput>(
            this StateEither<ParserState<TInput>, string, TOutput> parser, int n)
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

        public static StateEither<ParserState<TInput>, string, TOutput> Pure<TInput, TOutput>(TOutput val)
        {
            return val.InsertRight<ParserState<TInput>, string, TOutput>();
        }

        public static StateEither<ParserState<TInput>, string, IMaybe<TOutput>> MakeOptional<TInput, TOutput>(
            this StateEither<ParserState<TInput>, string, TOutput> parser)
        {
            return parser.Select(val => val.ToMaybe()).Or(Pure<TInput, IMaybe<TOutput>>(Maybe.Nothing<TOutput>()));
        }

        public static StateEither<ParserState<TInput>, string, Unit> EoF<TInput>()
        {
            return from isEoF in ParserState<TInput>.IsEoF()
                   from result in BasicFunctions.EIf(isEoF, () => Unit.Only, () => "Expected EoF but more input remains").ToStateEither<ParserState<TInput>, string, Unit>()
                   select result;
        }

        public static StateEither<ParserState<TInput>, string, TOutput> WithoutConsuming<TInput, TOutput>(this StateEither<ParserState<TInput>, string, TOutput> parser)
        {
            return from i in ParserState<TInput>.Index.GetS().ToStateEither<ParserState<TInput>, string, uint>()
                   from result in parser
                   from _1 in ParserState<TInput>.Index.SetS(i).ToStateEither<ParserState<TInput>, string, Unit>()
                   select result;
        }
    }
}
