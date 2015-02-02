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
        /// <summary>
        /// Helper that constructs a zipper from a cons list
        /// </summary>
        /// <param name="xs">The list you wish to create a zipper for</param>
        /// <returns>A new zipper</returns>
        public static ConsListZipper<T> ToZipper(IConsList<T> xs)
        {
            return new ConsListZipper<T>(ConsList.Nil<T>(), xs);
        }

        /// <summary>
        /// All elements that occur in the source list before the current pointer
        /// </summary>
        private readonly IConsList<T> _before;

        /// <summary>
        /// The currently pointed at element, plus all elements that follow (from the source list)
        /// </summary>
        private readonly IConsList<T> _after;

        /// <summary>
        /// Constructor that accepts the 'before' and 'after' parts of the cons list
        /// </summary>
        /// <param name="before">All the elements that occur in the source list before the pointer</param>
        /// <param name="after">The currently pointed at element, plus all the elements that follow (from the source list)</param>
        private ConsListZipper(IConsList<T> before, IConsList<T> after)
        {
            _before = before;
            _after = after;
        }

        /// <summary>
        /// Advances the zipper to the next element in the source list
        /// </summary>
        /// <returns>A new zipper with the pointer advanced</returns>
        public ConsListZipper<T> Next()
        {
            return new ConsListZipper<T>(_after.Head.Select(a => a.Cons(_before)).GetOrElse(() => _before), _after.Tail.GetOrElse(() => _after));
        }

        /// <summary>
        /// Reverts the zipper to the previous element in the source list
        /// </summary>
        /// <returns>A new zipper with the pointer reverted</returns>
        public ConsListZipper<T> Prev()
        {
            return new ConsListZipper<T>(_before.Tail.GetOrElse(() => _before), _before.Head.Select(b => b.Cons(_after)).GetOrElse(() => _after));
        }

        /// <summary>
        /// Alters the zipper to point at the first element in the source list
        /// </summary>
        /// <returns>A new zipper with the pointer set to the first element of the source list</returns>
        public ConsListZipper<T> First()
        {
            return new ConsListZipper<T>(ConsList.Nil<T>(), _before.FoldL(_after, (afters, b) => b.Cons(afters)));
        }

        /// <summary>
        /// Alters the zipper to point at the last element in the source list
        /// </summary>
        /// <returns>A new zipper with the pointer set to the first element of the source list</returns>
        public ConsListZipper<T> Last()
        {
            return new ConsListZipper<T>(_after.FoldL(_before, (befores, a) => a.Cons(befores)), ConsList.Nil<T>()).Prev();
        }

        /// <summary>
        /// Get the element from the source list that this zipper currently points to
        /// </summary>
        /// <returns>The element from the source list, or nothing if the pointer was pointed to a non-existant element</returns>
        public IMaybe<T> Get()
        {
            return _after.Head;
        }

        /// <summary>
        /// Inserts an element into the source list at the current pointer
        /// </summary>
        /// <param name="t">The element to be inserted</param>
        /// <returns>A new zipper with the pointer set at the newly inserted element</returns>
        public ConsListZipper<T> Set(T t)
        {
            return new ConsListZipper<T>(_before, _after.Tail.Select(ts => t.Cons(ts)).GetOrElse(() => t.LiftList()));
        }

        /// <summary>
        /// Dual of the 'ToZipper' function that gives you a cons list based on the current zipper state
        /// </summary>
        /// <returns>A cons list modified as per the interactions with the zipper</returns>
        public IConsList<T> ToList()
        {
            return _before.FoldL(_after, (afters, b) => b.Cons(afters));
        }
    }
}
