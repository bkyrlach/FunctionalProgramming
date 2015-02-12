using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class StateEnumerable<TState, TValue>
    {
        private readonly State<TState, IEnumerable<TValue>> _self;

        public StateEnumerable(State<TState, IEnumerable<TValue>> state)
        {
            _self = state;
        }

        public StateEnumerable(IEnumerable<TValue> values) : this(values.Insert<TState, IEnumerable<TValue>>())
        {
            
        }

        public StateEnumerable(State<TState, TValue> state) : this(state.Select(value => value.LiftEnumerable()))
        {
            
        }

        public StateEnumerable(TValue value) : this(value.LiftEnumerable())
        {
            
        }

        public State<TState, IEnumerable<TValue>> Out()
        {
            return _self;
        }

        public StateEnumerable<TState, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new StateEnumerable<TState, TResult>(_self.Select(values => values.Select(f)));
        }

        public StateEnumerable<TState, TResult> Bind<TResult>(Func<TValue, StateEnumerable<TState, TResult>> f)
        {
            return new StateEnumerable<TState, TResult>(_self.SelectMany(values => values.Select(v => f(v).Out()).Sequence().Select(enumerable => enumerable.SelectMany(BasicFunctions.Identity))));
        }

        public StateEnumerable<TState, TValue> Keep(Func<TValue, bool> predicate)
        {
            return new StateEnumerable<TState, TValue>(_self.Select(values => values.Where(predicate)));
        }
    }

    public static class StateEnumerable
    {
        public static StateEnumerable<TState, TValue> In<TState, TValue>(this State<TState, IEnumerable<TValue>> state)
        {
            return new StateEnumerable<TState, TValue>(state);
        }

        public static StateEnumerable<TState, TValue> ToStateEnumerable<TState, TValue>(this State<TState, TValue> state)
        {
            return new StateEnumerable<TState, TValue>(state);
        }

        public static StateEnumerable<TState, TValue> ToStateEnumerable<TState, TValue>(this IEnumerable<TValue> values)
        {
            return new StateEnumerable<TState, TValue>(values);
        }

        public static StateEnumerable<TState, TValue> ToStateEnumerable<TState, TValue>(this TValue value)
        {
            return new StateEnumerable<TState, TValue>(value);
        }

        public static StateEnumerable<TState, TValue> Where<TState, TValue>(this StateEnumerable<TState, TValue> stateT,
            Func<TValue, bool> predicate)
        {
            return stateT.Keep(predicate);
        }

        public static StateEnumerable<TState, TResult> Select<TState, TInitial, TResult>(
            this StateEnumerable<TState, TInitial> stateT, Func<TInitial, TResult> f)
        {
            return stateT.FMap(f);
        }

        public static StateEnumerable<TState, TResult> SelectMany<TState, TInitial, TResult>(
            this StateEnumerable<TState, TInitial> stateT, Func<TInitial, StateEnumerable<TState, TResult>> f)
        {
            return stateT.Bind(f);
        }

        public static StateEnumerable<TState, TSelect> SelectMany<TState, TInitial, TResult, TSelect>(
            this StateEnumerable<TState, TInitial> stateT, Func<TInitial, StateEnumerable<TState, TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToStateEnumerable<TState, TSelect>()));
        } 
    }
}
