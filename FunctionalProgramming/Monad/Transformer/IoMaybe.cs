using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class IoMaybe<T>
    {
        public readonly Io<IMaybe<T>> Out; 

        public IoMaybe(Io<IMaybe<T>> io)
        {
            Out = io;        
        }

        public IoMaybe(T t)
        {
            Out = Io.Apply(() => t.ToMaybe());
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
            return new IoMaybe<T>(ioT.Out.Select(m => m.Where(predicate)));
        }

        public static IoMaybe<TResult> Select<TInitial, TResult>(this IoMaybe<TInitial>  ioT,
            Func<TInitial, TResult> f)
        {
            return new IoMaybe<TResult>(ioT.Out.Select(m => m.Select(f)));
        }

        public static IoMaybe<TResult> SelectMany<TInitial, TResult>(this IoMaybe<TInitial> ioT,
            Func<TInitial, IoMaybe<TResult>> f)
        {
            return new IoMaybe<TResult>(ioT.Out.SelectMany(m => m.Match(
                just: v => f(v).Out,
                nothing: () => NothingIo<TResult>().Out)));
        }

        public static IoMaybe<TSelect> SelectMany<TInitial, TResult, TSelect>(this IoMaybe<TInitial> ioT,
            Func<TInitial, IoMaybe<TResult>> f, Func<TInitial, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToIoMaybe()));
        }
    }
}
