using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class StateTryEnumerable<TState, TValue>
    {
        private readonly State<TState, Try<IEnumerable<TValue>>> _self;

        public StateTryEnumerable(State<TState, Try<IEnumerable<TValue>>> self)
        {
            _self = self;
        }

        public StateTryEnumerable(Try<IEnumerable<TValue>> @try)
            : this(@try.Insert<TState, Try<IEnumerable<TValue>>>())
        {
            
        }

        public StateTryEnumerable(IEnumerable<TValue> enumerable)
            : this(Try.Attempt(() => enumerable))
        {
            
        }

        public State<TState, Try<IEnumerable<TValue>>> Out()
        {
            return _self;
        }

        public StateTryEnumerable<TState, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new StateTryEnumerable<TState, TResult>(_self.Select(@try => @try.Select(enumerable => enumerable.Select(f))));
        }

        public StateTryEnumerable<TState, TResult> Bind<TResult>(Func<TValue, StateTryEnumerable<TState, TResult>> f)
        {
            return new StateTryEnumerable<TState, TResult>(_self.SelectMany(@try => @try.Match(
                success: t => t.Select(v => f(v).Out()).Sequence().Select(xs => xs.Sequence().Select(ys => ys.SelectMany(BasicFunctions.Identity))),
                failure: ex => ex.Fail<IEnumerable<TResult>>().Insert<TState, Try<IEnumerable<TResult>>>())
            ));
        }

        public StateTryEnumerable<TState, TValue> Keep(Func<TValue, bool> predicate)
        {
            return new StateTryEnumerable<TState, TValue>(_self.Select(@try => @try.Select(enumerable => enumerable.Where(predicate))));
        }
    }

    public static class StateTryEnumerable
    {
        public static StateTryEnumerable<TState, TValue> ToStateTryEnumerable<TState, TValue>(this State<TState, Try<IEnumerable<TValue>>> state)
        {
            return new StateTryEnumerable<TState, TValue>(state);
        }

        public static StateTryEnumerable<TState, TValue> ToStateTryEnumerable<TState, TValue>(this Try<IEnumerable<TValue>> @try)
        {
            return new StateTryEnumerable<TState, TValue>(@try);
        }

        public static StateTryEnumerable<TState, TValue> ToStateTryEnumerable<TState, TValue>(this IEnumerable<TValue> enumerable)
        {
            return new StateTryEnumerable<TState, TValue>(enumerable);
        }

        public static StateTryEnumerable<TState, TValue> ToStateTryEnumerable<TState, TValue>(this TValue v)
        {
            return new StateTryEnumerable<TState, TValue>(v.LiftEnumerable());
        }

        public static StateTryEnumerable<TState, TValue> ToStateTryEnumerable<TState, TValue>(this State<TState, TValue> state)
        {
            return new StateTryEnumerable<TState, TValue>(state.Select(v => Try.Attempt(() => v.LiftEnumerable())));
        }

        public static StateTryEnumerable<TState, TValue> ToStateTryEnumerable<TState, TValue>(this Try<TValue> @try)
        {
            return new StateTryEnumerable<TState, TValue>(@try.Select(v => v.LiftEnumerable()));
        }

        public static StateTryEnumerable<TState, TResult> Select<TState, TValue, TResult>(
            this StateTryEnumerable<TState, TValue> state, Func<TValue, TResult> f)
        {
            return state.FMap(f);
        }

        public static StateTryEnumerable<TState, TResult> SelectMany<TState, TValue, TResult>(
            this StateTryEnumerable<TState, TValue> state, Func<TValue, StateTryEnumerable<TState, TResult>> f)
        {
            return state.Bind(f);
        }

        public static StateTryEnumerable<TState, TSelect> SelectMany<TState, TValue, TResult, TSelect>(
            this StateTryEnumerable<TState, TValue> state, Func<TValue, StateTryEnumerable<TState, TResult>> f, Func<TValue, TResult, TSelect> selector)
        {
            return state.SelectMany(a => f(a).SelectMany(b =>  new StateTryEnumerable<TState, TSelect>(selector(a, b).LiftEnumerable())));
        }

        public static StateTryEnumerable<TState, TValue> Where<TState, TValue>(
            this StateTryEnumerable<TState, TValue> state, Func<TValue, bool> predicate)
        {
            return state.Keep(predicate);
        } 
    }
}
