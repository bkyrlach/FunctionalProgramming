using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Helpers;
using FunctionalProgramming.Monad;
using FunctionalProgramming.Monad.Outlaws;

namespace FunctionalProgramming.Streaming
{
    public class End : Exception
    {
        public static End Only = new End();

        private End() { }

        public override string ToString()
        {
            return "End";
        }
    }

    public class Kill : Exception
    {
        public static Kill Only = new Kill();

        private Kill() { }

        public override string ToString()
        {
            return "Kill";
        }
    }

    public static class Process
    {
        private static Process<TI, TO> BufferHelper<TState, TI, TO>(TState buffer, Func<TState> @new, Action<TI, TState> add,
            Func<TState, bool> ready, Func<TState, TO> flush)
        {
            return new Await<TI, TO>(() => default(TI), either => either.Match(
                left: e => new Halt<TI, TO>(e),
                right: i =>
                {
                    Process<TI, TO> retval;
                    add(i, buffer);
                    if (ready(buffer))
                    {
                        retval = new Emit<TI, TO>(flush(buffer)).Concat(() => BufferHelper(@new(), @new, add, ready, flush));
                    }
                    else
                    {
                        retval = new Cont<TI, TO>(() => BufferHelper(buffer, @new, add, ready, flush));
                    }
                    return retval;
                }));            
        }

        public static Process<TI, TO> Buffer<TState, TI, TO>(Func<TState> @new, Action<TI, TState> add,
            Func<TState, bool> ready, Func<TState, TO> flush)
        {
            return BufferHelper(@new(), @new, add, ready, flush);
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
                () => default(TI),
                either => either.Match(
                    left: ex => BasicFunctions.If(ex is End,
                        () => fallback.ToMaybe().Select(f => f()).GetOrElse(Halt1<TI, TO>),
                        () => new Halt<TI, TO>(ex)),
                    right: recv));
        }

        public static Process<TI, TO> Lift1<TI, TO>(Func<TI, TO> f)
        {
            return Await1<TI, TO>(i => Emit<TI, TO>(f(i)));
        }

        public static Process<TI, TO> Lift<TI, TO>(Func<TI, TO> f)
        {
            return Lift1(f).Repeat();
        }

        public static Process<TI, TO> Apply<TI, TO>(Func<TI> req, Func<TI, TO> recv)
        {
            return new Await<TI, TO>(req, either => either.Match<Process<TI, TO>>(
                left: e => new Halt<TI, TO>(e),
                right: i => new Emit<TI, TO>(recv(i))));
        } 

        public static Process<TI, TO> Halt1<TI, TO>()
        {
            return new Halt<TI, TO>(End.Only);
        }

        public static Process<TI, TO> Try<TI, TO>(Func<Process<TI, TO>> p)
        {
            return TryOps.Attempt(p).Match(
                success: BasicFunctions.Identity,
                failure: ex => new Halt<TI, TO>(ex));
        }

        public static Process<TI, TO> Concat<TI, TO>(this Process<TI, TO> p1, Func<Process<TI, TO>> p2)
        {
            return p1.OnHalt(ex => BasicFunctions.If<Process<TI, TO>>(ex is End, () => new Cont<TI, TO>(() => Try(p2)), () => new Halt<TI, TO>(ex)));
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
                halt: e => new Halt<TI, TO2>(e),
                cont: cw => new Cont<TI, TO2>(() => cw.Select(f)),
                eval: (effect, next) => new Eval<TI, TO2>(effect, next.Select(f)));
        }

        public static Process<TI, TO2> SelectMany<TI, TO1, TO2>(this Process<TI, TO1> m, Func<TO1, Process<TI, TO2>> f)
        {
            return m.Match(
                halt: e => new Halt<TI, TO2>(e),
                emit: (h, t) => Try(() => f(h)).Concat(() => t.SelectMany(f)),
                await: (req, recv) => new Await<TI, TO2>(req, recv.AndThen(p => p.SelectMany(f))),
                cont: cw => new Cont<TI, TO2>(() => cw.SelectMany(f)),
                eval: (effect, next) => new Eval<TI, TO2>(effect, next.SelectMany(f)));
        }

        public static Process<TI, TO> Resource<TR, TI, TO>(Func<TR> create, Action<TR> initialize, Action<TR> release, Process<TI, TO> use)
        {
            TR resource = default(TR);
            return new Eval<TI, TO>(
                () => resource = create(),
                new Eval<TI, TO>(
                    () => initialize(resource),
                    use.OnHalt(ex => new Eval<TI, TO>(() => release(resource)).Concat(() => new Halt<TI, TO>(ex)))));
        }
    }

    public abstract class Process<TI, TO>
    {
        public IEnumerable<TO> RunLog()
        {
            var acc = new List<TO>();
            var cur = this;
            var isDone = false;
            while (!isDone)
            {
                cur.Match(
                    emit: (h, t) =>
                    {
                        acc.Add(h);
                        cur = t;
                        return Unit.Only;
                    },
                    halt: ex =>
                    {
                        if (ex is End)
                        {
                            isDone = true;
                        }
                        else
                        {
                            throw ex;
                        }
                        return Unit.Only;
                    },
                    await: (req, recv) =>
                    {
                        var res = new Task<TI>(req).Await();
                        cur = recv(res.AsRight<Exception, TI>());
                        return Unit.Only;
                    },
                    cont: cw =>
                    {
                        cur = cw;
                        return Unit.Only;
                    },
                    eval: (effect, next) =>
                    {
                        new Task(effect).RunSynchronously();
                        cur = next;
                        return Unit.Only;
                    });                
            }
            return acc;
        }

        public TO Run()
        {
            var result = default(TO);
            var cur = this;
            var isDone = false;
            while (!isDone)
            {
                cur.Match(
                    emit: (h, t) =>
                    {
                        result = h;
                        cur = t;
                        return Unit.Only;
                    },
                    halt: ex =>
                    {
                        if (ex is End)
                        {
                            isDone = true;
                        }
                        else
                        {
                            throw ex;
                        }
                        return Unit.Only;
                    },
                    await: (req, recv) =>
                    {
                        var res = new Task<TI>(req).Await();
                        cur = recv(res.AsRight<Exception, TI>());
                        return Unit.Only;
                    },
                    cont: cw =>
                    {
                        cur = cw;
                        return Unit.Only;
                    },
                    eval: (effect, next) =>
                    {
                        new Task(effect).RunSynchronously();
                        cur = next;
                        return Unit.Only;
                    });
            }
            return result;
        }
       
        public Process<TI, TO> Repeat()
        {
            return this.Concat(Repeat);
        }

        public Process<TI, TO2> Pipe<TO2>(Process<TO, TO2> p2)
        {
            return p2.Match(
                halt: e => Kill<TO2>().OnHalt(e2 => new Halt<TI, TO2>(e).Concat(() => new Halt<TI, TO2>(e2))),
                emit: (h, t) => new Emit<TI, TO2>(h, Pipe(t)),
                cont: cw => new Cont<TI, TO2>(() => Pipe(cw)), 
                eval: (effect, next) => new Eval<TI, TO2>(effect, Pipe<TO2>(next)),
                await: (req, recv) => Match(
                    halt: e => new Halt<TI, TO>(e).Pipe(recv(e.AsLeft<Exception, TO>())),
                    emit: (h, t) => t.Pipe(Process.Try(() => recv(h.AsRight<Exception, TO>()))),
                    cont: cw => new Cont<TI, TO2>(() => cw.Pipe(p2)), 
                    eval: (effect, next) => new Eval<TI, TO2>(effect, next.Pipe<TO2>(p2)), 
                    await: (req0, recv0) => new Await<TI, TO2>(req0, recv0.AndThen(p => p.Pipe(p2)))));
        }

        public Process<TI, TO2> Kill<TO2>()
        {
            return Match(
                await: (req, recv) => recv(Streaming.Kill.Only.AsLeft<Exception, TI>()).Drain<TO2>().OnHalt(e => BasicFunctions.If(e is Kill, Process.Halt1<TI, TO2>, () => new Halt<TI, TO2>(e))),
                halt: e => new Halt<TI, TO2>(e),
                emit: (h, t) => t.Kill<TO2>(),
                eval: (effect, next) => next.Kill<TO2>(),
                cont: cw => cw.Kill<TO2>());
        }

        public Process<TI, TO> OnHalt(Func<Exception, Process<TI, TO>> f)
        {
            return Match(
                halt: e => Process.Try(() => f(e)),
                emit: (h, t) => new Emit<TI, TO>(h, t.OnHalt(f)),
                await: (req, recv) => new Await<TI, TO>(req, recv.AndThen(p => p.OnHalt(f))),
                eval: (effect, next) => new Eval<TI, TO>(effect, next.OnHalt(f)), 
                cont: cw => new Cont<TI, TO>(() => cw.OnHalt(f)));
        }

        public Process<TI, TO2> Drain<TO2>()
        {
            return Match(
                halt: e => new Halt<TI, TO2>(e),
                emit: (h, t) => t.Drain<TO2>(),
                await: (req, recv) => new Await<TI, TO2>(req, recv.AndThen(p => p.Drain<TO2>())),
                eval: (effect, next) => next.Drain<TO2>(),
                cont: cw => new Cont<TI, TO2>(cw.Drain<TO2>));
        }

        public Process<TI, TO> AsFinalizer()
        {
            return Match<Process<TI, TO>>(
                emit: (h, t) => new Emit<TI, TO>(h, t.AsFinalizer()),
                halt: e => new Halt<TI, TO>(e),
                cont: cw => new Cont<TI, TO>(cw.AsFinalizer), 
                eval: (effect, next) => new Eval<TI, TO>(effect, next.AsFinalizer()), 
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
                cont: cw => new Cont<TI, TO3>(() => Tee(p2, cw)), 
                eval: (effect, next) => new Eval<TI, TO3>(effect, Tee(p2, next)), 
                await: (side, recv) => new Task<IEither<TO, TO2>>(side).Await().Match(
                    left: isO => Match(
                        halt: e => p2.Kill<TO3>().OnComplete(() => new Halt<TI, TO3>(e)),
                        emit: (o, ot) => ot.Tee(p2, Process.Try(() => recv(o.AsLeft<TO, TO2>().AsRight<Exception, IEither<TO, TO2>>()))),
                        cont: cw => new Cont<TI, TO3>(() => cw.Tee(p2, tee)), 
                        eval: (effect, next) => new Eval<TI, TO3>(effect, next.Tee(p2, tee)),
                        await: (reql, recvl) => new Await<TI, TO3>(reql, recvl.AndThen(this2 => this2.Tee(p2, tee)))),
                    right: isO2 => p2.Match(
                        halt: e => Kill<TO3>().OnComplete(() => new Halt<TI, TO3>(e)),
                        emit: (o2, ot) => Tee(ot, Process.Try(() => recv(o2.AsRight<TO, TO2>().AsRight<Exception, IEither<TO, TO2>>()))),
                        cont: cw => new Cont<TI, TO3>(() => Tee(cw, tee)), 
                        eval: (effect, next) => new Eval<TI, TO3>(effect, Tee(next, tee)), 
                        await: (reqr, recvr) => new Await<TI, TO3>(reqr, recvr.AndThen(p3 => Tee(p3, tee))))));
        }

        public abstract T Match<T>(
            Func<Exception, T> halt,
            Func<Func<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit,
            Func<Process<TI, TO>, T> cont,
            Func<Action, Process<TI, TO>, T> eval);
    }

    public sealed class Halt<TI, TO> : Process<TI, TO>
    {
        private readonly Exception _error;

        public Halt(Exception error)
        {
            _error = error;
        }

        public override T Match<T>(
            Func<Exception, T> halt,
            Func<Func<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit,
            Func<Process<TI, TO>, T> cont,
            Func<Action, Process<TI, TO>, T> eval)
        {
            return halt(_error);
        }

        public override string ToString()
        {
            return string.Format("Halt({0})", _error);
        }
    }

    public sealed class Await<TI, TO> : Process<TI, TO>
    {
        private readonly Func<TI> _request;
        private readonly Func<IEither<Exception, TI>, Process<TI, TO>> _receive;

        public Await(Func<TI> request, Func<IEither<Exception, TI>, Process<TI, TO>> receive)
        {
            _request = request;
            _receive = receive;
        }

        public override T Match<T>(
            Func<Exception, T> halt,
            Func<Func<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit,
            Func<Process<TI, TO>, T> cont,
            Func<Action, Process<TI, TO>, T> eval)
        {
            return @await(_request, _receive);
        }

        public override string ToString()
        {
            return string.Format("Await(req, recv)");
        }
    }

    public sealed class Emit<TI, TO> : Process<TI, TO>
    {
        private readonly TO _head;
        private readonly Process<TI, TO> _tail;

        public Emit(TO head, Process<TI, TO> tail = null)
        {
            _head = head;
            _tail = tail ?? Process.Halt1<TI, TO>();
        }

        public override T Match<T>(
            Func<Exception, T> halt,
            Func<Func<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit,
            Func<Process<TI, TO>, T> cont,
            Func<Action, Process<TI, TO>, T> eval)
        {
            return emit(_head, _tail);
        }

        public override string ToString()
        {
            return string.Format("Emit({0}, {1})", _head, _tail);
        }
    }

    public sealed class Cont<TI, TO> : Process<TI, TO>
    {
        private readonly Lazy<Process<TI, TO>> _continueWith;

        public Cont(Func<Process<TI, TO>> continueWith)
        {
            _continueWith = new Lazy<Process<TI, TO>>(continueWith);
        }

        public override T Match<T>(
            Func<Exception, T> halt, 
            Func<Func<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await, 
            Func<TO, Process<TI, TO>, T> emit,
            Func<Process<TI, TO>, T> cont,
            Func<Action, Process<TI, TO>, T> eval)
        {
            return cont(_continueWith.Value);
        }

        public override string ToString()
        {
            return string.Format("Cont({0})", _continueWith.IsValueCreated ? _continueWith.Value.ToString() : "???");
        }
    }

    public sealed class Eval<TI, TO> : Process<TI, TO>
    {
        private readonly Action _effect;
        private readonly Process<TI, TO> _next;

        public Eval(Action effect, Process<TI, TO > next = null)
        {
            _effect = effect;
            _next = next ?? Process.Halt1<TI, TO>();
        } 

        public override T Match<T>(
            Func<Exception, T> halt, 
            Func<Func<TI>, Func<IEither<Exception, TI>, Process<TI, TO>>, T> await,
            Func<TO, Process<TI, TO>, T> emit,
            Func<Process<TI, TO>, T> cont,
            Func<Action, Process<TI, TO>, T> eval)
        {
            return eval(_effect, _next);
        }
    }
}
