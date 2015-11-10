using System;

namespace FunctionalProgramming.Monad.Transformer
{
    public sealed class ReaderEither<TEnvironment, TLeft, TRight>
    {
        public readonly Reader<TEnvironment, IEither<TLeft, TRight>> Out;

        public ReaderEither(Reader<TEnvironment, IEither<TLeft, TRight>> self)
        {
            Out = self;
        }

        public ReaderEither(IEither<TLeft, TRight> either) : this(new Reader<TEnvironment, IEither<TLeft, TRight>>(env => either))
        {
            
        }

        public ReaderEither(TRight val) : this(val.AsRight<TLeft, TRight>())
        {
            
        }

        public ReaderEither(TLeft val) : this(val.AsLeft<TLeft, TRight>())
        {
            
        }

        public ReaderEither<TEnvironment, TLeft, TResult> FMap<TResult>(Func<TRight, TResult> f)
        {
            return new ReaderEither<TEnvironment, TLeft, TResult>(Out.Select(either => either.Select(f)));
        }

        public ReaderEither<TEnvironment, TLeft, TResult> Bind<TResult>(
            Func<TRight, ReaderEither<TEnvironment, TLeft, TResult>> f)
        {
            return new ReaderEither<TEnvironment, TLeft, TResult>(Out.SelectMany(either => either.Match(
                left: l => new Reader<TEnvironment, IEither<TLeft, TResult>>(env => l.AsLeft<TLeft, TResult>()),
                right: r => f(r).Out)));
        }
    }

    public static class ReaderEither
    {
        public static ReaderEither<TEnvironment, TLeft, TRight> ToReaderEither<TEnvironment, TLeft, TRight>(
            this Reader<TEnvironment, IEither<TLeft, TRight>> reader)
        {
            return new ReaderEither<TEnvironment, TLeft, TRight>(reader);
        }

        public static ReaderEither<TEnvironment, TLeft, TRight> ToReaderEither<TEnvironment, TLeft, TRight>(
            this IEither<TLeft, TRight> either)
        {
            return new ReaderEither<TEnvironment, TLeft, TRight>(either);
        }

        public static ReaderEither<TEnvironment, TLeft, TRight> ToReaderEither<TEnvironment, TLeft, TRight>(
            this Reader<TEnvironment, TRight> reader)
        {
            return new ReaderEither<TEnvironment, TLeft, TRight>(reader.Select(r => r.AsRight<TLeft, TRight>()));
        }

        public static ReaderEither<TEnvironment, TLeft, TResult> Select<TEnvironment, TLeft, TRight, TResult>(
            this ReaderEither<TEnvironment, TLeft, TRight> mt, Func<TRight, TResult> f)
        {
            return mt.FMap(f);
        }

        public static ReaderEither<TEnvironment, TLeft, TResult> SelectMany<TEnvironment, TLeft, TRight, TResult>(
            this ReaderEither<TEnvironment, TLeft, TRight> mt,
            Func<TRight, ReaderEither<TEnvironment, TLeft, TResult>> f)
        {
            return mt.Bind(f);
        }

        public static ReaderEither<TEnvironment, TLeft, TSelect> SelectMany
            <TEnvironment, TLeft, TRight, TResult, TSelect>(this ReaderEither<TEnvironment, TLeft, TRight> mt,
                Func<TRight, ReaderEither<TEnvironment, TLeft, TResult>> f, Func<TRight, TResult, TSelect> selector)
        {
            return mt.SelectMany(a => f(a).SelectMany(b => new ReaderEither<TEnvironment, TLeft, TSelect>(selector(a, b))));
        }
    }
}
