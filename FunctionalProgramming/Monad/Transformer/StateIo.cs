using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public struct StateIo<TState, TValue>
    {
        private readonly Func<TState, Io<Tuple<TState, TValue>>> _body;

        public StateIo(Func<TState, Io<Tuple<TState, TValue>>> body)
        {
            _body = body;
        }

        public StateIo(State<TState, Io<TValue>> state) : this(s =>
        {
            var result = state.Run(s);
            return result.Item2.Select(val => Tuple.Create(result.Item1, val));
        })
        {
            
        }

        public StateIo(Io<TValue> io) : this(io.Insert<TState, Io<TValue>>())
        {
            
        }

        public Io<Tuple<TState, TValue>> RunIo(TState state)
        {
            return _body(state);
        }

        public Io<TValue> EvalIo(TState state)
        {
            return RunIo(state).Select(result => result.Item2);
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
            return new StateIo<TState, TResult>(s =>
            {
                var s1 = stateT.RunIo(s);
                return s1.Select(pair => Tuple.Create(pair.Item1, f(pair.Item2)));
            });
        }

        public static StateIo<TState, TResult> SelectMany<TState, TValue, TResult>(this StateIo<TState, TValue> stateT, Func<TValue, StateIo<TState, TResult>> f)
        {
            return new StateIo<TState, TResult>(s =>
            {
                var s1 = stateT.RunIo(s);
                return Io.Apply(() =>
                {
                    var pair = s1.UnsafePerformIo();
                    var s2 = f(pair.Item2).RunIo(pair.Item1);
                    return s2.UnsafePerformIo();
                });
            });
        }

        public static StateIo<TState, TSelect> SelectMany<TState, TValue, TResult, TSelect>(this StateIo<TState, TValue> stateT, Func<TValue, StateIo<TState, TResult>> f, Func<TValue, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToStateIo<TState, TSelect>()));
        }
    }
}
