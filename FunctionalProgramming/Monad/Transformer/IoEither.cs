using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public static class IoEither
    {
        //public static Io<IEither<TLeft, TRight>> AsLeftIo<TLeft, TRight>(this TLeft t)
        //{
        //    return Io<IEither<TLeft, TRight>>.Apply(() => t.AsLeft<TLeft, TRight>());
        //}

        //public static Io<IEither<TLeft, TRight>> AsRightIo<TLeft, TRight>(this TRight t)
        //{
        //    return Io<IEither<TLeft, TRight>>.Apply(() => t.AsRight<TLeft, TRight>());
        //}

        //public static Io<IEither<TLeft, TResult>> Select<TLeft, TRight, TResult>(this Io<IEither<TLeft, TRight>> ioT,
        //    Func<TRight, TResult> f)
        //{
        //    return ioT.Select(e => e.Select(f));
        //}

        //public static Io<IEither<TLeft, TResult>> SelectMany<TLeft, TRight, TResult>(
        //    this Io<IEither<TLeft, TRight>> ioT, Func<TRight, Io<IEither<TLeft, TResult>>> f)
        //{
        //    return ioT.SelectMany(e => e.Match(
        //        left: AsLeftIo<TLeft, TResult>,
        //        right: f));
        //}

        //public static Io<IEither<TLeft, TSelect>> SelectMany<TLeft, TRight, TResult, TSelect>(
        //    this Io<IEither<TLeft, TRight>> ioT, Func<TRight, Io<IEither<TLeft, TResult>>> f,
        //    Func<TRight, TResult, TSelect> selector)
        //{
        //    return ioT.SelectMany(a => f(a).SelectMany(b => selector(a, b).AsRightIo<TLeft, TSelect>()));
        //}
    }
}
