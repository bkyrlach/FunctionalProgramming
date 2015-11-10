using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public static class IoEitherMaybe
    {
        public static IoEitherMaybe<TLeft, TRight> ToIoEitherMaybe<TLeft, TRight>(this Io<IEither<TLeft, IMaybe<TRight>>> either)
        {
            return new IoEitherMaybe<TLeft, TRight>(either);
        }

        public static IoEitherMaybe<TLeft, TRight> ToIoEitherMaybe<TLeft, TRight>(this IEither<TLeft, TRight> m)
        {
            return new IoEitherMaybe<TLeft, TRight>(m.Select(r => r.ToMaybe()));
        }

        public static IoEitherMaybe<TLeft, TRight> ToIoEitherMaybe<TLeft, TRight>(this IMaybe<TRight> m)
        {
            return new IoEitherMaybe<TLeft, TRight>(m);
        }

        public static IoEitherMaybe<TLeft, TRight> ToIoEitherMaybe<TLeft, TRight>(this Io<TRight> io)
        {
            return new IoEitherMaybe<TLeft, TRight>(io.Select(right => right.ToMaybe().AsRight<TLeft, IMaybe<TRight>>()));
        }

        public static IoEitherMaybe<TLeft, TRight> AsIoLeftMaybe<TLeft, TRight>(this TLeft left)
        {
            return new IoEitherMaybe<TLeft, TRight>(left.AsLeft<TLeft, IMaybe<TRight>>());
        }

        public static IoEitherMaybe<TLeft, TRight> AsIoRightMaybe<TLeft, TRight>(this TRight right)
        {
            return new IoEitherMaybe<TLeft, TRight>(right.ToMaybe().AsRight<TLeft, IMaybe<TRight>>());
        }

        public static IoEitherMaybe<TLeft, TResult> Select<TLeft, TRight, TResult>(this IoEitherMaybe<TLeft, TRight> eitherT, Func<TRight, TResult> f)
        {
            return eitherT.FMap(f);
        }

        public static IoEitherMaybe<TLeft, TResult> SelectMany<TLeft, TRight, TResult>(this IoEitherMaybe<TLeft, TRight> eitherT, Func<TRight, IoEitherMaybe<TLeft, TResult>> f)
        {
            return eitherT.Bind(f);
        }

        public static IoEitherMaybe<TLeft, TSelect> SelectMany<TLeft, TRight, TResult, TSelect>(this IoEitherMaybe<TLeft, TRight> eitherT, Func<TRight, IoEitherMaybe<TLeft, TResult>> f, Func<TRight, TResult, TSelect> selector)
        {
            return eitherT.SelectMany(a => f(a).SelectMany(b => selector(a, b).AsIoRightMaybe<TLeft, TSelect>()));
        }

        public static IoEitherMaybe<TLeft, TRight> Where<TLeft, TRight>(this IoEitherMaybe<TLeft, TRight> eitherT, Func<TRight, bool> predicate)
        {
            return eitherT.Keep(predicate);
        }
    }

    public class IoEitherMaybe<TLeft, TRight>
    {
        public readonly Io<IEither<TLeft, IMaybe<TRight>>> Out;

        public IoEitherMaybe(Io<IEither<TLeft, IMaybe<TRight>>> io)
        {
            Out = io;
        }

        public IoEitherMaybe(IEither<TLeft, IMaybe<TRight>> either)
            : this(Io.Apply(() => either))
        {

        }

        public IoEitherMaybe(IMaybe<TRight> maybe)
            : this(maybe.AsRight<TLeft, IMaybe<TRight>>())
        {

        }

        public IoEitherMaybe(TRight val)
            : this(val.ToMaybe())
        {

        }

        public IoEitherMaybe<TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new IoEitherMaybe<TLeft, TResult>(Out.Select(either => either.Select(maybe => maybe.Select(f))));
        }

        public IoEitherMaybe<TLeft, TResult> Bind<TResult>(Func<TRight, IoEitherMaybe<TLeft, TResult>> f)
        {
            return new IoEitherMaybe<TLeft, TResult>(Out.SelectMany(either => either.Match(
                left: left => Io.Apply(() => left.AsLeft<TLeft, IMaybe<TResult>>()),
                right: maybe => maybe.Match(
                    just: val => f(val).Out,
                    nothing: () => Io.Apply(() => Maybe.Nothing<TResult>().AsRight<TLeft, IMaybe<TResult>>())))));
        }

        public IoEitherMaybe<TLeft, TRight> Keep(Func<TRight, bool> predicate)
        {
            return new IoEitherMaybe<TLeft, TRight>(Out.Select(either => either.Select(maybe => maybe.Where(predicate))));
        }
    }
}
