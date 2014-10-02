using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoEnumerableTry<T>
    {
        private readonly Io<IEnumerable<Try<T>>> _self;

        public IoEnumerableTry(Io<IEnumerable<Try<T>>> io)
        {
            _self = io;
        }

        public IoEnumerableTry(IEnumerable<Try<T>> tries) : this(Io<IEnumerable<Try<T>>>.Apply(() => tries))
        {
            
        }

        public IoEnumerableTry(Func<T> toAttempt) : this(Io<IEnumerable<Try<T>>>.Apply(() => TryOps.Attempt(toAttempt).LiftEnumerable()))
        {
            
        }

        public Io<IEnumerable<Try<T>>> Out()
        {
            return _self;
        }

        public IoEnumerableTry<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoEnumerableTry<TResult>(_self.Select(tries => tries.Select(@try => @try.Select(f))));
        }

        public IoEnumerableTry<TResult> Bind<TResult>(Func<T, IoEnumerableTry<TResult>> f)
        {
            return new IoEnumerableTry<TResult>(_self.SelectMany(tries => tries.Select(@try => @try.Match(
                success: val => f(val).Out(),
                failure: ex => Io<IEnumerable<Try<TResult>>>.Apply(() => ex.Fail<TResult>().LiftEnumerable())))
                .Sequence()
                .Select(x => x.SelectMany(BasicFunctions.Identity))));
        }
    }

    public static class IoEnumerableTry
    {
        public static IoEnumerableTry<T> In<T>(this Io<IEnumerable<Try<T>>> io)
        {
            return new IoEnumerableTry<T>(io);
        }

        public static IoEnumerableTry<T> ToIoEnumerableTry<T>(this Io<T> io)
        {
            return new IoEnumerableTry<T>(io.Select(t => TryOps.Attempt(() => t).LiftEnumerable()));
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
            return ioT.SelectMany(a => f(a).SelectMany(b => new IoEnumerableTry<TSelect>(Io<IEnumerable<Try<TSelect>>>.Apply(() => TryOps.Attempt(() => selector(a, b)).LiftEnumerable()))));
        }
    }
}
