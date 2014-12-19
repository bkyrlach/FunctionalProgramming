using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Basics
{
    /// <summary>
    /// A functional zipper for cons lists. Provides a way to treat an immutable linked list as if it were a traditional list, updating elements at specific
    /// indeces, removing elements at specific indeces, etc...
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the list for which this is a zipper</typeparam>
    public sealed class ConsListZipper<T>
    {
        public static ConsListZipper<T> ToZipper(IConsList<T> xs)
        {
            return new ConsListZipper<T>(ConsList.Nil<T>(), xs);
        }

        private readonly IConsList<T> _before;
        private readonly IConsList<T> _after;

        private ConsListZipper(IConsList<T> before, IConsList<T> after)
        {
            _before = before;
            _after = after;
        }

        public ConsListZipper<T> Next()
        {
            return new ConsListZipper<T>(_after.Head.Select(a => a.Cons(_before)).GetOrElse(() => _before), _after.Tail.GetOrElse(() => _after));
        }

        public ConsListZipper<T> Prev()
        {
            return new ConsListZipper<T>(_before.Tail.GetOrElse(() => _before), _before.Head.Select(b => b.Cons(_after)).GetOrElse(() => _after));
        }

        public ConsListZipper<T> First()
        {
            return new ConsListZipper<T>(ConsList.Nil<T>(), _before.FoldL(_after, (afters, b) => b.Cons(afters)));
        }

        public ConsListZipper<T> Last()
        {
            return new ConsListZipper<T>(_after.FoldL(_before, (befores, a) => a.Cons(befores)), ConsList.Nil<T>()).Prev();
        }

        public IMaybe<T> Get()
        {
            return _after.Head;
        }

        public ConsListZipper<T> Set(T t)
        {
            return new ConsListZipper<T>(_before, _after.Tail.Select(ts => t.Cons(ts)).GetOrElse(() => t.LiftList()));
        }

        public IConsList<T> ToList()
        {
            return _before.FoldL(_after, (afters, b) => b.Cons(afters));
        }
    }
}
