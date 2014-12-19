using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class StateEither<TState, TLeft, TRight>
    {
        private readonly State<TState, IEither<TLeft, TRight>> _self;

        public StateEither(State<TState, IEither<TLeft, TRight>> state)
        {
            _self = state;
        }

        public StateEither(IEither<TLeft, TRight> either) : this(either.Insert<TState, IEither<TLeft, TRight>>())
        {
            
        }

        public StateEither(TRight right) : this(right.AsRight<TLeft, TRight>())
        {
            
        }

        public StateEither(TLeft left) : this(left.AsLeft<TLeft, TRight>())
        {
            
        }

        public State<TState, IEither<TLeft, TRight>> Out()
        {
            return _self;
        }

        public StateEither<TState, TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new StateEither<TState, TLeft, TResult>(_self.Select(either => either.Select(f)));
        }

        public StateEither<TState, TLeft, TResult> Bind<TResult>(Func<TRight, StateEither<TState, TLeft, TResult>> f)
        {
            return new StateEither<TState, TLeft, TResult>(new State<TState, IEither<TLeft, TResult>>(s =>
            {
                var result = _self.Run(s);
                return result.Item2.Match(
                    left: l => Tuple.Create(result.Item1, l.AsLeft<TLeft, TResult>()),
                    right: r => f(r).Out().Run(result.Item1));
            }));
        }
    }

    public static class StateEither
    {
        public static StateEither<TState, TLeft, TRight> ToStateEither<TState, TLeft, TRight>(
            this State<TState, IEither<TLeft, TRight>> state)
        {
            return new StateEither<TState, TLeft, TRight>(state);
        }

        public static StateEither<TState, TLeft, TRight> ToStateEither<TState, TLeft, TRight>(
            this State<TState, TRight> state)
        {
            return new StateEither<TState, TLeft, TRight>(state.Select(right => right.AsRight<TLeft, TRight>()));
        }

        public static StateEither<TState, TLeft, TRight> ToStateEither<TState, TLeft, TRight>(
            this IEither<TLeft, TRight> either)
        {
            return new StateEither<TState, TLeft, TRight>(either);
        }

        public static StateEither<TState, TLeft, TRight> InsertLeft<TState, TLeft, TRight>(this TLeft t)
        {
            return new StateEither<TState, TLeft, TRight>(t);
        }

        public static StateEither<TState, TLeft, TRight> InsertRight<TState, TLeft, TRight>(this TRight t)
        {
            return new StateEither<TState, TLeft, TRight>(t);
        }

        public static StateEither<TState, TLeft, TResult> Select<TState, TLeft, TRight, TResult>(this StateEither<TState, TLeft, TRight> stateT, Func<TRight, TResult> f)
        {
            return stateT.FMap(f);
        }

        public static StateEither<TState, TLeft, TResult> SelectMany<TState, TLeft, TRight, TResult>(
            this StateEither<TState, TLeft, TRight> stateT, Func<TRight, StateEither<TState, TLeft, TResult>> f)
        {
            return stateT.Bind(f);
        }

        public static StateEither<TState, TLeft, TSelect> SelectMany<TState, TLeft, TRight, TResult, TSelect>(
            this StateEither<TState, TLeft, TRight> stateT, Func<TRight, StateEither<TState, TLeft, TResult>> f,
            Func<TRight, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).InsertRight<TState, TLeft, TSelect>()));
        }
    }
}
