using System.Collections;
using System.Collections.Generic;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Basics
{
    public static class DictionaryExtensions    
    {
        public static IMaybe<TValue> Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return
                dictionary.ToMaybe().SelectMany(d => key.ToMaybe().Where(d.ContainsKey).SelectMany(k => d[k].ToMaybe()));
        } 
    }
}
