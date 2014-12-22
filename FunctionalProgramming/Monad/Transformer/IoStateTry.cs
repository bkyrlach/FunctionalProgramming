using FunctionalProgramming.Monad.Outlaws;
using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoStateTry<TState, TValue>
    {
        private readonly Io<State<TState, Try<TValue>>> _self;

        public IoStateTry(Io<State<TState, Try<TValue>>> io)
        {
            _self = io;
        }

        public IoStateTry(State<TState, Try<TValue>> state)
            : this(Io<State<TState, Try<TValue>>>.Apply(() => state))
        {

        }

        public IoStateTry(Try<TValue> @try)
            : this(@try.Insert<TState, Try<TValue>>())
        {

        }

        public IoStateTry(TValue val)
            : this(TryOps.Attempt(() => val))
        {
            
        }

        public Io<State<TState, Try<TValue>>> Out()
        {
            return _self;
        }

        public IoStateTry<TState, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new IoStateTry<TState, TResult>(_self.Select(state => state.Select(@try => @try.Select(f))));
        }

        public IoStateTry<TState, TResult> Bind<TResult>(Func<TValue, IoStateTry<TState, TResult>> f)
        {
            return new IoStateTry<TState, TResult>(_self.SelectMany(state => Io<State<TState, Try<TResult>>>.Apply(() => new State<TState, Try<TResult>>(s =>
            {
                var result = state.Run(s);
                return result.Item2.Match(
                    success: val => f(val).Out().UnsafePerformIo().Run(result.Item1),
                    failure: ex => Tuple.Create(result.Item1, ex.Fail<TResult>()));
            }))));
        }
    }

    public static class IoStateTry
    {
        public static IoStateTry<TState, T> ToIoStateTry<TState, T>(this Io<State<TState, Try<T>>> ioT)
        {
            return new IoStateTry<TState, T>(ioT);
        }

        public static IoStateTry<TState, T> ToIoStateTry<TState, T>(this Io<T> io)
        {
            return new IoStateTry<TState, T>(io.Select(t => TryOps.Attempt(() => t).Insert<TState, Try<T>>()));
        }

        public static IoStateTry<TState, T> ToIoStateTry<TState, T>(this State<TState, T> state)
        {
            return new IoStateTry<TState, T>(state.Select(t => TryOps.Attempt(() => t)));
        }

        public static IoStateTry<TState, T> ToIoStateTry<TState, T>(this T t)
        {
            return new IoStateTry<TState, T>(t);
        }

        public static IoStateTry<TState, TResult> Select<TState, TValue, TResult>(this IoStateTry<TState, TValue> ioT,
            Func<TValue, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoStateTry<TState, TResult> SelectMany<TState, TValue, TResult>(this IoStateTry<TState, TValue> ioT,
            Func<TValue, IoStateTry<TState, TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoStateTry<TState, TSelect> SelectMany<TState, TValue, TResult, TSelect>(
            this IoStateTry<TState, TValue> ioT, Func<TValue, IoStateTry<TState, TResult>> f,
            Func<TValue, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoStateTry<TState, TSelect>()));
        }
    }

}
