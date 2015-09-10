using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class StateEither<TState, TLeft, TRight>
    {
        public readonly State<TState, IEither<TLeft, TRight>> Out;

        public StateEither(State<TState, IEither<TLeft, TRight>> state)
        {
            Out = state;
        }

        public StateEither(IEither<TLeft, TRight> either)
            : this(either.Insert<TState, IEither<TLeft, TRight>>())
        {

        }

        public StateEither(TRight right)
            : this(new Right<TLeft, TRight>(right))
        {

        }

        public StateEither(TLeft left)
            : this(new Left<TLeft, TRight>(left))
        {

        }

        public StateEither<TState, TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new StateEither<TState, TLeft, TResult>(Out.Select(either => either.Select(f)));
        }

        public StateEither<TState, TLeft, TResult> Bind<TResult>(Func<TRight, StateEither<TState, TLeft, TResult>> f)
        {
            return new StateEither<TState, TLeft, TResult>(new State<TState, IEither<TLeft, TResult>>(s =>
            {
                var result = Out.Run(s);
                return result.Item2.Match(
                    left: l => Tuple.Create(result.Item1, l.AsLeft<TLeft, TResult>()),
                    right: r => f(r).Out.Run(result.Item1));
            }));
        }

        public StateEither<TState, TLeft, TRight> Or(StateEither<TState, TLeft, TRight> otherState)
        {
            return new StateEither<TState, TLeft, TRight>(new State<TState, IEither<TLeft, TRight>>(state =>
            {
                var res1 = Out.Run(state);                
                var e1 = res1.Item2;                
                return e1.Match(
                    right: val => Tuple.Create(res1.Item1, val.AsRight<TLeft, TRight>()),
                    left: _ => 
                    {
                        var res2 = otherState.Out.Run(state);
                        var e2 = res2.Item2;
                        return e2.Match(
                            right: val => Tuple.Create(res2.Item1, val.AsRight<TLeft, TRight>()),
                            left: err => Tuple.Create(res2.Item1, err.AsLeft<TLeft, TRight>()));
                    });
            }));
        }

        public StateEither<TState, TLeft, TRight> CombineTakeLeft<TOtherRight>(StateEither<TState, TLeft, TOtherRight> otherState)
        {
            return new StateEither<TState, TLeft, TRight>(
                from e1 in Out
                from e2 in otherState.Out
                select e1.CombineTakeLeft(e2));
        }

        public StateEither<TState, TLeft, TOtherRight> CombineTakeRight<TOtherRight>(StateEither<TState, TLeft, TOtherRight> otherState)
        {
            return new StateEither<TState, TLeft, TOtherRight>(
                from e1 in Out
                from e2 in otherState.Out
                select e1.CombineTakeRight(e2));
        }
    }

    public static class StateEither
    {
        public static StateEither<TState, TLeft, TRight> ToStateEither<TState, TLeft, TRight>(this State<TState, IEither<TLeft, TRight>> state)
        {
            return new StateEither<TState, TLeft, TRight>(state);
        }

        public static StateEither<TState, TLeft, TRight> ToStateEither<TState, TLeft, TRight>(this State<TState, TRight> state)
        {
            return new StateEither<TState, TLeft, TRight>(state.Select(right => (IEither<TLeft, TRight>)new Right<TLeft, TRight>(right)));
        }

        public static StateEither<TState, TLeft, TRight> ToStateEither<TState, TLeft, TRight>(this IEither<TLeft, TRight> either)
        {
            return new StateEither<TState, TLeft, TRight>(either);
        }

        public static StateEither<TState, TLeft, TRight> InsertLeft<TState, TLeft, TRight>(this TLeft t)
        {
            return new StateEither<TState, TLeft, TRight>(t);
        }

        public static StateEither<TState, TLeft, TRight> InsertRight<TState, TLeft, TRight>(this TRight t)
        {
            return new StateEither<TState, TLeft, TRight>(t);
        }

        public static StateEither<TState, TLeft, TResult> Select<TState, TLeft, TRight, TResult>(this StateEither<TState, TLeft, TRight> stateT, Func<TRight, TResult> f)
        {
            return stateT.FMap(f);
        }

        public static StateEither<TState, TLeft, TResult> SelectMany<TState, TLeft, TRight, TResult>(this StateEither<TState, TLeft, TRight> stateT, Func<TRight, StateEither<TState, TLeft, TResult>> f)
        {
            return stateT.Bind(f);
        }

        public static StateEither<TState, TLeft, TSelect> SelectMany<TState, TLeft, TRight, TResult, TSelect>(this StateEither<TState, TLeft, TRight> stateT, Func<TRight, StateEither<TState, TLeft, TResult>> f, Func<TRight, TResult, TSelect> selector)
        {
            return stateT.SelectMany(a => f(a).SelectMany(b => selector(a, b).InsertRight<TState, TLeft, TSelect>()));
        }

        public static StateEither<TState, TLeft, TRight> Or<TState, TLeft, TRight>(this StateEither<TState, TLeft, TRight> left, StateEither<TState, TLeft, TRight> right)
        {
            return left.Or(right);
        }

        public static StateEither<TState, TLeft, TRight> CombineTakeLeft<TState, TLeft, TRight, TOtherRight>(this StateEither<TState, TLeft, TRight> left, StateEither<TState, TLeft, TOtherRight> right)
        {
            return left.CombineTakeLeft(right);
        }

        public static StateEither<TState, TLeft, TOtherRight> CombineTakeRight<TState, TLeft, TRight, TOtherRight>(this StateEither<TState, TLeft, TRight> left, StateEither<TState, TLeft, TOtherRight> right)
        {
            return left.CombineTakeRight(right);
        }

        public static StateEither<TState, TLeft, IEnumerable<TRight>> Many<TState, TLeft, TRight>(this StateEither<TState, TLeft, TRight> stateT)
        {
            return
                (from e in stateT.Out
                 from result in e.Match(
                     left: err => Enumerable.Empty<TRight>().AsRight<TLeft, IEnumerable<TRight>>().Insert<TState, IEither<TLeft, IEnumerable<TRight>>>(),
                     right: h => stateT.Many().Select(t => h.LiftEnumerable().Concat(t)).Out)
                 select result)
                .ToStateEither<TState, TLeft, IEnumerable<TRight>>();
        }

        public static StateEither<TState, TLeft, IEnumerable<TRight>> Many1<TState, TLeft, TRight>(this StateEither<TState, TLeft, TRight> stateT)
        {
            return from first in stateT
                   from rest in stateT.Many()
                   select first.LiftEnumerable().Concat(rest);
        }
    }
}
