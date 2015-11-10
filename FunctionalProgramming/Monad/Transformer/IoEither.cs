using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class IoEither<TLeft, TRight>
    {
        public readonly Io<IEither<TLeft, TRight>> Out;
 
        public IoEither(Io<IEither<TLeft, TRight>> io)
        {
            Out = io;
        }

        public IoEither(IEither<TLeft, TRight> either) : this(Io.Apply(() => either))
        {
            
        }

        public IoEither(TRight rightVal) : this(rightVal.AsRight<TLeft, TRight>())
        {
            
        }

        public IoEither(TLeft leftVal) : this(leftVal.AsLeft<TLeft, TRight>())
        {
            
        }

        public IoEither<TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new IoEither<TLeft, TResult>(Out.Select(either => either.Select(f)));
        }

        public IoEither<TLeft, TResult> Bind<TResult>(Func<TRight, IoEither<TLeft, TResult>> f)
        {
            return new IoEither<TLeft, TResult>(Out.SelectMany(either => either.Match(
                left: l => Io.Apply(() => l.AsLeft<TLeft, TResult>()),
                right: r => f(r).Out)));
        }
    }

    public static class IoEither
    {
        public static IoEither<TLeft, TRight> ToIoEither<TLeft, TRight>(this Io<IEither<TLeft, TRight>> io)
        {
            return new IoEither<TLeft, TRight>(io);    
        }

        public static IoEither<TLeft, TRight> ToIoEither<TLeft, TRight>(this Io<TRight> io)
        {
            return new IoEither<TLeft, TRight>(io.Select(right => right.AsRight<TLeft, TRight>()));
        }

        public static IoEither<TLeft, TRight> ToIoEither<TLeft, TRight>(this IEither<TLeft, TRight> either)
        {
            return new IoEither<TLeft, TRight>(either);
        }

        public static IoEither<TLeft, TRight> AsLeftIo<TLeft, TRight>(this TLeft t)
        {
            return new IoEither<TLeft, TRight>(t);
        }

        public static IoEither<TLeft, TRight> AsRightIo<TLeft, TRight>(this TRight t)
        {
            return new IoEither<TLeft, TRight>(t);
        }

        public static IoEither<TLeft, TResult> Select<TLeft, TRight, TResult>(this IoEither<TLeft, TRight> ioT,
            Func<TRight, TResult> f)
        {
            return ioT.FMap(f);
        }

        public static IoEither<TLeft, TResult> SelectMany<TLeft, TRight, TResult>(
            this IoEither<TLeft, TRight> ioT, Func<TRight, IoEither<TLeft, TResult>> f)
        {
            return ioT.Bind(f);
        }

        public static IoEither<TLeft, TSelect> SelectMany<TLeft, TRight, TResult, TSelect>(
            this IoEither<TLeft, TRight> ioT, Func<TRight, IoEither<TLeft, TResult>> f,
            Func<TRight, TResult, TSelect> selector)
        {
            return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).AsRightIo<TLeft, TSelect>()));
        }
    }
}
