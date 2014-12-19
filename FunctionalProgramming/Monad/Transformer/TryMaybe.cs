using System;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class TryMaybe<T>
    {
        private readonly Try<IMaybe<T>> _self;

        public TryMaybe(Try<IMaybe<T>> @try)
        {
            _self = @try;
        }

        public TryMaybe(IMaybe<T> maybe) : this(Try.Attempt(() => maybe))
        {
            
        }

        public TryMaybe(T val) : this(val.ToMaybe())
        {
            
        }

        public Try<IMaybe<T>> Out()
        {
            return _self;
        }

        public TryMaybe<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new TryMaybe<TResult>(_self.Select(maybe => maybe.Select(f)));
        }

        public TryMaybe<TResult> Bind<TResult>(Func<T, TryMaybe<TResult>> f)
        {
            return new TryMaybe<TResult>(_self.Match(
                failure: ex => ex.Fail<IMaybe<TResult>>(),
                success: maybe => maybe.Match(
                    just: val => f(val).Out(),
                    nothing: () => Try.Attempt(() => Maybe.Nothing<TResult>()))));
        }
    }

    public static class TryMaybe
    {
        public static TryMaybe<T> ToTryMaybe<T>(this Try<IMaybe<T>> @try)
        {
            return new TryMaybe<T>(@try);
        }

        public static TryMaybe<T> ToTryMaybe<T>(this T t) where T : class
        {
            return new TryMaybe<T>(t);
        }

        public static TryMaybe<T> ToTryMaybe<T>(this IMaybe<T> maybe)
        {
            return new TryMaybe<T>(maybe);
        }

        public static TryMaybe<T> ToTryMaybe<T>(this Try<T> @try) where T : class
        {
            return new TryMaybe<T>(@try.Select(t => t.ToMaybe()));
        }

        public static TryMaybe<TResult> Select<TInitial, TResult>(this TryMaybe<TInitial> tryT,
            Func<TInitial, TResult> f)
        {
            return tryT.FMap(f);
        }

        public static TryMaybe<TResult> SelectMany<TInitial, TResult>(this TryMaybe<TInitial> tryT,
            Func<TInitial, TryMaybe<TResult>> f)
        {
            return tryT.Bind(f);
        }

        public static TryMaybe<TSelect> SelectMany<TInitial, TResult, TSelect>(this TryMaybe<TInitial> tryT,
            Func<TInitial, TryMaybe<TResult>> f, Func<TInitial, TResult, TSelect> selector) where TSelect : class
        {
            return tryT.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToTryMaybe()));
        }
    }
}
