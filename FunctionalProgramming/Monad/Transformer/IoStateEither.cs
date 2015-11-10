using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class IoStateEither<TState, TLeft, TRight>
    {
        public readonly Io<State<TState, IEither<TLeft, TRight>>> Out;

        public IoStateEither(Io<State<TState, IEither<TLeft, TRight>>> state)
        {
            Out = state;
        }

        public IoStateEither(State<TState, IEither<TLeft, TRight>> state) : this(Io.Apply(() => state))
        {
            
        }

        public IoStateEither(IEither<TLeft, TRight> either) : this(either.Insert<TState, IEither<TLeft, TRight>>())
        {
            
        }

        public IoStateEither(TRight right) : this(right.AsRight<TLeft, TRight>())
        {
            
        }

        public IoStateEither(TLeft left)
            : this(left.AsLeft<TLeft, TRight>())
        {
            
        }

        public IoStateEither<TState, TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new IoStateEither<TState, TLeft, TResult>(Out.Select(state => state.Select(either => either.Select(f))));
        }

        public IoStateEither<TState, TLeft, TResult> Bind<TResult>(Func<TRight, IoStateEither<TState, TLeft, TResult>> f)
        {
            return new IoStateEither<TState, TLeft, TResult>(Out.SelectMany(state => Io.Apply(() => new State<TState, IEither<TLeft, TResult>>(s =>
            {
                var result = state.Run(s);
                return result.Item2.Match(
                    left: l => Tuple.Create(result.Item1, l.AsLeft<TLeft, TResult>()),
                    right: r => f(r).Out.UnsafePerformIo().Run(result.Item1));
            }))));
        }
    }

    public static class IoStateEither
    {
        public static IoStateEither<TState, TLeft, TRight> ToIoStateEither<TState, TLeft, TRight>(
            this Io<State<TState, IEither<TLeft, TRight>>> state)
        {
            return new IoStateEither<TState, TLeft, TRight>(state);
        }

        public static IoStateEither<TState, TLeft, TRight> ToIoStateEither<TState, TLeft, TRight>(this Io<TRight> io)
        {
            return new IoStateEither<TState, TLeft, TRight>(io.Select(right => right.AsRight<TLeft, TRight>().Insert<TState, IEither<TLeft, TRight>>()));
        }

        public static IoStateEither<TState, TLeft, TRight> ToIoStateEither<TState, TLeft, TRight>(
            this State<TState, TRight> state)
        {
            return new IoStateEither<TState, TLeft, TRight>(state.Select(right => right.AsRight<TLeft, TRight>()));
        }

        public static IoStateEither<TState, TLeft, TRight> ToIoStateEither<TState, TLeft, TRight>(
            this IEither<TLeft, TRight> either)
        {
            return new IoStateEither<TState, TLeft, TRight>(either);
        }

        public static IoStateEither<TState, TLeft, TRight> ToIoStateEither<TState, TLeft, TRight>(this Io<IEither<TLeft, TRight>> ioEither)
        {
            return new IoStateEither<TState, TLeft, TRight>(ioEither.Select(either => either.Insert<TState, IEither<TLeft, TRight>>()));
        } 

        public static IoStateEither<TState, TLeft, TRight> InsertLeftIo<TState, TLeft, TRight>(this TLeft t)
        {
            return new IoStateEither<TState, TLeft, TRight>(t);
        }

        public static IoStateEither<TState, TLeft, TRight> InsertRightIo<TState, TLeft, TRight>(this TRight t)
        {
            return new IoStateEither<TState, TLeft, TRight>(t);
        }

        public static IoStateEither<TState, TLeft, TResult> Select<TState, TLeft, TRight, TResult>(this IoStateEither<TState, TLeft, TRight> stateT, Func<TRight, TResult> f)
        {
            return stateT.FMap(f);
        }

        public static IoStateEither<TState, TLeft, TResult> SelectMany<TState, TLeft, TRight, TResult>(
            this IoStateEither<TState, TLeft, TRight> stateT, Func<TRight, IoStateEither<TState, TLeft, TResult>> f)
        {
            return stateT.Bind(f);
        }

        public static IoStateEither<TState, TLeft, TSelect> SelectMany<TState, TLeft, TRight, TResult, TSelect>(
            this IoStateEither<TState, TLeft, TRight> stateT, Func<TRight, IoStateEither<TState, TLeft, TResult>> f,
            Func<TRight, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).InsertRightIo<TState, TLeft, TSelect>()));
        }
    }
}
