using System;
using System.Collections.Generic;
using System.Diagnostics;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public interface IWriterMaybe<TLog, TValue>
    {
        Tuple<TLog, IMaybe<TValue>> ValueWithLog();
        IMaybe<TValue> Value();
    }

    public static class WriterMaybe
    {
        public static IWriterMaybe<TLog, TValue> MaybeWithLog<TLog, TValue>(this TValue v, TLog log)
        {
            return new WriterMaybeImpl<TLog, TValue>(log, v);
        }

        public static IWriterMaybe<TLog, TValue> MaybeWithLog<TLog, TValue>(this TValue v, Func<TValue, TLog> f)
        {
            return new WriterMaybeImpl<TLog, TValue>(f(v), v);
        }

        public static IWriterMaybe<IEnumerable<TLog>, TValue> MaybeWithLogs<TLog, TValue>(this TValue v, TLog log)
        {
            return new WriterMaybeImpl<IEnumerable<TLog>, TValue>(log.LiftEnumerable(), v);
        }

        public static IWriterMaybe<IEnumerable<TLog>, TValue> MaybeWithLogs<TLog, TValue>(this TValue v, Func<TValue, TLog> f)
        {
            return new WriterMaybeImpl<IEnumerable<TLog>, TValue>(f(v).LiftEnumerable(), v);
        }

        public static IWriterMaybe<TLog, Unit> MaybeLogOnly<TLog>(this TLog log)
        {
            return new WriterMaybeImpl<TLog, Unit>(log, Unit.Only);
        }

        public static IWriterMaybe<IEnumerable<TLog>, Unit> MaybeLogsOnly<TLog>(this TLog log)
        {
            return new WriterMaybeImpl<IEnumerable<TLog>, Unit>(log.LiftEnumerable(), Unit.Only);
        }

        public static IWriterMaybe<TLog, TValue> MaybeNoLog<TLog, TValue>(this TValue v, IMonoid<TLog> m)
        {
            return new WriterMaybeImpl<TLog, TValue>(m.MZero, v);
        }

        public static IWriterMaybe<IEnumerable<TLog>, TValue> LogIfEmpty<TLog, TValue>(this IWriterMaybe<IEnumerable<TLog>, TValue> m, TLog log)
        {
            var mo = EnumerableMonoid<TLog>.Only;
            var logAndValue = m.ValueWithLog();
            return logAndValue.Item2.IsEmpty
                ? new WriterMaybeImpl<IEnumerable<TLog>, TValue>(mo.MAppend(logAndValue.Item1, log.LiftEnumerable()), logAndValue.Item2)
                : m;
        }

        public static IWriterMaybe<IEnumerable<TLog>, TValue> LogIfPresent<TLog, TValue>(
            this IWriterMaybe<IEnumerable<TLog>, TValue> m, TLog log)
        {
            var mo = EnumerableMonoid<TLog>.Only;
            var logAndValue = m.ValueWithLog();
            return logAndValue.Item2.IsEmpty
                ? m
                : new WriterMaybeImpl<IEnumerable<TLog>, TValue>(mo.MAppend(logAndValue.Item1, log.LiftEnumerable()), logAndValue.Item2);            
        }

        public static IWriterMaybe<IEnumerable<TLog>, TValue> LogIf<TLog, TValue>(
            this IWriterMaybe<IEnumerable<TLog>, TValue> m, TLog ifEmpty, TLog ifPresent)
        {
            var mo = EnumerableMonoid<TLog>.Only;
            var logAndValue = m.ValueWithLog();
            return logAndValue.Item2.IsEmpty
                ? new WriterMaybeImpl<IEnumerable<TLog>, TValue>(mo.MAppend(logAndValue.Item1, ifEmpty.LiftEnumerable()), logAndValue.Item2)
                : new WriterMaybeImpl<IEnumerable<TLog>, TValue>(mo.MAppend(logAndValue.Item1, ifPresent.LiftEnumerable()), logAndValue.Item2);                        
        }

        public static IWriterMaybe<TLog, TValue> Where<TLog, TValue>(this IWriterMaybe<TLog, TValue> m,
            Func<TValue, bool> predicate)
        {
            var logAndValue = m.ValueWithLog();
            return new WriterMaybeImpl<TLog, TValue>(logAndValue.Item1, logAndValue.Item2.Where(predicate));
        }

        public static IWriterMaybe<TLog, TResult> Select<TLog, TInitial, TResult>(this IWriterMaybe<TLog, TInitial> m,
            Func<TInitial, TResult> f)
        {
            var logAndValue = m.ValueWithLog();
            return new WriterMaybeImpl<TLog, TResult>(logAndValue.Item1, logAndValue.Item2.Select(f));
        }

        public static IWriterMaybe<IEnumerable<TLog>, TResult> SelectMany<TLog, TInitial, TResult>(
            this IWriterMaybe<IEnumerable<TLog>, TInitial> m, Func<TInitial, IWriterMaybe<IEnumerable<TLog>, TResult>> f)
        {
            var mo = EnumerableMonoid<TLog>.Only;
            var logAndValue = m.ValueWithLog();
            var maybeNewLogAndValue = logAndValue.Item2.Select(v => f(v).ValueWithLog());
            return new WriterMaybeImpl<IEnumerable<TLog>, TResult>(mo.MAppend(logAndValue.Item1, maybeNewLogAndValue.Select(newLogAndValue => newLogAndValue.Item1).GetOrElse(() => mo.MZero)), maybeNewLogAndValue.SelectMany(newLogAndValue => newLogAndValue.Item2));
        }

        public static IWriterMaybe<IEnumerable<TLog>, TSelect> SelectMany<TLog, TInitial, TResult, TSelect>(
            this IWriterMaybe<IEnumerable<TLog>, TInitial> m, Func<TInitial, IWriterMaybe<IEnumerable<TLog>, TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return from initial in m
                   from result in f(initial)
                   select selector(initial, result);
        }

        private class WriterMaybeImpl<TLog, TValue> : IWriterMaybe<TLog, TValue>
        {
            private readonly TLog _log;
            private readonly IMaybe<TValue> _maybeValue;

            public WriterMaybeImpl(TLog log, TValue value)
            {
                _log = log;
                _maybeValue = value.ToMaybe();
            }

            public WriterMaybeImpl(TLog log, IMaybe<TValue> maybeValue)
            {
                _log = log;
                _maybeValue = maybeValue;
            }

            public Tuple<TLog, IMaybe<TValue>> ValueWithLog()
            {
                return Tuple.Create(_log, _maybeValue);
            }

            public IMaybe<TValue> Value()
            {
                return _maybeValue;
            }
        }
    }

}
