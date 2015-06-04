using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class IoMaybe<T>
    {
        private readonly Io<IMaybe<T>> _self; 

        public IoMaybe(Io<IMaybe<T>> io)
        {
            _self = io;        
        }

        public IoMaybe(T t) : this(Io.Apply(() => t.ToMaybe()))
        {
            
        }

        public IoMaybe<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new IoMaybe<TResult>(_self.Select(m => m.Select(f)));
        }

        public Io<IMaybe<T>> Out()
        {
            return _self;
        }
        
        public IoMaybe<TResult> Bind<TResult>(Func<T, IoMaybe<TResult>> f)
        {
            return new IoMaybe<TResult>(_self.SelectMany(m => m.Match(
                just: v => f(v).Out(),
                nothing: () => IoMaybe.NothingIo<TResult>().Out())));
        }
    }

    public static class IoMaybe
    {
        public static IoMaybe<T> ToIoMaybe<T>(this Io<IMaybe<T>> io)
        {
            return new IoMaybe<T>(io);
        }

        public static IoMaybe<T> ToIoMaybe<T>(this T t)
        {
            return new IoMaybe<T>(t);
        }

        public static IoMaybe<T> ToIoMaybe<T>(this Io<T> io)
        {
            return new IoMaybe<T>(io.Select(t => t.ToMaybe()));
        }

        public static IoMaybe<T> ToIoMaybe<T>(this IMaybe<T> m)
        {
            return new IoMaybe<T>(Io.Apply(() => m));
        }

        public static IoMaybe<T> NothingIo<T>()
        {
            return Maybe.Nothing<T>().ToIoMaybe();
        }

        public static IoMaybe<T> Where<T>(this IoMaybe<T> ioT, Func<T, bool> predicate)
        {
            return new IoMaybe<T>(ioT.Out().Select(m => m.Where(predicate)));
        }

        public static IoMaybe<TResult> Select<TInitial, TResult>(this IoMaybe<TInitial>  ioT,
            Func<TInitial, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoMaybe<TResult> SelectMany<TInitial, TResult>(this IoMaybe<TInitial> ioT,
            Func<TInitial, IoMaybe<TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoMaybe<TSelect> SelectMany<TInitial, TResult, TSelect>(this IoMaybe<TInitial> ioT,
            Func<TInitial, IoMaybe<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoMaybe()));
        }
    }
}
