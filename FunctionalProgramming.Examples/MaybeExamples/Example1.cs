using FunctionalProgramming.Monad;
using System.Linq;

namespace FunctionalProgramming.Examples.MaybeExamples
{
    class A
    {
        public int Id { get; set; }
        public int BId { get; set; }
    }

    class B
    {
        public int Id { get; set; }
        public IQueryable<C> Cs { get; set; }
    }

    class C
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public static class Example1
    {
        public static void Example()
        {
            var c1 = new C {Id = 1, Name = "Test"};
            var c2 = new C {Id = 2, Name = "Another"};
            var c3 = new C {Id = 3, Name = "Yet Another"};
            var c4 = new C {Id = 4, Name = "More C"};
            var someCs = (new[] { c1, c2, c3}).AsQueryable();
            var otherCs = (new[] {c1, c2, c4}).AsQueryable();
            var b1 = new B {Id = 1, Cs = someCs};
            var b2 = new B {Id = 2, Cs = someCs};
            var b3 = new B {Id = 2, Cs = otherCs};
            var bs = (new[] {b1, b2, b3}).AsQueryable();
            var a1 = new A {Id = 1, BId = 1};
            var a2 = new A {Id = 1, BId = 2};
            var   a3 = new A {Id = 1, BId = 4};
            var  _as = (new[] {a1, a2, a3}).AsQueryable();

            string cName;
            var myA = _as.FirstOrDefault(a => a.Id == 1);
            if (myA != null)
            {
                var myB = bs.FirstOrDefault(b => b.Id == myA.BId);
                if (myB != null)
                {
                    var myC = myB.Cs.FirstOrDefault(c => c.Id == 1);
                    if (myC != null)
                    {
                        cName = myC.Name;
                    }
                }
            }

            // Not guaranteed safe to use cName here
            // Using Maybe is better, but not good enough...
            var cName2 = _as.FirstOrDefault(a => a.Id == 1).ToMaybe()
                    .SelectMany(a => bs.FirstOrDefault(b => b.Id == a.BId).ToMaybe())
                    .SelectMany(b => b.Cs.FirstOrDefault(c => c.Id == 1).ToMaybe())
                    .Select(c => c.Name);

            // We can make this beautiful using LINQ Query Syntax

            var cName3 = from a in _as.FirstOrDefault(a => a.Id == 1).ToMaybe()
                         from b in bs.FirstOrDefault(b => b.Id == a.BId).ToMaybe()
                         from c in b.Cs.FirstOrDefault(c => c.Id == 1).ToMaybe()
                         select c.Name;
        }
    }
}