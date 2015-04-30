using System;
using System.Collections.Generic;
using System.Linq;

namespace Bede.Thallium.Belt
{
    static class CollectionExtensions
    {
        public static IEnumerable<T> Cons<T>(this T head, IEnumerable<T> seq)
        {
            return new [] { head }.Concat(seq);
        }

        public static V Lookup<K, V>(this IDictionary<K, V> @this, K key) where V : new()
        {
            return Lookup(@this, key, k => new V());
        }

        public static V Lookup<K, V>(this IDictionary<K, V> @this, K key, V item)
        {
            return Lookup(@this, key, k => item);
        }

        public static V Lookup<K, V>(this IDictionary<K, V> @this, K key, Func<K, V> item)
        {
            V i;
            if (!@this.TryGetValue(key, out i))
            {
                @this[key] = i = item(key);
            }
            return i;
        }
    }
}