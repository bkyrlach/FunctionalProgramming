using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class IoStateEnumerable<TState, TValue>
    {
        public readonly Io<State<TState, IEnumerable<TValue>>> Out;

        public IoStateEnumerable(Io<State<TState, IEnumerable<TValue>>> io)
        {
            Out = io;
        }

        public IoStateEnumerable(State<TState, IEnumerable<TValue>> state) : this(Io.Apply(() => state))
        {

        }

        public IoStateEnumerable(IEnumerable<TValue> values)
            : this(values.Insert<TState, IEnumerable<TValue>>())
        {

        }

        public IoStateEnumerable(State<TState, TValue> state)
            : this(state.Select(value => value.LiftEnumerable()))
        {

        }

        public IoStateEnumerable(TValue value)
            : this(value.LiftEnumerable())
        {

        }

        public IoStateEnumerable<TState, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new IoStateEnumerable<TState, TResult>(Out.Select(state => state.Select(values => values.Select(f))));
        }

        public IoStateEnumerable<TState, TResult> Bind<TResult>(Func<TValue, IoStateEnumerable<TState, TResult>> f)
        {
            return new IoStateEnumerable<TState, TResult>(Out.Select(state => state.SelectMany(xs => xs.Select(x => f(x).Out).Sequence().Select(states => states.Sequence()).UnsafePerformIo()).Select(xs => xs.SelectMany(BasicFunctions.Identity))));
        }

        public IoStateEnumerable<TState, TValue> Keep(Func<TValue, bool> predicate)
        {
            return new IoStateEnumerable<TState, TValue>(Out.Select(state => state.Select(values => values.Where(predicate))));
        }
    }

    public static class IoStateEnumerable
    {
        public static IoStateEnumerable<TState, TValue> ToIoStateEnumerable<TState, TValue>(this Io<State<TState, IEnumerable<TValue>>> io)
        {
            return new IoStateEnumerable<TState, TValue>(io);
        }

        public static IoStateEnumerable<TState, TValue> ToIoStateEnumerable<TState, TValue>(this State<TState, IEnumerable<TValue>> state)
        {
            return new IoStateEnumerable<TState, TValue>(state);
        }

        public static IoStateEnumerable<TState, TValue> ToIoStateEnumerable<TState, TValue>(this State<TState, TValue> state)
        {
            return new IoStateEnumerable<TState, TValue>(state);
        }

        public static IoStateEnumerable<TState, TValue> ToIoStateEnumerable<TState, TValue>(this IEnumerable<TValue> values)
        {
            return new IoStateEnumerable<TState, TValue>(values);
        }

        public static IoStateEnumerable<TState, TValue> ToIoStateEnumerable<TState, TValue>(this TValue value)
        {
            return new IoStateEnumerable<TState, TValue>(value);
        }

        public static IoStateEnumerable<TState, TValue> ToIoStateEnumerable<TState, TValue>(this Io<TValue> io)
        {
            return new IoStateEnumerable<TState, TValue>(io.Select(x => x.LiftEnumerable().Insert<TState, IEnumerable<TValue>>()));
        }

        public static IoStateEnumerable<TState, TValue> Where<TState, TValue>(this IoStateEnumerable<TState, TValue> stateT,
            Func<TValue, bool> predicate)
        {
            return stateT.Keep(predicate);
        }

        public static IoStateEnumerable<TState, TResult> Select<TState, TInitial, TResult>(
            this IoStateEnumerable<TState, TInitial> stateT, Func<TInitial, TResult> f)
        {
            return stateT.FMap(f);
        }

        public static IoStateEnumerable<TState, TResult> SelectMany<TState, TInitial, TResult>(
            this IoStateEnumerable<TState, TInitial> stateT, Func<TInitial, IoStateEnumerable<TState, TResult>> f)
        {
            return stateT.Bind(f);
        }

        public static IoStateEnumerable<TState, TSelect> SelectMany<TState, TInitial, TResult, TSelect>(
            this IoStateEnumerable<TState, TInitial> stateT, Func<TInitial, IoStateEnumerable<TState, TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoStateEnumerable<TState, TSelect>()));
        }
    }
}
