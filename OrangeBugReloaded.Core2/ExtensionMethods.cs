using System.Collections.Generic;

namespace OrangeBugReloaded.Core
{
    public static class ExtensionMethods
    {
        public static TValue TryGetValue<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
