using FunctionalProgramming.Monad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalProgramming.Basics
{
    public class Transaction
    {
        private IDictionary<Guid, Ref> _refs;
         
        internal Transaction()
        {
            _refs = new Dictionary<Guid, Ref>();    
        }
    }

    public abstract class Ref
    {
        protected Guid Id;
        protected int Version;
        protected object Value;
    }

    public class Ref<T> : Ref
    {        

        public Ref(T value)
        {
            Id = Guid.NewGuid();
            Value = value;
            Version = 0;
        }

        public Action<Func<T, T>> Swap(Transaction t)
        {
            return f =>
            {

            };
        }
    }


    public static class STM
    {


        public static void Atomic(Action<Transaction> change)
        {
            
        }  
    }
}
