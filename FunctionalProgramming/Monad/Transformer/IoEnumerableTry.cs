using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoEnumerableTry<T>
    {
        public readonly Io<IEnumerable<Try<T>>> Out;

        public IoEnumerableTry(Io<IEnumerable<Try<T>>> io)
        {
            Out = io;
        }

        public IoEnumerableTry(IEnumerable<Try<T>> tries) : this(Io.Apply(() => tries))
        {
            
        }

        public IoEnumerableTry(Func<T> toAttempt) : this(Io.Apply(() => Try.Attempt(toAttempt).LiftEnumerable()))
        {
            
        }

        public IoEnumerableTry<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoEnumerableTry<TResult>(Out.Select(tries => tries.Select(@try => @try.Select(f))));
        }

        public IoEnumerableTry<TResult> Bind<TResult>(Func<T, IoEnumerableTry<TResult>> f)
        {
            return new IoEnumerableTry<TResult>(Out.SelectMany(tries => tries.Select(@try => @try.Match(
                success: val => f(val).Out,
                failure: ex => Io.Apply(() => ex.Fail<TResult>().LiftEnumerable())))
                .Sequence()
                .Select(x => x.SelectMany(BasicFunctions.Identity))));
        }
    }

    public static class IoEnumerableTry
    {
        public static IoEnumerableTry<T> ToIoEnumerableTry<T>(this Io<IEnumerable<Try<T>>> io)
        {
            return new IoEnumerableTry<T>(io);
        }

        public static IoEnumerableTry<T> ToIoEnumerableTry<T>(this Io<T> io)
        {
            return new IoEnumerableTry<T>(io.Select(t => Try.Attempt(() => t).LiftEnumerable()));
        }

        public static IoEnumerableTry<T> ToIoEnumerableTry<T>(this Try<T> @try)
        {
            return new IoEnumerableTry<T>(@try.LiftEnumerable());
        }

        public static IoEnumerableTry<T> ToIoEnumerableTry<T>(this IEnumerable<T> xs)
        {
            return new IoEnumerableTry<T>(xs.Select(t => Try.Attempt(() => t)));
        }

        public static IoEnumerableTry<TResult> Select<TInitial, TResult>(this IoEnumerableTry<TInitial> ioT,
            Func<TInitial, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoEnumerableTry<TResult> SelectMany<TInitial, TResult>(this IoEnumerableTry<TInitial> ioT,
            Func<TInitial, IoEnumerableTry<TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoEnumerableTry<TSelect> SelectMany<TInitial, TResult, TSelect>(
            this IoEnumerableTry<TInitial> ioT, Func<TInitial, IoEnumerableTry<TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => new IoEnumerableTry<TSelect>(Io.Apply(() => Try.Attempt(() => selector(a, b)).LiftEnumerable()))));
        }
    }
}
