using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class TryEnumerable<T>
    {
        private readonly Try<IEnumerable<T>> _self;


        public TryEnumerable(Try<IEnumerable<T>> self)
        {
            _self = self;
        }

        public TryEnumerable(IEnumerable<T> enumerable)
            : this(Try.Attempt(() => enumerable))
        {
            
        }

        public TryEnumerable(T t)
            : this(t.LiftEnumerable())
        {
            
        }

        public Try<IEnumerable<T>> Out()
        {
            return _self;
        }

        public TryEnumerable<TResult> FMap<TResult>(Func<T, TResult> f)
        {
            return new TryEnumerable<TResult>(_self.Select(enumerable => enumerable.Select(f)));
        }

        public TryEnumerable<TResult> Bind<TResult>(Func<T, TryEnumerable<TResult>> f)
        {
            return new TryEnumerable<TResult>(_self.Match(
                success: enumerable => enumerable.Select(t => f(t).Out()).Sequence().Select(e => e.SelectMany(BasicFunctions.Identity)),
                failure: ex => ex.Fail<IEnumerable<TResult>>()));
        }

        public TryEnumerable<T> Keep(Func<T, bool> predicate)
        {
            return new TryEnumerable<T>(_self.Select(enumerable => enumerable.Where(predicate)));
        } 
    }

    public static class TryEnumerable
    {
        public static TryEnumerable<T> ToTryEnumerable<T>(this Try<IEnumerable<T>> @try)
        {
            return new TryEnumerable<T>(@try);
        }

        public static TryEnumerable<T> ToTryEnumerable<T>(this IEnumerable<T> enumerable)
        {
            return new TryEnumerable<T>(enumerable);
        }

        public static TryEnumerable<T> ToTryEnumerable<T>(this T t)
        {
            return new TryEnumerable<T>(t);
        }

        public static TryEnumerable<T> ToTryEnumerable<T>(this Try<T> @try)
        {
            return new TryEnumerable<T>(@try.Select(t => t.LiftEnumerable()));
        }

        public static TryEnumerable<TResult> Select<TValue, TResult>(this TryEnumerable<TValue> @try, Func<TValue, TResult> f)
        {
            return @try.FMap(f);
        }

        public static TryEnumerable<TResult> SelectMany<TValue, TResult>(this TryEnumerable<TValue> @try,
            Func<TValue, TryEnumerable<TResult>> f)
        {
            return @try.Bind(f);
        }

        public static TryEnumerable<TSelect> SelectMany<TValue, TResult, TSelect>(this TryEnumerable<TValue> @try,
            Func<TValue, TryEnumerable<TResult>> f, Func<TValue, TResult, TSelect> selector)
        {
            return @try.SelectMany(a => f(a).SelectMany(b => selector(a, b).ToTryEnumerable()));
        }

        public static TryEnumerable<T> Where<T>(this TryEnumerable<T> @try, Func<T, bool> predicate)
        {
            return @try.Keep(predicate);
        } 
    }
}
