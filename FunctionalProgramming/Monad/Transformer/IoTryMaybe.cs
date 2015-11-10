using System;
using System.Runtime.Remoting.Messaging;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoTryMaybe<T>
    {
        public readonly Io<Try<IMaybe<T>>> Out;

        public IoTryMaybe(Io<Try<IMaybe<T>>> io)
        {
            Out = io;
        }

        public IoTryMaybe(Try<IMaybe<T>> aTry)
            : this(Io.Apply(() => aTry))
        {

        }

        public IoTryMaybe(IMaybe<T> m)
            : this(Io.Apply(() => Try.Attempt(() => m)))
        {

        }

        public IoTryMaybe(T t)
            : this(Io.Apply(() => Try.Attempt(() => t.ToMaybe())))
        {
        }

        public IoTryMaybe<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoTryMaybe<TResult>(Out.Select(t => t.Select(m => m.Select(f))));
        }

        public IoTryMaybe<TResult> Bind<TResult>(Func<T, IoTryMaybe<TResult>> f)
        {
            return new IoTryMaybe<TResult>(Out.SelectMany(t => t.Match(
                success: m => m.Match(
                    just: v => f(v).Out,
                    nothing: () => Io.Apply(() => Try.Attempt(() => Maybe.Nothing<TResult>()))),
                failure: ex => Io.Apply(() => ex.Fail<IMaybe<TResult>>()))));
        }

        public IoTryMaybe<T> Keep(Func<T, bool> predicate)
        {
            return new IoTryMaybe<T>(Out.Select(@try => @try.Select(maybe => maybe.Where(predicate))));
        }
    }

    public static class IoTryMaybe
    {
        public static IoTryMaybe<T> ToIoTryMaybe<T>(this Io<Try<IMaybe<T>>> io)
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
            return io.Select(t => Try.Attempt(() => t.ToMaybe())).ToIoTryMaybe();
        }

        public static IoTryMaybe<T> ToIoTryMaybe<T>(this Io<Try<T>> io)
        {
            return new IoTryMaybe<T>(io.Select(@try => @try.Select(t => t.ToMaybe())));
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

        public static IoTryMaybe<T> Where<T>(this IoTryMaybe<T> ioT, Func<T, bool> predicate)
        {
            return ioT.Keep(predicate);
        }
    }
}
