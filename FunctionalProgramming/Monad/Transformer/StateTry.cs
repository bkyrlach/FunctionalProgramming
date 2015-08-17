using System;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class StateTry<TState, TValue>
    {
        private readonly State<TState, Try<TValue>> _self;

        public StateTry(State<TState, Try<TValue>> state)
        {
            _self = state;
        }

        public StateTry(Try<TValue> t) : this(t.Insert<TState, Try<TValue>>())
        {
            
        }

        public StateTry(TValue t) : this(Try.Attempt(() => t))
        {
            
        }

        public State<TState, Try<TValue>> Out()
        {
            return _self;
        }

        public StateTry<TState, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new StateTry<TState, TResult>(_self.Select(t => t.Select(f)));
        }

        public StateTry<TState, TResult> Bind<TResult>(Func<TValue, StateTry<TState, TResult>> f)
        {
            return new StateTry<TState, TResult>(_self.SelectMany(t => t.Match(
                success: val => f(val).Out(),
                failure: ex => ex.Fail<TResult>().Insert<TState, Try<TResult>>())));
        }
    }

    public static class StateTry
    {
        public static StateTry<TState, T> ToStateTry<TState, T>(this T t)
        {
            return new StateTry<TState, T>(Try.Attempt(() => t).Insert<TState, Try<T>>());
        }

        public static StateTry<TState, T> ToStateTry<TState, T>(this Try<T> @try)
        {
            return new StateTry<TState, T>(@try);
        }

        public static StateTry<TState, T> ToStateTry<TState, T>(this State<TState, T> state)
        {
            return new StateTry<TState, T>(state.Select(t => Try.Attempt(() => t)));
        }

        public static StateTry<TState, T> ToStateTry<TState, T>(this State<TState, Try<T>> state)
        {
            return new StateTry<TState, T>(state);
        }

        public static StateTry<TState, TResult> Select<TState, TInitial, TResult>(
            this StateTry<TState, TInitial> stateT, Func<TInitial, TResult> f)
        {
            return stateT.FMap(f);
        }

        public static StateTry<TState, TResult> SelectMany<TState, TInitial, TResult>(
            this StateTry<TState, TInitial> stateT, Func<TInitial, StateTry<TState, TResult>> f)
        {
            return stateT.Bind(f);
        }

        public static StateTry<TState, TSelect> SelectMany<TState, TInitial, TResult, TSelect>(
            this StateTry<TState, TInitial> stateT, Func<TInitial, StateTry<TState, TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToStateTry<TState, TSelect>()));
        }
    }
}
