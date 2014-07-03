using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Test.ConsoleApp
{
    abstract class Comparison
    {
        public abstract T Match<T>(Func<T> isGreater, Func<T> isLess, Func<T> isEqual);
    }

    sealed class LessThan : Comparison
    {
        public static readonly LessThan Only = new LessThan();

        private LessThan()
        {
            
        }
        public override T Match<T>(Func<T> isGreater, Func<T> isLess, Func<T> isEqual)
        {
            return isGreater();
        }
    }

    sealed class GreaterThan : Comparison
    {
        public static readonly GreaterThan Only = new GreaterThan();

        private GreaterThan()
        {
            
        }
        public override T Match<T>(Func<T> isGreater, Func<T> isLess, Func<T> isEqual)
        {
            return isLess();
        }
    }

    sealed class EqualTo : Comparison
    {
        public static readonly EqualTo Only = new EqualTo();

        private EqualTo()
        {
            
        }
        public override T Match<T>(Func<T> isGreater, Func<T> isLess, Func<T> isEqual)
        {
            return isEqual();
        }
    }

    public class SomeResult
    {        
        public string ValueIWant { get; set; }        
    }

    public class SomeBadResult
    {
        public IEnumerable<string> Errors { get; set; } 
    }

    class Program
    {
        private static void Main(string[] args)
        {
            String a = null;
            String b = "bob";

            Console.WriteLine(a.ToMaybe().Where(x => x.Length > 3).GetOrElse(() => "ben is cool"));
            Console.WriteLine(b.ToMaybe().Where(x => x.Length > 3).GetOrElse(() => "ben is cool"));

            int c = 20;
            int d = 10;

            Console.WriteLine(DivideIfEven(c).SelectMany(DivideIfEven).SelectMany(DivideIfEven));
            Console.WriteLine(DivideIfEven(d).SelectMany(DivideIfEven).SelectMany(DivideIfEven));

            var firstNames = new[] {"Ben", "Rob", null };
            var lastNames = new[] {null, "Chipman", "Schmitz"};

            firstNames.Zip(lastNames, GetFullName).Select(name => name.GetOrElse(() => "Unknown")).ToList().ForEach(Console.WriteLine);

            IEither<int, string> test1 = c.AsLeft<int, string>();
            IEither<int, string> test2 = b.AsRight<int, string>();

            test1.Select(x => x + "abc");
            test2.Select(x => x + "abc");

            test1.Match(
                left: n => n.ToString(),
                right: s => s);

            IEither<string, int> test3 = test1.Swap();
            IEither<string, double> test4 = test1.SelectEither(n => n.ToString(), double.Parse);

            var lol = new[] {new[] {1}, new[] {2}, new[] {3}};

            SumList(lol, EnumerableMonoid<int>.Only).ToList().ForEach(Console.WriteLine);
        }

        static T SumList<T>(IEnumerable<T> ts, IMonoid<T> m)
        {
            return ts.Aggregate(m.MZero, m.MAppend);
        }

        static IMaybe<int> DivideIfEven(int n)
        {
            return n%2 == 0 ? (n/2).ToMaybe() : MaybeExtensions.Nothing<int>();
        } 

        static IMaybe<string> GetFullName(string firstName, string lastName)
        {
            return from fn in firstName.ToMaybe()
                   from ln in lastName.ToMaybe()
                   select ln + ", " + fn;
        }
    }

}
