using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Stores a table of dependencies between tiles on a map.
    /// <seealso cref="MapDependencyAttribute"/>
    /// </summary>
    public class MapDependencyTable
    {
        private static readonly HashSet<Point> _emptySet = new HashSet<Point>();

        // _dependentOn[X] = { Points that depend on X }
        private Dictionary<Point, HashSet<Point>> _dependentOn = new Dictionary<Point, HashSet<Point>>();

        // _dependencies[X] = { Points that X depends on }
        private Dictionary<Point, HashSet<Point>> _dependencies = new Dictionary<Point, HashSet<Point>>();

        /// <summary>
        /// Adds a dependency.
        /// </summary>
        /// <param name="dependentPosition">The position of the tile that depends on the tile at <paramref name="target"/></param>
        /// <param name="target">The position of the tile that the tile at <paramref name="dependentPosition"/> depends on</param>
        public void Add(Point dependentPosition, Point target)
        {
            HashSet<Point> dependenciesList;
            HashSet<Point> targetList;

            if (Contains(target, dependentPosition))
                throw new InvalidOperationException($"Adding the dependeny '{dependentPosition} depends on {target}' would create a cyclic dependency");

            if (!_dependencies.TryGetValue(dependentPosition, out dependenciesList))
                dependenciesList = _dependencies[dependentPosition] = new HashSet<Point>();

            if (!_dependentOn.TryGetValue(target, out targetList))
                targetList = _dependentOn[target] = new HashSet<Point>();

            dependenciesList.Add(target);
            targetList.Add(dependentPosition);
        }

        /// <summary>
        /// Removes a dependency.
        /// </summary>
        /// <param name="dependentPosition">The position of the tile that depends on the tile at <paramref name="target"/></param>
        /// <param name="target">The position of the tile that the tile at <paramref name="dependentPosition"/> depends on</param>
        public void Remove(Point dependentPosition, Point target)
        {
            HashSet<Point> dependenciesList;
            HashSet<Point> targetList;

            if (_dependencies.TryGetValue(dependentPosition, out dependenciesList))
            {
                dependenciesList.Remove(target);

                if (!dependenciesList.Any())
                    _dependencies.Remove(dependentPosition);
            }

            if (_dependentOn.TryGetValue(target, out targetList))
            {
                targetList.Remove(dependentPosition);

                if (!targetList.Any())
                    _dependentOn.Remove(target);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="MapDependencyTable"/> contains
        /// the specified dependency.
        /// </summary>
        /// <param name="dependentPosition">The position of the tile that depends on the tile at <paramref name="target"/></param>
        /// <param name="target">The position of the tile that the tile at <paramref name="dependentPosition"/> depends on</param>
        public bool Contains(Point dependentPosition, Point target)
        {
            HashSet<Point> list;

            if (_dependentOn.TryGetValue(target, out list))
            {
                return list.Contains(dependentPosition);
            }

            return false;
        }

        /// <summary>
        /// Removes all dependencies from other positions to the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        public void RemoveDependenciesOn(Point position)
        {
            _dependentOn.Remove(position);

            foreach (var dependenciesList in _dependencies)
            {
                dependenciesList.Value.Remove(position);
            }
        }

        /// <summary>
        /// Removes all dependencies from the specified tile to any other position.
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="position">Position of the tile</param>
        public void RemoveDependenciesOf(Tile tile, Point position)
        {
            var dependencies = MapDependencyAttribute.GetDependencies(tile, position);

            foreach (var dependency in dependencies)
                Remove(position, dependency);
        }

        /// <summary>
        /// Calculates the positions on which the specified tile depends and
        /// adds the tile's position to the dependency lists of those positions.
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="position">Position of the tile</param>
        public void AddDependenciesOf(Tile tile, Point position)
        {
            var dependencies = MapDependencyAttribute.GetDependencies(tile, position);

            foreach (var dependency in dependencies)
                Add(position, dependency);
        }

        /// <summary>
        /// Gets a collection of points that depend on the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>A point collection</returns>
        public IEnumerable<Point> GetDependenciesOn(Point position)
            => _dependentOn.TryGetValue(position, _emptySet);

        /// <summary>
        /// Gets a collection of points on which the specified point depends.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>A point collection</returns>
        public IEnumerable<Point> GetDependenciesOf(Point position)
            => _dependencies.TryGetValue(position, _emptySet);

        /// <summary>
        /// Executes an async action on a set of points with respect to dependencies.
        /// That means that points and their dependencies (and further transitive dependencies)
        /// are visited in the correct order.
        /// </summary>
        /// <param name="initialBag">
        /// The points where <paramref name="action"/> must be executed on.
        /// </param>
        /// <param name="action">
        /// An action that is executed for each visited point. Depending on the changes
        /// made by the action, this action might also be executed multiple times for the
        /// same point.
        /// Return true to continue visiting points depending on the current point.
        /// Return false to prevent visiting points depending on the current point.
        /// Return null to terminate the whole algorithm without visiting any other nodes.
        /// </param>
        public async Task DoAsyncWorkFollowingDependenciesAsync(IEnumerable<Point> initialBag, Func<Point, Task<bool?>> action)
        {
            var bag = new HashSet<Point>(initialBag);
            var todoPoints = bag.Where(p => _dependencies.TryGetValue(p, _emptySet).All(q => !bag.Contains(q)));
            var counter = 0;

            while (bag.Any())
            {
                // Note: This does not fully protect us from cycles, there could still be
                // cases where P and Q go alternately in and out of the bag
                if (!todoPoints.Any() || counter++ > 1000000)
                    throw new InvalidOperationException("Dependency cycle detected");

                var current = todoPoints.First();

                var hasChanged = await action(current);

                if (!hasChanged.HasValue)
                    return; // Null has been returned -> terminate algorithm

                // Remove visited point from bag
                bag.Remove(current);

                if (hasChanged.Value)
                {
                    // Add all points that depend on the visited point
                    foreach (var dependentPoint in _dependentOn.TryGetValue(current, _emptySet))
                        bag.Add(dependentPoint);
                }
            }
        }
    }

    /// <summary>
    /// An attribute that expresses a dependency on another tile of the map.
    /// Only <see cref="Point"/> properties may be annotated with this attribute.
    /// "X depends on Y" means that whenever the tile at position Y changes
    /// X must be notified so that it can update the tile at position X.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MapDependencyAttribute : Attribute
    {
        /// <summary>
        /// If true, the value of the annotated <see cref="Point"/> property
        /// is considered an offset to the position of the tile rather
        /// than an absolute position.
        /// </summary>
        public bool IsRelative { get; set; }

        /// <summary>
        /// Scans the specified object for <see cref="Point"/> properties
        /// annotated with a <see cref="MapDependencyAttribute"/> and
        /// returns a collection of the values of these properties.
        /// </summary>
        /// <param name="o">The object that is scanned for dependencies</param>
        /// <param name="position">
        /// The position that is used to compute the absolute position of
        /// relative dependencies
        /// </param>
        /// <returns>A collection of points the object depends on</returns>
        public static Point[] GetDependencies(object o, Point position)
        {
            return o.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(prop => prop.PropertyType == typeof(Point) && prop.CanRead)
                .Select(prop => new { Property = prop, Attribute = prop.GetCustomAttribute<MapDependencyAttribute>() })
                .Where(x => x.Attribute != null)
                .Select(x => (Point)x.Property.GetValue(o) + (x.Attribute.IsRelative ? position : Point.Zero))
                .Distinct()
                .ToArray();
        }
    }
}
