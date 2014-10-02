using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public static class StateEither
    {
        //public static State<TState, IEither<TLeft, TRight>> InsertLeft<TState, TLeft, TRight>(this TLeft t)
        //{
        //    return t.AsLeft<TLeft, TRight>().Insert<TState, IEither<TLeft, TRight>>();
        //}

        //public static State<TState, IEither<TLeft, TRight>> InsertRight<TState, TLeft, TRight>(this TRight t)
        //{
        //    return t.AsRight<TLeft, TRight>().Insert<TState, IEither<TLeft, TRight>>();
        //}

        //public static State<TState, IEither<TLeft, TResult>> Select<TState, TLeft, TRight, TResult>(this State<TState, IEither<TLeft, TRight>> stateT, Func<TRight, TResult> f)
        //{
        //    return stateT.Select(e => e.Select(f));
        //}

        //public static State<TState, IEither<TLeft, TResult>> SelectMany<TState, TLeft, TRight, TResult>(
        //    this State<TState, IEither<TLeft, TRight>> stateT, Func<TRight, State<TState, IEither<TLeft, TResult>>> f)
        //{
        //    return stateT.SelectMany(e => e.Match(
        //        left: l => l.InsertLeft<TState, TLeft, TResult>(),
        //        right: f));
        //}

        //public static State<TState, IEither<TLeft, TSelect>> SelectMany<TState, TLeft, TRight, TResult, TSelect>(
        //    this State<TState, IEither<TLeft, TRight>> stateT, Func<TRight, State<TState, IEither<TLeft, TResult>>> f,
        //    Func<TRight, TResult, TSelect> selector)
        //{
        //    return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).InsertRight<TState, TLeft, TSelect>()));
        //}
    }
}
