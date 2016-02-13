using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    public static class ExtensionMethods
    {
        private static readonly Random _random = new Random();

        public static TValue TryGetValue<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
            => collection.OrderBy(o => _random.NextDouble());

        public static Queue<T> ToQueue<T>(this IEnumerable<T> collection) => new Queue<T>(collection);

        public static Stack<T> ToStack<T>(this IEnumerable<T> collection) => new Stack<T>(collection);

        public static Point GetRandomPoint(this Rectangle rectangle)
        {
            return new Point(
                _random.Next(rectangle.Left, rectangle.Right),
                _random.Next(rectangle.Bottom, rectangle.Top));
        }

        public static async Task<Tuple<MoveResult, Point>> SpawnAsync(this IGameplayMap map, Entity entity, Rectangle area)
        {
            var availableSpawnPoints = area.Shuffle().ToQueue();

            while (availableSpawnPoints.Any())
            {
                var spawnPosition = availableSpawnPoints.Dequeue();
                var result = await map.SpawnAsync(entity, spawnPosition);

                if (result.IsSuccessful)
                    return new Tuple<MoveResult, Point>(result, spawnPosition);
            }

            return null;
        }

        // Taken from http://stackoverflow.com/a/13503860
        internal static IEnumerable<TResult> FullOuterJoin<TA, TB, TKey, TResult>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TKey> selectKeyA,
            Func<TB, TKey> selectKeyB,
            Func<TA, TB, TKey, TResult> projection,
            TA defaultA = default(TA),
            TB defaultB = default(TB),
            IEqualityComparer<TKey> cmp = null)
        {
            cmp = cmp ?? EqualityComparer<TKey>.Default;
            var alookup = a.ToLookup(selectKeyA, cmp);
            var blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TKey>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                       from xa in alookup[key].DefaultIfEmpty(defaultA)
                       from xb in blookup[key].DefaultIfEmpty(defaultB)
                       select projection(xa, xb, key);

            return join;
        }

    }
}
