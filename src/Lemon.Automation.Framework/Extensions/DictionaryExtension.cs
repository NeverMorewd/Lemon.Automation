using System.Collections.Concurrent;

namespace Lemon.Automation.Framework.Extensions
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// https://codereview.stackexchange.com/questions/2025/extension-methods-to-make-concurrentdictionary-getoradd-and-addorupdate-thread-s
        /// https://stackoverflow.com/questions/12595911/memory-cache-or-concurrent-dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public static V GetOrAdd<T, V>(this ConcurrentDictionary<T, Lazy<V>> dictionary,
            T key,
            Func<T, V> valueFactory) where T : notnull
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            Lazy<V> lazy = dictionary.GetOrAdd(key, new Lazy<V>(() => valueFactory(key)));
            return lazy.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="addValueFactory"></param>
        /// <param name="updateValueFactory"></param>
        /// <returns></returns>
        public static V AddOrUpdate<T, V>(this ConcurrentDictionary<T, Lazy<V>> dictionary,
            T key,
            Func<T, V> addValueFactory,
            Func<T, V, V> updateValueFactory) where T : notnull
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            Lazy<V> lazy = dictionary.AddOrUpdate(key,
                        new Lazy<V>(() => addValueFactory(key)),
                        (k, oldValue) => new Lazy<V>(() => updateValueFactory(k, oldValue.Value)));
            return lazy.Value;
        }


        public static V GetOrAdd<T, U, V>(this ConcurrentDictionary<T, U> dictionary,
            T key,
            Func<T, V> valueFactory) where U : Lazy<V> where T : notnull
        {
            U lazy = dictionary.GetOrAdd(key, (U)new Lazy<V>(() => valueFactory(key)));
            return lazy.Value;
        }

        public static V AddOrUpdate<T, U, V>(this ConcurrentDictionary<T, U> dictionary,
            T key,
            Func<T, V> addValueFactory,
            Func<T, V, V> updateValueFactory) where U : Lazy<V> where T : notnull
        {
            U lazy = dictionary.AddOrUpdate(key,
                        (U)new Lazy<V>(() => addValueFactory(key)),
                        (k, oldValue) => (U)new Lazy<V>(() => updateValueFactory(k, oldValue.Value)));
            return lazy.Value;
        }
    }
}
