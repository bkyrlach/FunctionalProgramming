using System;
using System.Collections.Generic;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Basics
{
    public static class DictionaryExtensions    
    {
        public static IDictionary<TKey, TValue> Put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            Tuple<TKey, TValue> update)
        {
            var newDictionary = new Dictionary<TKey, TValue>(dictionary);
            newDictionary[update.Item1] = update.Item2;
            return newDictionary;
        }

        public static IMaybe<TValue> Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return
                dictionary.ToMaybe().SelectMany(d => key.ToMaybe().Where(d.ContainsKey).SelectMany(k => d[k].ToMaybe()));
        } 
    }
}
