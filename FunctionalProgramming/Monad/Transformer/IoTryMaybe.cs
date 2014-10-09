using System;
using System.Runtime.Remoting.Messaging;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoTryMaybe<T>
    {
        private readonly Io<Try<IMaybe<T>>> _self;

        public IoTryMaybe(Io<Try<IMaybe<T>>> io)
        {
            _self = io;
        }

        public IoTryMaybe(Try<IMaybe<T>> aTry) : this(Io<Try<IMaybe<T>>>.Apply(() => aTry))
        {

        }

        public IoTryMaybe(IMaybe<T> m) : this(Io<Try<IMaybe<T>>>.Apply(() => TryOps.Attempt(() => m)))
        {
            
        }

        public IoTryMaybe(T t) : this(Io<Try<IMaybe<T>>>.Apply(() => TryOps.Attempt(() => t.ToMaybe())))
        {
        }

        public IoTryMaybe<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoTryMaybe<TResult>(_self.Select(t => t.Select(m => m.Select(f))));
        }

        public Io<Try<IMaybe<T>>> Out()
        {
            return _self;
        }

        public IoTryMaybe<TResult> Bind<TResult>(Func<T, IoTryMaybe<TResult>> f)
        {
            return new IoTryMaybe<TResult>(_self.SelectMany(t => t.Match(
                success: m => m.Match(
                    just: v => f(v).Out(),
                    nothing: () => Io<Try<IMaybe<TResult>>>.Apply(() => TryOps.Attempt(() => Maybe.Nothing<TResult>()))),
                failure: ex => Io<Try<IMaybe<TResult>>>.Apply(() => ex.Fail<IMaybe<TResult>>()))));
        }
    }

    public static class IoTryMaybe
    {
        public static IoTryMaybe<T> In<T>(this Io<Try<IMaybe<T>>> io)
        {
            return new IoTryMaybe<T>(io);    
        }       

        public static IoTryMaybe<T> ToIoTryMaybe<T>(this T t)
        {
            return new IoTryMaybe<T>(t);    
        }

        public static IoTryMaybe<T> ToIoTryMaybe<T>(this IMaybe<T> maybe)
        {
            return new IoTryMaybe<T>(maybe);
        }

        public static IoTryMaybe<T> ToIoTryMaybe<T>(this Try<IMaybe<T>> @try)
        {
            return new IoTryMaybe<T>(@try);
        }

        public static IoTryMaybe<T> ToIoTryMaybe<T>(this Try<T> @try)
        {
            return new IoTryMaybe<T>(@try.Select(t => t.ToMaybe()));
        }

        public static IoTryMaybe<T> ToIoTryMaybe<T>(this Io<T> io)
        {
            return io.Select(t => TryOps.Attempt(() => t.ToMaybe())).In();
        }

        public static IoTryMaybe<TResult> Select<TInitial, TResult>(this IoTryMaybe<TInitial> ioT,
            Func<TInitial, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoTryMaybe<TResult> SelectMany<TInitial, TResult>(this IoTryMaybe<TInitial> ioT,
            Func<TInitial, IoTryMaybe<TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoTryMaybe<TSelect> SelectMany<TInitial, TResult, TSelect>(
            this IoTryMaybe<TInitial> ioT, Func<TInitial, IoTryMaybe<TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoTryMaybe()));
        }
    }
}
