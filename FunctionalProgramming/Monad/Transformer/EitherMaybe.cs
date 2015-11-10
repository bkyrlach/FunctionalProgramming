using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public static class EitherMaybe
    {
        public static EitherMaybe<TLeft, TRight> ToEitherMaybe<TLeft, TRight>(this IEither<TLeft, IMaybe<TRight>> either)
        {
            return new EitherMaybe<TLeft, TRight>(either);
        }

        public static EitherMaybe<TLeft, TRight> ToEitherMaybe<TLeft, TRight>(this IEither<TLeft, TRight> m)
        {
            return new EitherMaybe<TLeft, TRight>(m.Select(r => r.ToMaybe()));
        }

        public static EitherMaybe<TLeft, TRight> ToEitherMaybe<TLeft, TRight>(this IMaybe<TRight> m)
        {
            return new EitherMaybe<TLeft, TRight>(m);
        }

        public static EitherMaybe<TLeft, TRight> AsLeftMaybe<TLeft, TRight>(this TLeft left)
        {
            return new EitherMaybe<TLeft, TRight>(left.AsLeft<TLeft, IMaybe<TRight>>());
        }

        public static EitherMaybe<TLeft, TRight> AsRightMaybe<TLeft, TRight>(this TRight right)
        {
            return new EitherMaybe<TLeft, TRight>(right.ToMaybe().AsRight<TLeft, IMaybe<TRight>>());
        }

        public static EitherMaybe<TLeft, TResult> Select<TLeft, TRight, TResult>(this EitherMaybe<TLeft, TRight> eitherT, Func<TRight, TResult> f)
        {
            return eitherT.FMap(f);
        }

        public static EitherMaybe<TLeft, TResult> SelectMany<TLeft, TRight, TResult>(this EitherMaybe<TLeft, TRight> eitherT, Func<TRight, EitherMaybe<TLeft, TResult>> f)
        {
            return eitherT.Bind(f);
        }

        public static EitherMaybe<TLeft, TSelect> SelectMany<TLeft, TRight, TResult, TSelect>(this EitherMaybe<TLeft, TRight> eitherT, Func<TRight, EitherMaybe<TLeft, TResult>> f, Func<TRight, TResult, TSelect> selector)
        {
            return eitherT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToMaybe().AsRight<TLeft, IMaybe<TSelect>>().ToEitherMaybe()));
        }

        public static EitherMaybe<TLeft, TRight> Where<TLeft, TRight>(this EitherMaybe<TLeft, TRight> eitherT, Func<TRight, bool> predicate)
        {
            return eitherT.Keep(predicate);
        }
    }

    public class EitherMaybe<TLeft, TRight>
    {
        public readonly IEither<TLeft, IMaybe<TRight>> Out;

        public EitherMaybe(IEither<TLeft, IMaybe<TRight>> either)
        {
            Out = either;
        }

        public EitherMaybe(IMaybe<TRight> maybe)
            : this(maybe.AsRight<TLeft, IMaybe<TRight>>())
        {

        }

        public EitherMaybe(TRight val)
            : this(val.ToMaybe())
        {

        }

        public EitherMaybe<TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new EitherMaybe<TLeft, TResult>(Out.Select(maybe => maybe.Select(f)));
        }

        public EitherMaybe<TLeft, TResult> Bind<TResult>(Func<TRight, EitherMaybe<TLeft, TResult>> f)
        {
            return new EitherMaybe<TLeft, TResult>(Out.Match(
                left: left => left.AsLeft<TLeft, IMaybe<TResult>>(),
                right: maybe => maybe.Match(
                    just: val => f(val).Out,
                    nothing: () => Maybe.Nothing<TResult>().AsRight<TLeft, IMaybe<TResult>>())));
        }

        public EitherMaybe<TLeft, TRight> Keep(Func<TRight, bool> predicate)
        {
            return new EitherMaybe<TLeft, TRight>(Out.Select(maybe => maybe.Where(predicate)));
        }

    }
}
