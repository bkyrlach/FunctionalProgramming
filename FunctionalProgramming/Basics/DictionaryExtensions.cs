using System;
using System.Collections.Generic;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Basics
{
    /// <summary>
    /// Helper class containing extensions to Dictionary that allow function modification, and safe retrieval of values.
    /// </summary>
    public static class DictionaryExtensions    
    {
        /// <summary>
        /// Updates a dictionary in a non-destructive manner, by returning a new dictionary with the update applied, leaving
        /// the original dictionary in tact.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary</typeparam>
        /// <param name="dictionary">The dictionary to "modify"</param>
        /// <param name="update">The key/value pair to update/insert</param>
        /// <returns>A new dictionary with the contents of the old dictionary, with the key value pair from update inserted</returns>
        public static IDictionary<TKey, TValue> Put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            Tuple<TKey, TValue> update)
        {
            var newDictionary = new Dictionary<TKey, TValue>(dictionary);
            newDictionary[update.Item1] = update.Item2;
            return newDictionary;
        }

        /// <summary>
        /// Safe method for retrieving values from a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary</typeparam>
        /// <param name="dictionary">The dictionary to retrieve values from</param>
        /// <param name="key">The key to retrieve the value for</param>
        /// <returns>Nothing if the dictionary is null, the dictionary does not contain the key, or value associated with that key is null, otherwise Just the value</returns>
        public static IMaybe<TValue> Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return
                dictionary.ToMaybe().SelectMany(d => key.ToMaybe().Where(d.ContainsKey).SelectMany(k => d[k].ToMaybe()));
        } 
    }
}
