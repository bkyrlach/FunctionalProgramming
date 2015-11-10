using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class IoEnumerableMaybe<T>
    {
        public readonly Io<IEnumerable<IMaybe<T>>> Out;

        public IoEnumerableMaybe(Io<IEnumerable<IMaybe<T>>> io)
        {
            Out = io;
        }

        public IoEnumerableMaybe(IEnumerable<IMaybe<T>> maybes) : this(Io.Apply(() => maybes))
        {
            
        }

        public IoEnumerableMaybe(IMaybe<T> maybe) : this(maybe.LiftEnumerable())
        {
            
        }

        public IoEnumerableMaybe(T t) : this(t.ToMaybe())
        {
            
        }

        public IoEnumerableMaybe<T> Keep(Func<T, bool> predicate)
        {
            return new IoEnumerableMaybe<T>(Out.Select(maybes => maybes.Select(maybe => maybe.Where(predicate))));
        }

        public IoEnumerableMaybe<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoEnumerableMaybe<TResult>(Out.Select(maybes => maybes.Select(maybe => maybe.Select(f))));
        }

        public IoEnumerableMaybe<TResult> Bind<TResult>(Func<T, IoEnumerableMaybe<TResult>> f)
        {
            return new IoEnumerableMaybe<TResult>(Out.SelectMany(maybes => maybes.Select(maybe => maybe.Match(
                just: v => f(v).Out,
                nothing: () => Io.Apply(() => Enumerable.Empty<IMaybe<TResult>>())))
                .Sequence()
                .Select(x => x.SelectMany(BasicFunctions.Identity))));
        }
    }

    public static class IoEnumerableMaybe
    {
        public static IoEnumerableMaybe<T> ToIoEnumerableMaybe<T>(this Io<IEnumerable<IMaybe<T>>> io)
        {
            return new IoEnumerableMaybe<T>(io);
        }

        public static IoEnumerableMaybe<T> ToIoEnumerableMaybe<T>(this Io<T> io)
        {
            return new IoEnumerableMaybe<T>(io.Select(t => t.ToMaybe().LiftEnumerable()));
        }

        public static IoEnumerableMaybe<T> ToIoEnumerableMaybe<T>(this Io<IMaybe<T>> ioMaybe)
        {
            return new IoEnumerableMaybe<T>(ioMaybe.Select(maybe => maybe.LiftEnumerable()));
        }

        public static IoEnumerableMaybe<T> ToIoEnumerableMaybe<T>(this IEnumerable<IMaybe<T>> maybes)
        {
            return new IoEnumerableMaybe<T>(maybes);
        }

        public static IoEnumerableMaybe<T> ToIoEnumerableMaybe<T>(this IEnumerable<T> ts)
        {
            return new IoEnumerableMaybe<T>(ts.Select(t => t.ToMaybe()));
        }

        public static IoEnumerableMaybe<T> ToIoEnumerableMaybe<T>(this IMaybe<T> maybe)
        {
            return new IoEnumerableMaybe<T>(maybe);
        }

        public static IoEnumerableMaybe<T> ToIoEnumerableMaybe<T>(this T t)
        {
            return new IoEnumerableMaybe<T>(t);
        }

        public static IoEnumerableMaybe<TResult> Select<TInitial, TResult>(this IoEnumerableMaybe<TInitial> ioT,
            Func<TInitial, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoEnumerableMaybe<TResult> SelectMany<TInitial, TResult>(this IoEnumerableMaybe<TInitial> iot,
            Func<TInitial, IoEnumerableMaybe<TResult>> f)
        {
            return iot.Bind(f);
        }

        public static IoEnumerableMaybe<TSelect> SelectMany<TInitial, TResult, TSelect>(
            this IoEnumerableMaybe<TInitial> ioT, Func<TInitial, IoEnumerableMaybe<TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoEnumerableMaybe()));
        }

        public static IoEnumerableMaybe<T> Where<T>(this IoEnumerableMaybe<T> ioT, Func<T, bool> predicate)
        {
            return ioT.Keep(predicate);
        } 
    }
}