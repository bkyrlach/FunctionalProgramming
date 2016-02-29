using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public static Process<Unit> Eval(Action effect)
        {
            return new Eval<Unit>(effect);
        }

        public static Process<T> Apply<T>(params T[] ts)
        {
            return ts.Reverse().Aggregate((Process<T>)new Halt<T>(End.Only), (p, t) => new Emit<T>(t, p));
        }

        public static Process<T> RepeatUntil<T>(this Process<T> p, Func<bool> predicate)
        {
            return p.Concat(() => predicate() ? new Halt<T>(End.Only) : p.RepeatUntil(predicate));
        }

        public static Process<T> While<T>(this Process<T> p, Func<bool> predicate)
        {
            return Await<T>.Create(predicate, either => either.Match(
                left: e => new Halt<T>(e),
                right: b => b ? p.Concat(() => p.While(predicate)) : new Halt<T>(End.Only)));
        }

        public static Process<T> AwaitAndEmit<T>(Func<T> effect)
        {
            return Await<T>.Create(effect, result => result.Match<Process<T>>(
                left: ex => new Halt<T>(ex),
                right: ti => new Emit<T>(ti)));
        }

        public static Process<T> Continually<T>(Func<T> effect)
        {
            return AwaitAndEmit(effect).Repeat();
        }

        public static Process1<T, Unit> Sink<T>(Action<T> effect)
        {
            return Lift<T, Unit>(i =>
            {
                effect(i);
                return Unit.Only;
            });
        }

        public static Process1<T, Unit> Sink<T>(Action effect)
        {
            return Sink<T>(i => effect());
        }

        public static Process<Unit> Delay(int milliseconds)
        {
            return Await<Unit>.Create(() =>
            {
                Thread.Sleep(milliseconds);
                return Unit.Only;
            },
                either => either.Match(
                    left: ex => new Halt<Unit>(ex),
                    right: i => new Halt<Unit>(End.Only)));
        }

        private static Func<IEither<Tuple<T1, Func<T2>>, Tuple<T2, Func<T1>>>> WhenEither<T1, T2>(Func<T1> f1, Func<T2> f2)
        {
            return () =>
            {
                IEither<Tuple<T1, Func<T2>>, Tuple<T2, Func<T1>>> result;
                var t1 = new Task<T1>(f1);
                var t2 = new Task<T2>(f2);
                t1.Start();
                t2.Start();
                var tresult = Task.WhenAny(t1, t2).SafeRun();
                if (tresult == t1)
                {
                    Func<T2> f2p = () => t2.Result;
                    result = Tuple.Create(t1.Result, f2p).AsLeft<Tuple<T1, Func<T2>>, Tuple<T2, Func<T1>>>();
                }
                else
                {
                    Func<T1> f1p = () => t1.Result;
                    result = Tuple.Create(t2.Result, f1p).AsRight<Tuple<T1, Func<T2>>, Tuple<T2, Func<T1>>>();
                }
                return result;
            };
        }

        public static Process<IEither<T1, T2>> Wye<T1, T2>(Process<T1> p1, Process<T2> p2)
        {
            var random = new Random();
            if (random.Next() % 2 == 0)
            {
                return p1.Match(
                    halt: e => new Halt<IEither<T1, T2>>(e),
                    emit: (h, t) => new Emit<IEither<T1, T2>>(h.AsLeft<T1, T2>(), Wye(t, p2)),
                    cont: cw => new Cont<IEither<T1, T2>>(cw.Select(p => Wye(p, p2))),
                    eval: (effect, next) => new Eval<IEither<T1, T2>>(effect, Wye(next, p2)),
                    await: (reql, recvl) => p2.Match<Process<IEither<T1, T2>>>(
                        halt: e => new Halt<IEither<T1, T2>>(e),
                        emit: (h, t) => new Emit<IEither<T1, T2>>(h.AsRight<T1, T2>(), Wye(Await<T1>.Create(reql, (Func<IEither<Exception, object>, Process<T1>>)recvl), t)),
                        cont: cw => new Cont<IEither<T1, T2>>(cw.Select(p => Wye(p1, p))),
                        eval: (effect, next) => new Eval<IEither<T1, T2>>(effect, Wye(p1, next)),
                        await: (reqr, recvr) => Await<IEither<T1, T2>>.Create(WhenEither(reql, reqr), x => x.Match(
                                left: e => new Halt<IEither<T1, T2>>(e),
                                right: either => either.Match(
                                    left: l => HandleBranch<T1, T2>(l, recvl, recvr, true),
                                    right: r => HandleBranch<T1, T2>(r, recvl, recvr, false))))));
            }
            else
            {
                return p2.Match(
                    halt: e => new Halt<IEither<T1, T2>>(e),
                    emit: (h, t) => new Emit<IEither<T1, T2>>(h.AsRight<T1, T2>(), Wye(p1, t)),
                    cont: cw => new Cont<IEither<T1, T2>>(cw.Select(p => Wye(p1, p))),
                    eval: (effect, next) => new Eval<IEither<T1, T2>>(effect, Wye(p1, next)),
                    await: (reqr, recvr) => p1.Match<Process<IEither<T1, T2>>>(
                        halt: e => new Halt<IEither<T1, T2>>(e),
                        emit: (h, t) => new Emit<IEither<T1, T2>>(h.AsLeft<T1, T2>(), Wye(t, p2)),
                        cont: cw => new Cont<IEither<T1, T2>>(cw.Select(p => Wye(p, p2))),
                        eval: (effect, next) => new Eval<IEither<T1, T2>>(effect, Wye<T1, T2>(next, p2)),
                        await: (reql, recvl) => Await<IEither<T1, T2>>.Create(WhenEither(reql, reqr), x => x.Match(
                            left: e => new Halt<IEither<T1, T2>>(e),
                            right: either => either.Match(
                                left: l => HandleBranch<T1, T2>(l, recvl, recvr, true),
                                right: r => HandleBranch<T1, T2>(r, recvl, recvr, false))))));
            }
        }

        private static Process<IEither<T1, T2>> HandleBranch<T1, T2>(object l, Func<IEither<Exception, object>, Process<T1>> recvl, Func<IEither<Exception, object>, Process<T2>> recvr, bool isLeft)
        {
            Process<IEither<T1, T2>> retval;
            var pair = (Tuple<object, Func<object>>)l;
            if (isLeft)
            {
                retval = Wye(recvl(pair.Item1.AsRight<Exception, object>()), Await<T2>.Create(pair.Item2, recvr));
            }
            else
            {
                retval = Wye(Await<T1>.Create(pair.Item2, recvl), recvr(pair.Item1.AsRight<Exception, object>()));
            }
            return retval;
        }

        private static Process<T> BufferHelper<TState, T>(TState buffer, Func<TState> @new, Action<T, TState> add,
            Func<TState, bool> ready, Func<TState, T> flush)
        {
            return Await<T>.Create(() => default(T), either => either.Match(
                left: e => new Halt<T>(e),
                right: i =>
                {
                    add(i, buffer);
                    return ready(buffer) ? new Emit<T>(flush(buffer)).Concat(() => BufferHelper(@new(), @new, add, ready, flush)) : new Cont<T>(() => BufferHelper(buffer, @new, add, ready, flush));
                }));
        }

        public static Process<T> Buffer<TState, T>(Func<TState> @new, Action<T, TState> add,
            Func<TState, bool> ready, Func<TState, T> flush)
        {
            return BufferHelper(@new(), @new, add, ready, flush);
        }

        public static Process<T> Emit<T>(T head, Process<T> tail = null)
        {
            return new Emit<T>(head, tail ?? new Halt<T>(End.Only));
        }

        public static Process1<TI, TO> Await1<TI, TO>(
            Func<TI, Process1<TI, TO>> recv,
            Process1<TI, TO> fallback = null)
        {
            return new Await1<TI, TO>(
                () => default(TI),
                either => either.Match(
                    left: ex => ex is End
                        ? fallback.ToMaybe().GetOrElse(() => Halt1<TI, TO>())
                        : new Halt1<TI, TO>(ex),
                    right: i => Try(() => recv(i))));
        }

        public static Process1<TI, TO> Lift1<TI, TO>(Func<TI, TO> f)
        {
            return Await1<TI, TO>(i => Emit1<TI, TO>(f(i)));
        }

        public static Process1<TI, TO> Emit1<TI, TO>(TO h, Process1<TI, TO> t = null)
        {
            return new Emit1<TI, TO>(h, t ?? new Halt1<TI, TO>(End.Only));
        }

        public static Process1<T1, T2> Lift<T1, T2>(Func<T1, T2> f)
        {
            return Lift1(f).Repeat();
        }

        public static Process<T> Halt<T>(Exception ex = null)
        {
            return new Halt<T>(ex ?? End.Only);
        }

        public static Process1<TI, TO> Halt1<TI, TO>(Exception ex = null)
        {
            return new Halt1<TI, TO>(ex ?? End.Only);
        }

        public static Process<T> Try<T>(Func<Process<T>> p)
        {
            return Monad.Outlaws.Try.Attempt(p).Match(
                success: BasicFunctions.Identity,
                failure: ex => new Halt<T>(ex));
        }

        public static Process1<TI, TO> Try<TI, TO>(Func<Process1<TI, TO>> p)
        {
            return Monad.Outlaws.Try.Attempt(p).Match(
                success: BasicFunctions.Identity,
                failure: ex => new Halt1<TI, TO>(ex));
        }

        public static Process<T> Repeat<T>(this Process<T> p1)
        {
            return p1.Concat(p1.Repeat);
        }

        public static Process<T> Concat<T>(this Process<T> p1, Func<Process<T>> p2)
        {
            return p1.OnHalt(ex => ex is End ? (Process<T>)new Cont<T>(() => Try(p2)) : new Halt<T>(ex));
        }

        public static Process1<TI, TO> Repeat<TI, TO>(this Process1<TI, TO> p1)
        {
            return p1.Concat(p1.Repeat);
        }

        public static Process1<TI, TO> Concat<TI, TO>(this Process1<TI, TO> p1, Func<Process1<TI, TO>> p2)
        {
            return p1.OnHalt(ex => ex is End ? (Process1<TI, TO>)new Cont1<TI, TO>(() => Try(p2)) : new Halt1<TI, TO>(ex));
        }

        public static Process<T2> Select<T1, T2>(this Process<T1> m, Func<T1, T2> f)
        {
            return m.Pipe(Lift(f));
        }

        public static Process<T2> SelectMany<T1, T2>(this Process<T1> m, Func<T1, Process<T2>> f)
        {
            return m.Match(
                halt: e => new Halt<T2>(e),
                emit: (h, t) => Try(() => f(h)).Concat(() => t.SelectMany(f)),
                await: (req, recv) => Await<T2>.Create(req, ((Func<IEither<Exception, object>, Process<T1>>)recv).AndThen(p => p.SelectMany(f))),
                cont: cw => new Cont<T2>(cw.Select(p => p.SelectMany(f))),
                eval: (effect, next) => new Eval<T2>(effect, next.SelectMany(f)));
        }

        public static Process<T> Resource<TR, T>(Func<TR> create, Action<TR> initialize, Action<TR> release, Func<TR, Process<T>> use)
        {
            TR resource = default(TR);
            return new Eval<T>(
                () => { resource = create(); },
                new Eval<T>(
                    () => initialize(resource),
                    new Cont<T>(() => use(resource).OnHalt(ex => new Eval<T>(() => release(resource)).Concat(() => new Halt<T>(ex))))));
        }

        public static Process<T> Resource<TR, T>(Func<TR> create, Action<TR> release, Func<TR, Process<T>> use)
        {
            TR resource = default(TR);
            return new Eval<T>(
                () => { resource = create(); },
                new Cont<T>(() => use(resource).OnHalt(ex => new Eval<T>(() => release(resource)).Concat(() => new Halt<T>(ex)))));
        }

        public static Process1<T, List<T>> Buffer<T>(int chunkSize)
        {
            var chunk = new List<T>();
            var count = 0;
            return new Await1<T, List<T>>(() => default(T), either => either.Match<Process1<T, List<T>>>(
                left: ex => new Halt1<T, List<T>>(ex),
                right: next =>
                {
                    chunk.Add(next);
                    count++;
                    if (count >= chunkSize)
                    {
                        return new Emit1<T, List<T>>(chunk, new Halt1<T, List<T>>(ChunkReady.Only));
                    }
                    else
                    {
                        return new Halt1<T, List<T>>(End.Only);
                    }
                })).Repeat().OnHalt(ex => ex is ChunkReady ? Buffer<T>(chunkSize) : new Emit1<T, List<T>>(chunk, new Halt1<T, List<T>>(ex)));
        }

        public static Process<T> Where<T>(this Process<T> m, Func<T, bool> predicate)
        {
            return m.Pipe(Filter(predicate));
        }

        public static Process1<T, T> Filter<T>(Func<T, bool> predicate)
        {
            return new Await1<T, T>(() => default(T), either => either.Match(
                left: ex => new Halt1<T, T>(ex),
                right: t => predicate(t) ? (Process1<T, T>)new Emit1<T, T>(t, new Halt1<T, T>(End.Only)) : new Halt1<T, T>(End.Only))).Repeat();
        }
    }

    internal class ChunkReady : Exception
    {
        public static ChunkReady Only = new ChunkReady();

        private ChunkReady() { }
    }

    public abstract class Process<T>
    {
        public TResult Match<TResult>(
            Func<Exception, TResult> halt,
            Func<Func<object>, Func<IEither<Exception, Object>, Process<T>>, TResult> await,
            Func<T, Process<T>, TResult> emit,
            Func<Func<Process<T>>, TResult> cont,
            Func<Action, Process<T>, TResult> eval)
        {
            TResult retval;
            if (this is Halt<T>)
            {
                var temp = this as Halt<T>;
                retval = halt(temp.Error);
            }
            else if (this is Await<T>)
            {
                var temp = this as Await<T>;
                retval = @await(temp.Request, temp.Receive);
            }
            else if (this is Emit<T>)
            {
                var temp = this as Emit<T>;
                retval = emit(temp.Head, temp.Tail);
            }
            else if (this is Cont<T>)
            {
                var temp = this as Cont<T>;
                retval = cont(temp.ContinueWith);
            }
            else if (this is Eval<T>)
            {
                var temp = this as Eval<T>;
                retval = eval(temp.Effect, temp.Next);
            }
            else
            {
                throw new MatchException(typeof(Process<T>), GetType());
            }
            return retval;
        }

        public List<T> RunLog()
        {
            var results = new List<T>();
            bool isDone = false;
            var step = this;
            while (!isDone)
            {
                step.Match(
                    halt: e =>
                    {
                        if (e is End || e is Kill)
                        {
                            isDone = true;
                        }
                        else
                        {
                            throw e;
                        }
                        return Unit.Only;
                    },
                    await: (req, recv) =>
                    {
                        var o = req();
                        step = recv(o.AsRight<Exception, object>());
                        return Unit.Only;
                    },
                    emit: (h, t) =>
                    {
                        results.Add(h);
                        step = t;
                        return Unit.Only;
                    },
                    cont: cw =>
                    {
                        step = cw();
                        return Unit.Only;
                    },
                    eval: (effect, next) =>
                    {
                        effect();
                        step = next;
                        return Unit.Only;
                    });
            }
            return results;
        }

        public T Run()
        {
            return RunLog().Last();
        }

        public Process<T2> Pipe<T2>(Process1<T, T2> p2)
        {
            return p2.Match1(
                halt: e => Kill<T2>().OnHalt(e2 => new Halt<T2>(e).Concat(() => new Halt<T2>(e2))),
                emit: (h, t) => new Emit<T2>(h, Pipe(t)),
                cont: cw => new Cont<T2>(cw.Select(Pipe)),
                eval: (effect, next) => new Eval<T2>(effect, Pipe(next)),
                await: (req, recv) => Match(
                    halt: e => new Halt<T>(e).Pipe(recv(Streaming.Kill.Only.AsLeft<Exception, T>())),
                    emit: (h, t) => t.Pipe(Process.Try(() => recv(h.AsRight<Exception, T>()))),
                    cont: cw => new Cont<T2>(cw.Select(p => p.Pipe(p2))),
                    eval: (effect, next) => new Eval<T2>(effect, next.Pipe(p2)),
                    await: (req0, recv0) => Await<T2>.Create(req0, recv0.AndThen(p => p.Pipe(p2)))));
        }

        public Process<T2> Kill<T2>()
        {
            return
                Match(
                    halt: e => new Halt<T2>(e),
                    await: (req, recv) => recv(Streaming.Kill.Only.AsLeft<Exception, object>()).Drain<T2>().OnHalt(e => e is Kill ? new Halt<T2>(End.Only) : new Halt<T2>(e)),
                    emit: (h, t) => t.Kill<T2>(),
                    cont: cw => new Cont<T2>(cw.Select(p => p.Kill<T2>())),
                    eval: (effect, next) => next.Kill<T2>());
        }

        public Process<T> OnHalt(Func<Exception, Process<T>> f)
        {
            return
                Match(
                    halt: error => Process.Try(() => f(error)),
                    await: (req, recv) => Await<T>.Create(req, recv.AndThen(p => p.OnHalt(f))),
                    emit: (h, t) => new Emit<T>(h, t.OnHalt(f)),
                    cont: cw => new Cont<T>(cw.Select(p => p.OnHalt(f))),
                    eval: (effect, next) => new Eval<T>(effect, next.OnHalt(f)));
        }

        public Process<T2> Drain<T2>()
        {
            return Match(
                halt: error => new Halt<T2>(error),
                await: (req, recv) => new Cont<T2>(req.Select(o => o.AsRight<Exception, object>()).Select(recv).Select(p => p.Drain<T2>())),
                emit: (h, t) => t.Drain<T2>(),
                cont: cw => new Cont<T2>(cw.Select(p => p.Drain<T2>())),
                eval: (effect, next) => next.Drain<T2>());
        }

        public Process<T> AsFinalizer()
        {
            return Match<Process<T>>(
                halt: error => new Halt<T>(error),
                await: (req, recv) => Await<T>.Create(req, either => either.Match(
                    left: ex => ex is Kill
                        ? AsFinalizer()
                        : recv(either),
                    right: i => recv(i.AsRight<Exception, object>()))),
                emit: (h, t) => new Emit<T>(h, t.AsFinalizer()),
                cont: cw => new Cont<T>(cw.Select(p => p.AsFinalizer())),
                eval: (effect, next) => new Eval<T>(effect, next.AsFinalizer()));
        }

        public Process<T> OnComplete(Func<Process<T>> p)
        {
            return OnHalt(ex => ex is End ? p().AsFinalizer() : p().AsFinalizer().Concat(() => new Halt<T>(ex)));
        }

        public Process<T3> Tee<T2, T3>(Process<T2> p2, Process1<IEither<T, T2>, T3> tee)
        {
            return tee.Match1(
                halt: e => Kill<T3>().OnComplete(p2.Kill<T3>).OnComplete(() => new Halt<T3>(e)),
                emit: (h, t) => new Emit<T3>(h, Tee(p2, t)),
                cont: cw => new Cont<T3>(cw.Select(p => Tee(p2, p))),
                eval: (effect, next) => new Eval<T3>(effect, Tee(p2, next)),
                await: (req, recv) => Await<T3>.Create(req, either => either.Match(
                    left: ex => new Halt<T3>(ex),
                    right: side => side.Match(
                        left: t1 => Match(
                            halt: e => p2.Kill<T3>().OnComplete(() => new Halt<T3>(e)),
                            await: (reql, recvl) => Await<T3>.Create(reql, ((Func<IEither<Exception, object>, Process<T>>)recvl).AndThen(this2 => this2.Tee(p2, tee))),
                            emit: (h, t) => t.Tee(p2, Process.Try(() => recv(h.AsLeft<T, T2>().AsRight<Exception, IEither<T, T2>>()))),
                            cont: cw => new Cont<T3>(cw.Select(p => p.Tee(p2, tee))),
                            eval: (effect, next) => new Eval<T3>(effect, next.Tee(p2, tee))),
                        right: t2 => p2.Match(
                            halt: e => Kill<T3>().OnComplete(() => new Halt<T3>(e)),
                            await: (reqr, recvr) => Await<T3>.Create(reqr, ((Func<IEither<Exception, object>, Process<T2>>)recvr).AndThen(p3 => Tee(p3, tee))),
                            emit: (h, t) => Tee(t, Process.Try(() => recv(h.AsRight<T, T2>().AsRight<Exception, IEither<T, T2>>()))),
                            cont: cw => new Cont<T3>(cw.Select(p => Tee(p, tee))),
                            eval: (effect, next) => new Eval<T3>(effect, Tee(next, tee)))))));
        }
    }

    public sealed class Halt<T> : Process<T>
    {
        public readonly Exception Error;

        public Halt(Exception error)
        {
            Error = error;
        }

        public override string ToString()
        {
            return $"Halt({Error})";
        }
    }

    public sealed class Await<T> : Process<T>
    {
        public readonly Func<object> Request;
        public readonly Func<IEither<Exception, object>, Process<T>> Receive;

        public static Await<T> Create<TInput>(Func<TInput> request, Func<IEither<Exception, TInput>, Process<T>> receive)
        {
            var replacementReq = request.Select(i => (object)i);
            Func<IEither<Exception, object>, Process<T>> replacementFunc = e => receive(e.Select(o => (TInput)o));
            return new Await<T>(replacementReq, replacementFunc);
        }

        private Await(Func<object> request, Func<IEither<Exception, object>, Process<T>> receive)
        {
            Request = request;
            Receive = receive;
        }

        public override string ToString()
        {
            return "Await(req, recv)";
        }
    }

    public sealed class Emit<T> : Process<T>
    {
        public readonly T Head;
        public readonly Process<T> Tail;

        public Emit(T head, Process<T> tail = null)
        {
            Head = head;
            Tail = tail ?? new Halt<T>(End.Only);
        }

        public override string ToString()
        {
            return $"Emit({Head}, {Tail})";
        }
    }

    public sealed class Cont<T> : Process<T>
    {
        public readonly Func<Process<T>> ContinueWith;

        public Cont(Func<Process<T>> continueWith)
        {
            ContinueWith = continueWith;
        }

        public override string ToString()
        {
            //TODO Find better string representation
            return "Cont(???)";
        }
    }

    public sealed class Eval<T> : Process<T>
    {
        public readonly Action Effect;
        public readonly Process<T> Next;

        public Eval(Action effect, Process<T> next = null)
        {
            Effect = effect;
            Next = next ?? new Halt<T>(End.Only);
        }
    }

    public abstract class Process1<TI, TO> : Process<TO>
    {
        public Process1<TI, TO> OnHalt(Func<Exception, Process1<TI, TO>> f)
        {
            return Match1(
                halt: e => Process.Try(() => f(e)),
                await: (req, recv) => new Await1<TI, TO>(req, recv.AndThen(p => p.OnHalt(f))),
                emit: (h, t) => new Emit1<TI, TO>(h, t.OnHalt(f)),
                cont: cw => new Cont1<TI, TO>(cw.Select(p => p.OnHalt(f))),
                eval: (effect, next) => new Eval1<TI, TO>(effect, next.OnHalt(f)));
        }

        public TMatch Match1<TMatch>(
            Func<Exception, TMatch> halt,
            Func<Func<TI>, Func<IEither<Exception, TI>, Process1<TI, TO>>, TMatch> await,
            Func<TO, Process1<TI, TO>, TMatch> emit,
            Func<Func<Process1<TI, TO>>, TMatch> cont,
            Func<Action, Process1<TI, TO>, TMatch> eval)
        {
            TMatch retval;
            if (this is Halt1<TI, TO>)
            {
                var p = this as Halt1<TI, TO>;
                retval = halt(p.Error);
            }
            else if (this is Await1<TI, TO>)
            {
                var p = this as Await1<TI, TO>;
                retval = @await(p.Request, p.Receive);
            }
            else if (this is Emit1<TI, TO>)
            {
                var p = this as Emit1<TI, TO>;
                retval = emit(p.Head, p.Tail);
            }
            else if (this is Cont1<TI, TO>)
            {
                var p = this as Cont1<TI, TO>;
                retval = cont(p.ContinueWith);
            }
            else if (this is Eval1<TI, TO>)
            {
                var p = this as Eval1<TI, TO>;
                retval = eval(p.Effect, p.Next);
            }
            else
            {
                throw new MatchException(typeof(Process1<TI, TO>), GetType());
            }
            return retval;
        }
    }

    public sealed class Halt1<TI, TO> : Process1<TI, TO>
    {
        public readonly Exception Error;

        public Halt1(Exception error)
        {
            Error = error;
        }
    }

    public sealed class Await1<TI, TO> : Process1<TI, TO>
    {
        public readonly Func<TI> Request;
        public readonly Func<IEither<Exception, TI>, Process1<TI, TO>> Receive;

        public Await1(Func<TI> request, Func<IEither<Exception, TI>, Process1<TI, TO>> receive)
        {
            Request = request;
            Receive = receive;
        }
    }

    public sealed class Emit1<TI, TO> : Process1<TI, TO>
    {
        public readonly TO Head;
        public readonly Process1<TI, TO> Tail;

        public Emit1(TO head, Process1<TI, TO> tail)
        {
            Head = head;
            Tail = tail;
        }
    }

    public sealed class Cont1<TI, TO> : Process1<TI, TO>
    {
        public readonly Func<Process1<TI, TO>> ContinueWith;

        public Cont1(Func<Process1<TI, TO>> continueWith)
        {
            ContinueWith = continueWith;
        }
    }

    public sealed class Eval1<TI, TO> : Process1<TI, TO>
    {
        public readonly Action Effect;
        public readonly Process1<TI, TO> Next;

        public Eval1(Action effect, Process1<TI, TO> next)
        {
            Effect = effect;
            Next = next;
        }
    }
}