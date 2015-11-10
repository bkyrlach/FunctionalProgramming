using System;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Monad.Transformer
{
    public class ReaderTry<TEnvironment, TValue>
    {
        public readonly Reader<TEnvironment, Try<TValue>> Out;

        public ReaderTry(Reader<TEnvironment, Try<TValue>> self)
        {
            Out = self;
        }

        public ReaderTry(Try<TValue> @try) : this(new Reader<TEnvironment, Try<TValue>>(env => @try))
        {
            
        }

        public ReaderTry(TValue value) : this(Try.Attempt(() => value))
        {
            
        }

        public ReaderTry<TEnvironment, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new ReaderTry<TEnvironment, TResult>(Out.Select(@try => @try.Select(f)));
        }

        public ReaderTry<TEnvironment, TResult> Bind<TResult>(Func<TValue, ReaderTry<TEnvironment, TResult>> f)
        {
            return new ReaderTry<TEnvironment, TResult>(Out.SelectMany(@try => @try.Match(
                success: v => f(v).Out,
                failure: ex => new Reader<TEnvironment, Try<TResult>>(env => ex.Fail<TResult>()))));
        }
    }

    public static class ReaderTry
    {
        public static ReaderTry<TEnvironment, TValue> ToReaderTry<TEnvironment, TValue>(this TValue val)
        {
            return new ReaderTry<TEnvironment, TValue>(val);
        }

        public static ReaderTry<TEnvironment, TValue> ToReaderTry<TEnvironment, TValue>(this Try<TValue> @try)
        {
            return new ReaderTry<TEnvironment, TValue>(@try);
        }

        public static ReaderTry<TEnvironment, TValue> ToReaderTry<TEnvironment, TValue>(this 
            Reader<TEnvironment, Try<TValue>> reader)
        {
            return new ReaderTry<TEnvironment, TValue>(reader);
        }

        public static ReaderTry<TEnvironment, TValue> ToReaderTry<TEnvironment, TValue>(this 
            Reader<TEnvironment, TValue> reader)
        {
            return new ReaderTry<TEnvironment, TValue>(reader.Select(v => Try.Attempt(() => v)));
        }

        public static ReaderTry<TEnvironment, TResult> Select<TEnvironment, TValue, TResult>(
            this ReaderTry<TEnvironment, TValue> readerT, Func<TValue, TResult> f)
        {
            return readerT.FMap(f);
        }

        public static ReaderTry<TEnvironment, TResult> SelectMany<TEnvironment, TValue, TResult>(
            this ReaderTry<TEnvironment, TValue> readerT, Func<TValue, ReaderTry<TEnvironment, TResult>> f)
        {
            return readerT.Bind(f);
        }

        public static ReaderTry<TEnvironment, TSelect> SelectMany<TEnvironment, TValue, TResult, TSelect>(
            this ReaderTry<TEnvironment, TValue> readerT, Func<TValue, ReaderTry<TEnvironment, TResult>> f,
            Func<TValue, TResult, TSelect> selector)
        {
            return readerT.SelectMany(a => f(a).SelectMany(b => new ReaderTry<TEnvironment, TSelect>(selector(a, b))));
        }
    }
}
