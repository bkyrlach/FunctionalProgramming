using System;
using System.Collections.Generic;

namespace FunctionalProgramming.Helpers
{
    delegate ActionRec ActionRec();
    delegate ActionRec<T> ActionRec<T>(T t);
    delegate ActionRec<T1, T2> ActionRec<T1, T2>(T1 t1, T2 t2);

    delegate FuncRec<R> FuncRec<R>();
    delegate FuncRec<T, R> FuncRec<T, R>(T t);
    delegate FuncRec<T1, T2, R> FuncRec<T1, T2, R>(T1 t1, T2 t2);

    static class Ext
    {
        public static ActionRec Break(this ActionRec a) { return null; }
        public static ActionRec<T> Break<T>(this ActionRec<T> a) { return null; }
        public static ActionRec<T1, T2> Break<T1, T2>(this ActionRec<T1, T2> a) { return null; }

        public static Action Fix(this Func<ActionRec, Func<ActionRec>> f)
        {
            return () =>
            {
                ActionRec a = null;
                for (a = () => a; a != null; a = f(a)())
                    ;
            };
        }

        public static Action<T> Fix<T>(this Func<ActionRec<T>, Func<T, ActionRec<T>>> f)
        {
            return t =>
            {
                ActionRec<T> a = null;
                for (a = t_ => { t = t_; return a; }; a != null; a = f(a)(t))
                    ;
            };
        }

        public static Action<T1, T2> Fix<T1, T2>(this Func<ActionRec<T1, T2>, Func<T1, T2, ActionRec<T1, T2>>> f)
        {
            return (t1, t2) =>
            {
                ActionRec<T1, T2> a = null;
                for (a = (t1_, t2_) => { t1 = t1_; t2 = t2_; return a; }; a != null; a = f(a)(t1, t2))
                    ;
            };
        }

        // Would really like to store result on a property on the delegate,
        // but can't derive from Delegate manually in C#... This is "brr".
        private static Dictionary<Delegate, object> _brr = new Dictionary<Delegate, object>();

        public static FuncRec<R> Break<R>(this FuncRec<R> a, R res) { _brr[a] = res; return a; }
        public static FuncRec<T, R> Break<T, R>(this FuncRec<T, R> a, R res) { _brr[a] = res; return a; }
        public static FuncRec<T1, T2, R> Break<T1, T2, R>(this FuncRec<T1, T2, R> a, R res) { _brr[a] = res; return a; }

        public static Func<R> Fix<R>(this Func<FuncRec<R>, Func<FuncRec<R>>> f)
        {
            return () =>
            {
                object res_;
                FuncRec<R> a = null;
                for (a = () => a; !_brr.TryGetValue(a, out res_); a = f(a)())
                    ;
                var res = (R)res_;
                _brr.Remove(a);
                return res;
            };
        }

        public static Func<T, R> Fix<T, R>(this Func<FuncRec<T, R>, Func<T, FuncRec<T, R>>> f)
        {
            return t =>
            {
                object res_;
                FuncRec<T, R> a = null;
                for (a = t_ => { t = t_; return a; }; !_brr.TryGetValue(a, out res_); a = f(a)(t))
                    ;
                var res = (R)res_;
                _brr.Remove(a);
                return res;
            };
        }

        public static Func<T1, T2, R> Fix<T1, T2, R>(this Func<FuncRec<T1, T2, R>, Func<T1, T2, FuncRec<T1, T2, R>>> f)
        {
            return (t1, t2) =>
            {
                object res_;
                FuncRec<T1, T2, R> a = null;
                for (a = (t1_, t2_) => { t1 = t1_; t2 = t2_; return a; }; !_brr.TryGetValue(a, out res_); a = f(a)(t1, t2))
                    ;
                var res = (R)res_;
                _brr.Remove(a);
                return res;
            };
        }
    }
}
