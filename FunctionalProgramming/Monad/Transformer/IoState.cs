using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoState<TState, TValue>
    {
        private readonly Io<State<TState, TValue>> _self;
 
        public IoState(Io<State<TState, TValue>> io)
        {
            _self = io;
        }

        public IoState(State<TState, TValue> state) : this(Io.Apply(() => state))
        {
            
        }

        public IoState(TValue val) : this(val.Insert<TState, TValue>())
        {
            
        }

        public Io<State<TState, TValue>> Out()
        {
            return _self;
        }

        public IoState<TState, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new IoState<TState, TResult>(_self.Select(state => state.Select(f)));
        }

        public IoState<TState, TResult> Bind<TResult>(Func<TValue, IoState<TState, TResult>> f)
        {
            return new IoState<TState, TResult>(_self.SelectMany(state => Io.Apply(() => new State<TState, TResult>(s =>
            {
                var result = state.Run(s);
                var result2 = f(result.Item2).Out().UnsafePerformIo().Run(result.Item1);
                return result2;
            }))));
        }
    }

    public static class IoState
    {
        public static IoState<TState, T> In<TState, T>(this Io<State<TState, T>> ioT)
        {
            return new IoState<TState, T>(ioT);
        }

        public static IoState<TState, T> ToIoState<TState, T>(this Io<T> io)
        {
            return new IoState<TState, T>(io.Select(t => t.Insert<TState, T>()));
        }

        public static IoState<TState, T> ToIoState<TState, T>(this State<TState, T> state)
        {
            return new IoState<TState, T>(state);
        }

        public static IoState<TState, T> ToIoState<TState, T>(this T t)
        {
            return new IoState<TState, T>(t);
        }

        public static IoState<TState, TResult> Select<TState, TValue, TResult>(this IoState<TState, TValue> ioT,
            Func<TValue, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoState<TState, TResult> SelectMany<TState, TValue, TResult>(this IoState<TState, TValue> ioT,
            Func<TValue, IoState<TState, TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoState<TState, TSelect> SelectMany<TState, TValue, TResult, TSelect>(
            this IoState<TState, TValue> ioT, Func<TValue, IoState<TState, TResult>> f,
            Func<TValue, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoState<TState, TSelect>()));
        }
    }
}
