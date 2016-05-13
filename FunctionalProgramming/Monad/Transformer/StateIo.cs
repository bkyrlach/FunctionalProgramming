using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class StateIo<TState, TValue>
    {
        public readonly State<TState, Io<TValue>> Out;

        public StateIo(State<TState, Io<TValue>> self)
        {
            Out = self;
        }

        public StateIo(Io<TValue> io) : this(io.Insert<TState, Io<TValue>>())
        {
            
        }
    }

    public static class StateIo
    {
        public static StateIo<TState, T> ToStateIo<TState, T>(this State<TState, Io<T>> stateT)
        {
            return new StateIo<TState, T>(stateT);
        }

        public static StateIo<TState, T> ToStateIo<TState, T>(this Io<T> io)
        {
            return new StateIo<TState, T>(io);
        }

        public static StateIo<TState, T> ToStateIo<TState, T>(this State<TState, T> state)
        {
            return new StateIo<TState, T>(state.Select(val => Io.Apply(() => val)));
        }

        public static StateIo<TState, T> ToStateIo<TState, T>(this T t)
        {
            return new StateIo<TState, T>(Io.Apply(() => t));
        }

        public static StateIo<TState, TResult> Select<TState, TValue, TResult>(this StateIo<TState, TValue> stateT, Func<TValue, TResult> f)
        {
            return new StateIo<TState, TResult>(stateT.Out.Select(io => io.Select(f)));
        }

        public static StateIo<TState, TResult> SelectMany<TState, TValue, TResult>(this StateIo<TState, TValue> stateT, Func<TValue, StateIo<TState, TResult>> f)
        {
            return new State<TState, Io<TResult>>(state =>
            {
                var result = stateT.Out.Run(state);
                state = result.Item1;
                var value = result.Item2.UnsafePerformIo();
                var nextState = f(value);
                var nextValue = nextState.Out.Run(state);
                return nextValue;
            }).ToStateIo();
        }

        public static StateIo<TState, TSelect> SelectMany<TState, TValue, TResult, TSelect>(this StateIo<TState, TValue> stateT, Func<TValue, StateIo<TState, TResult>> f, Func<TValue, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToStateIo<TState, TSelect>()));
        }
    }
}
