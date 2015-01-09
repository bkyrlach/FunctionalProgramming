using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Streaming
{
    public class End : Exception
    {
        
    }

    public class Kill : Exception
    {
        
    }

    public static class Process
    {
        public static Process<T, T> Eval<T>(Task<T> t)
        {
            return new Await<T, T>(t, either => either.Match<Process<T, T>>(
                left: e => new Halt<T, T>(e),
                right: i => new Emit<T, T>(i, new Halt<T, T>(new End()))));
        }

        public static Process<T, T1> EvalOnly<T, T1>(Task<T> t)
        {
            return Eval(t).Drain<T1>();
        }

        public static Process<TI, TO> Emit<TI, TO>(TO head, Process<TI, TO> tail = null)
        {
            return new Emit<TI, TO>(head, tail ?? Halt1<TI, TO>());    
        }

        public static Process<TI, TO> Await1<TI, TO>(
            Func<TI, Process<TI, TO>> recv, 
            Func<Process<TI, TO>> fallback = null)
        {
            return new Await<TI, TO>(
                new Task<TI>(() => default(TI)),
                either => either.Match(
                    left: ex => BasicFunctions.If(ex is End,
                        () => fallback.ToMaybe().Select(f => f()).GetOrElse(Halt1<TI, TO>),
                        () => new Halt<TI, TO>(ex)),
                    right: recv));
        }

        public static Process<TI, TO> Lift<TI, TO>(Func<TI, TO> f)
        {
            return Await1<TI, TO>(i => Emit<TI, TO>(f(i))).Repeat();
        }

        public static Process<TI, TO> Halt1<TI, TO>()
        {
            return new Halt<TI, TO>(new End());
        }

        public static Process<TI, TO> Try<TI, TO>(Func<Process<TI, TO>> p)
        {
            return TryOps.Attempt(p).Match(
                success: BasicFunctions.Identity,
                failure: ex => new Halt<TI, TO>(ex));
        }

        public static Process<TI, TO> Concat<TI, TO>(this Process<TI, TO> p1, Func<Process<TI, TO>> p2)
        {
            return p1.OnHalt(ex => BasicFunctions.If(ex is End, () => Try(p2), () => new Halt<TI, TO>(ex)));
        }

        public static Process<T, T> Take<T>(int n)
        {
            return BasicFunctions.If(n <= 0, 
                Halt1<T, T>,
                () => Await1<T, T>(i => Emit(i, Take<T>(n - 1))));
        }

        public static Process<TI, TO2> Select<TI, TO1, TO2>(this Process<TI, TO1> m, Func<TO1, TO2> f)
        {
            return m.Match(
                await: (req, recv) => new Await<TI, TO2>(req, recv.AndThen(p => p.Select(f))),
                emit: (h, t) => Try(() => new Emit<TI, TO2>(f(h), t.Select(f))),
                halt: e => new Halt<TI, TO2>(e));
        }

        public static Process<TI, TO2> SelectMany<TI, TO1, TO2>(this Process<TI, TO1> m, Func<TO1, Process<TI, TO2>> f)
        {
            return m.Match(
                halt: e => new Halt<TI, TO2>(e),
                emit: (h, t) => Try(() => f(h)).Concat(() => t.SelectMany(f)),
                await: (req, recv) => new Await<TI, TO2>(req, recv.AndThen(p => p.SelectMany(f))));
        }

        public static Process<TI, TO> Resource<TI, TO>(Task<TI> acquire, Func<TI, Process<TI, TO>> use, Func<TI, Process<TI, TO>> release)
        {
            return Eval(acquire).SelectMany(r => use(r).OnComplete(() => release(r)));
        }

        public static Process<TI, TO> Resource1<TI, TO>(Task<TI> acquire, Func<TI, Process<TI, TO>> use,
            Func<TI, Task<Unit>> release)
        {
            return Resource(acquire, use, release.AndThen(t => EvalOnly<TI, TO>(t.Select(u => default(TI)))));
        }
    }

    public abstract class Process<TI, TO>
    {
        private static IEnumerable<TO> RunHelper(Process<TI, TO> cur, IEnumerable<TO> acc)
        {
            return cur.Match(
                emit: (h, t) => RunHelper(t, acc.Concat(h.LiftEnumerable())),
                halt: ex => BasicFunctions.If(ex is End, () => acc, () => { throw ex; }),
                await: (req, recv) => RunHelper(recv(req.Await().AsRight<Exception, TI>()), acc));
        }

        public IEnumerable<TO> Run()
        {
            return RunHelper(this, Enumerable.Empty<TO>());
        }

        private Process<TI, TO> RepeatHelper(Process<TI, TO> p)
        {
            return p.Match(
                halt: e => RepeatHelper(this),
                emit: (h, t) => new Emit<TI, TO>(h, RepeatHelper(t)),
                await: (req, recv) => new Await<TI, TO>(req, either => either.Match(
                    left: ex => recv(ex.AsLeft<Exception, TI>()),
                    right: i => RepeatHelper(recv(i.AsRight<Exception, TI>())))));
        }

        public Process<TI, TO> Repeat()
        {
            return RepeatHelper(this);
        }

        public Process<TI, TO2> Pipe<TO2>(Process<TO, TO2> p2)
        {
            return p2.Match(
                halt: e => Kill<TO2>().OnHalt(e2 => new Halt<TI, TO2>(e).Concat(() => new Halt<TI, TO2>(e2))),
                emit: (h, t) => new Emit<TI, TO2>(h, Pipe(t)),
                await: (req, recv) => Match(
                    halt: e => new Halt<TI, TO>(e).Pipe(recv(e.AsLeft<Exception, TO>())),
                    emit: (h, t) => t.Pipe(Process.Try(() => recv(h.AsRight<Exception, TO>()))),
                    await: (req0, recv0) => new Await<TI, TO2>(req0, recv0.AndThen(p => p.Pipe(p2)))));
        }

        public Process<TI, TO2> Kill<TO2>()
        {
            return Match(
                await: (req, recv) => recv(new Kill().AsLeft<Exception, TI>()).Drain<TO2>().OnHalt(e => BasicFunctions.If(e is Kill, () => new Halt<TI, TO2>(new End()), () => new Halt<TI, TO2>(e))),
                halt: e => new Halt<TI, TO2>(e),
                emit: (h, t) => t.Kill<TO2>());
        }

        public Process<TI, TO> OnHalt(Func<Exception, Process<TI, TO>> f)
        {
            return Match(
                halt: e => Process.Try(() => f(e)),
                emit: (h, t) => new Emit<TI, TO>(h, t.OnHalt(f)),
                await: (req, recv) => new Await<TI, TO>(req, recv.AndThen(p => p.OnHalt(f))));
        }

        public Process<TI, TO2> Drain<TO2>()
        {
            return Match(
                halt: e => new Halt<TI, TO2>(e),
                emit: (h, t) => t.Drain<TO2>(),
                await: (req, recv) => new Await<TI, TO2>(req, recv.AndThen(p => p.Drain<TO2>())));
        }

        public Process<TI, TO> AsFinalizer()
        {
            return Match<Process<TI, TO>>(
                emit: (h, t) => new Emit<TI, TO>(h, t.AsFinalizer()),
                halt: e => new Halt<TI, TO>(e),
                await: (req, recv) => new Await<TI, TO>(req, either => either.Match(
                    left: ex => BasicFunctions.If(ex is Kill, AsFinalizer, () => recv(either)),
                    right: i => recv(i.AsRight<Exception, TI>()))));
        }

        public Process<TI, TO> OnComplete(Func<Process<TI, TO>> p)
        {
            return OnHalt(ex => BasicFunctions.If(ex is End, () => p().AsFinalizer(), () => p().AsFinalizer().Concat(() => new Halt<TI, TO>(ex))));
        }

        public Process<TI, TO3> Tee<TO2, TO3>(Process<TI, TO2> p2, Process<IEither<TO, TO2>, TO3> tee)
        {
            return tee.Match(
                halt: e => Kill<TO3>().OnComplete(p2.Kill<TO3>).OnComplete(() => new Halt<TI, TO3>(e)),
                emit: (h, t) => new Emit<TI, TO3>(h, Tee(p2, t)),
                await: (side, recv) => side.Await().Match(
                    left: isO => Match(
                        halt: e => p2.Kill<TO3>().OnComplete(() => new Halt<TI, TO3>(e)),
                        emit: (o, ot) => ot.Tee(p2, Process.Try(() => recv(o.AsLeft<TO, TO2>().AsRight<Exception, IEither<TO, TO2>>()))),
                        await: (reql, recvl) => new Await<TI, TO3>(reql, recvl.AndThen(this2 => this2.Tee(p2, tee)))),
                    right: isO2 => p2.Match(
                        halt: e => Kill<TO3>().OnComplete(() => new Halt<TI, TO3>(e)),
                        emit: (o2, ot) => Tee(ot, Process.Try(() => recv(o2.AsRight<TO, TO2>().AsRight<Exception, IEither<TO, TO2>>()))),
                        await: (reqr, recvr) => new Await<TI, TO3>(reqr, recvr.AndThen(p3 => Tee(p3, tee))))));
        }

        public abstract T Match<T>(
            Func<Exception, T> halt,
            Func<Task<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit);
    }

    public sealed class Halt<TI, TO> : Process<TI, TO>
    {
        public readonly Exception Error;

        public Halt(Exception error)
        {
            Error = error;
        }

        public override T Match<T>(
            Func<Exception, T> halt,
            Func<Task<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit)
        {
            return halt(Error);
        }
    }

    public sealed class Await<TI, TO> : Process<TI, TO>
    {
        public readonly Task<TI> Request;
        public readonly Func<IEither<Exception, TI>, Process<TI, TO>> Receive;

        public Await(Task<TI> request, Func<IEither<Exception, TI>, Process<TI, TO>> receive)
        {
            Request = request;
            Receive = receive;
        }

        public override T Match<T>(
            Func<Exception, T> halt,
            Func<Task<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit)
        {
            return @await(Request, Receive);
        }
    }

    public sealed class Emit<TI, TO> : Process<TI, TO>
    {
        public readonly TO Head;
        public readonly Process<TI, TO> Tail;

        public Emit(TO head, Process<TI, TO> tail = null)
        {
            Head = head;
            Tail = tail ?? new Halt<TI, TO>(new End());
        }

        public override T Match<T>(
            Func<Exception, T> halt,
            Func<Task<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit)
        {
            return emit(Head, Tail);
        }
    }
}
