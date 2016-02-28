using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    public static class ExtensionMethods
    {
        private static readonly Random _random = new Random();

        // This method resolves ambiguity when calling TryGetValue(...) on a Dictionary<TKey, TValue>
        public static TValue TryGetValue<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
            => (dictionary as IDictionary<TKey, TValue>).TryGetValue(key, defaultValue);

        public static TValue TryGetValue<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue TryGetValue<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
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

        public static async Task<AreaSpawnResult> SpawnAsync(this IGameplayMap map, Entity entity, IEnumerable<Point> area)
        {
            var availableSpawnPoints = area.Shuffle().ToQueue();

            while (availableSpawnPoints.Any())
            {
                var spawnPosition = availableSpawnPoints.Dequeue();
                var result = await map.SpawnAsync(entity, spawnPosition);

                if (result.IsSuccessful)
                    return new AreaSpawnResult(result.Transaction, true, result.FollowUpEvents, spawnPosition);
            }

            return null;
        }

        /// <summary>
        /// Determines all positions that are directly or indirectly adjacent to
        /// and have the same region as the tile at the specified position.
        /// </summary>
        /// <param name="map">Map</param>
        /// <param name="startPosition">Start position</param>
        /// <param name="maxRadius">The maximum radius in which points are selected</param>
        /// <returns>
        /// Points that form a cohesive map area of the same region (including the start position).
        /// </returns>
        public static async Task<IReadOnlyCollection<Point>> GetCoherentPositionsAsync(this IMap map, Point startPosition, int maxRadius = 10)
        {
            var startTile = await map.GetMetadataAsync(startPosition);
            var visited = new HashSet<Point>();
            var queue = new Queue<Point>();
            queue.Enqueue(startPosition);

            while (queue.Count != 0)
            {
                var currentPosition = queue.Dequeue();

                foreach (var dir in Point.Directions)
                {
                    var neighborPosition = currentPosition + dir;
                    var distance = Point.Distance(neighborPosition, startPosition);

                    if (distance.X <= maxRadius && distance.Y <= maxRadius)
                    {
                        var neighbor = await map.GetMetadataAsync(currentPosition + dir);

                        if (!visited.Contains(neighborPosition) && neighbor.RegionId == startTile.RegionId)
                        {
                            queue.Enqueue(neighborPosition);
                            visited.Add(neighborPosition);
                        }
                    }
                }
            }

            return visited.ToArray();
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
