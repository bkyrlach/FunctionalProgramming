using System;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoTry<T>
    {
        public readonly Io<Try<T>> Out;
 
        public IoTry(Io<Try<T>> io)
        {
            Out = io;
        }

        public IoTry(Try<T> t) : this(Io.Apply(() => t))
        {
            
        }

        public IoTry(T t) : this(Try.Attempt(() => t))
        {
            
        }

        public IoTry<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoTry<TResult>(Out.Select(t => t.Select(f)));
        }

        public IoTry<TResult> Bind<TResult>(Func<T, IoTry<TResult>> f)
        {
            return new IoTry<TResult>(Out.SelectMany(t => t.Match(
               success: val => f(val).Out,
               failure: ex => ex.FailIo<TResult>().Out)));
        }
    }

    public static class IoTry
    {
        public static IoTry<T> ToIoTry<T>(this Io<Try<T>> io)
        {
            return new IoTry<T>(io);    
        }

        public static IoTry<T> ToIoTry<T>(this Io<T> io)
        {
            return new IoTry<T>(io.Select(t => Try.Attempt(() => t)));
        }

        public static IoTry<T> TryIo<T>(Func<T> f)
        {
            return new IoTry<T>(Io.Apply(() => Try.Attempt(f)));
        }

        public static IoTry<T> FailIo<T>(this Exception ex)
        {
            return new IoTry<T>(Io.Apply(() => ex.Fail<T>()));
        }

        public static IoTry<TResult> Select<TInitial, TResult>(this IoTry<TInitial> ioT, Func<TInitial, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoTry<TResult> SelectMany<TInitial, TResult>(this IoTry<TInitial> ioT,
            Func<TInitial, IoTry<TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoTry<TSelect> SelectMany<TInitial, TResult, TSelect>(this IoTry<TInitial> ioT,
            Func<TInitial, IoTry<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => TryIo(() => selector(a, b))));
        }
    }
}
