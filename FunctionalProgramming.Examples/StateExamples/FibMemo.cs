using System;
using System.Collections.Generic;
using System.Numerics;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Examples.StateExamples
{
    public static class FibMemo
    {
        public static BigInteger Fib(BigInteger n)
        {
            return FibM(n).Eval(new Dictionary<BigInteger, BigInteger>());
        }

        private static State<IDictionary<BigInteger, BigInteger>, BigInteger> FibM(BigInteger z)
        {
            return z <= 1
                ? z.Insert<IDictionary<BigInteger, BigInteger>, BigInteger>()
                : from u in StateExtensions.Get<IDictionary<BigInteger, BigInteger>, IMaybe<BigInteger>>(d => d.Get(z))
                    from v in u.Select(n => n.Insert<IDictionary<BigInteger, BigInteger>, BigInteger>()).GetOrElse(() =>
                        from r in FibM(z - 1)
                        from s in FibM(z - 2)
                        let t = r + s
                        from _ in
                            StateExtensions.Mod<IDictionary<BigInteger, BigInteger>>(d => d.Put(Tuple.Create(z, t)))
                        select t
                        )
                    select v;
        }
    }
}
