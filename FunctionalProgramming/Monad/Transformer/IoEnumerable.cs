using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Transformer
{
    public class IoEnumerable<T>
    {
        private readonly Io<IEnumerable<T>> _self;

        public IoEnumerable(Io<IEnumerable<T>> io)
        {
            _self = io;
        }

        public IoEnumerable(IEnumerable<T> ts) : this(Io<IEnumerable<T>>.Apply(() => ts))
        {
            
        }

        public Io<IEnumerable<T>> Out()
        {
            return _self;
        }

        public IoEnumerable<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoEnumerable<TResult>(_self.Select(ts => ts.Select(f)));
        }

        public IoEnumerable<TResult> Bind<TResult>(Func<T, IoEnumerable<TResult>> f)
        {
            return new IoEnumerable<TResult>(_self.SelectMany(ts => ts.Select(t => f(t).Out()).Sequence().Select(x => x.SelectMany(BasicFunctions.Identity))));
        }
    }

    public static class IoEnumerable
    {
        public static IoEnumerable<T> In<T>(this Io<IEnumerable<T>> io)
        {
            return new IoEnumerable<T>(io);
        }

        public static IoEnumerable<T> ToIoEnumerable<T>(this Io<T> io)
        {
            return new IoEnumerable<T>(io.Select(t => t.LiftEnumerable()));
        }

        public static IoEnumerable<T> ToIoEnumerable<T>(this IEnumerable<T> ts)
        {
            return new IoEnumerable<T>(Io<IEnumerable<T>>.Apply(() => ts));
        }

        public static IoEnumerable<T> ToIoEnumerable<T>(this T t)
        {
            return new IoEnumerable<T>(t.LiftEnumerable());
        }

        public static IoEnumerable<TResult> Select<TInitial, TResult>(this IoEnumerable<TInitial> ioT,
            Func<TInitial, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoEnumerable<TResult> SelectMany<TInitial, TResult>(this IoEnumerable<TInitial> ioT,
            Func<TInitial, IoEnumerable<TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoEnumerable<TSelect> SelectMany<TInitial, TResult, TSelect>(this IoEnumerable<TInitial> ioT,
            Func<TInitial, IoEnumerable<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoEnumerable()));
        }
    }
}
