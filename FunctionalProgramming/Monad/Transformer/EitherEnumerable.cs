using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Transformer
{
    public class EitherEnumerable<TLeft, TRight>
    {
        public readonly IEither<TLeft, IEnumerable<TRight>> Out;
 
        public EitherEnumerable(IEither<TLeft, IEnumerable<TRight>> either)
        {
            Out = either;
        }

        public EitherEnumerable(IEnumerable<TRight> xs) : this(xs.AsRight<TLeft, IEnumerable<TRight>>())
        {
            
        }

        public EitherEnumerable(TRight r) : this(r.LiftEnumerable())
        {
            
        }

        public EitherEnumerable<TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new EitherEnumerable<TLeft, TResult>(Out.Select(xs => xs.Select(f)));
        }

        public EitherEnumerable<TLeft, TResult> Bind<TResult>(Func<TRight, EitherEnumerable<TLeft, TResult>> f)
        {
            return new EitherEnumerable<TLeft, TResult>(Out.Match(
                right: xs => xs.Select(x => f(x).Out).Sequence().Select(ys => ys.SelectMany(BasicFunctions.Identity)),
                left: l => l.AsLeft<TLeft, IEnumerable<TResult>>()));
        } 
    }

    public static class EitherEnumerable
    {
        public static EitherEnumerable<TLeft, TResult> Select<TLeft, TRight, TResult>(this EitherEnumerable<TLeft, TRight> eitherT, Func<TRight, TResult> f)
        {
            return eitherT.FMap(f);
        }

        public static EitherEnumerable<TLeft, TResult> SelectMany<TLeft, TRight, TResult>(
            this EitherEnumerable<TLeft, TRight> eitherT, Func<TRight, EitherEnumerable<TLeft, TResult>> f)
        {
            return eitherT.Bind(f);
        }

        public static EitherEnumerable<TLeft, TSelect> SelectMany<TLeft, TRight, TResult, TSelect>(
            this EitherEnumerable<TLeft, TRight> eitherT, Func<TRight, EitherEnumerable<TLeft, TResult>> f,
            Func<TRight, TResult, TSelect> selector)
        {
            return eitherT.SelectMany(a => f(a).SelectMany(b => new EitherEnumerable<TLeft, TSelect>(selector(a, b))));
        } 
    }
}
