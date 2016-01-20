using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OrangeBugReloaded.Core.Designing
{
    /// <summary>
    /// Provides metadata about the <seealso cref="Tile"/> and
    /// <seealso cref="Entity"/> types available in the game.
    /// </summary>
    public static class GameInfo
    {
        private static List<GameObjectDescription> _tileTypes;
        private static List<GameObjectDescription> _entityTypes;

        /// <summary>
        /// <see cref="Tile"/> types.
        /// </summary>
        public static ReadOnlyCollection<GameObjectDescription> TileTypes => _tileTypes.AsReadOnly();

        /// <summary>
        /// <see cref="Entity"/> types.
        /// </summary>
        public static ReadOnlyCollection<GameObjectDescription> EntityTypes => _entityTypes.AsReadOnly();

        static GameInfo()
        {
            _tileTypes = Describe(Reflector.Default.GetTypes<Tile>());
            _entityTypes = Describe(Reflector.Default.GetTypes<Entity>());
        }

        private static List<GameObjectDescription> Describe(IEnumerable<Type> types)
        {
            return types
                .Select(o => new GameObjectDescription
                {
                    Type = o,
                    Metadata = o.GetCustomAttribute<GameObjectMetadataAttribute>()
                })
                .Where(o => o.Metadata != null)
                .ToList();
        }
    }

    /// <summary>
    /// Provides metadata about a <see cref="Tile"/> or <see cref="Entity"/> type.
    /// </summary>
    public class GameObjectDescription
    {
        /// <summary>
        /// Type.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Indicates whether the type is an <see cref="Entity"/> type.
        /// </summary>
        public bool IsEntityType => typeof(Entity).IsAssignableFrom(Type);

        /// <summary>
        /// Indicates whether the type is a <see cref="Tile"/> type.
        /// </summary>
        public bool IsTileType => typeof(Tile).IsAssignableFrom(Type);

        /// <summary>
        /// Metadata.
        /// </summary>
        public GameObjectMetadataAttribute Metadata { get; set; }

        /// <summary>
        /// Creates an instance of the type <see cref="Type"/>.
        /// </summary>
        /// <returns>OrangeBugGameObject</returns>
        public OrangeBugGameObject CreateInstance() => (OrangeBugGameObject)Activator.CreateInstance(Type);

        /// <inheritdoc/>
        public override string ToString()
        {
            return Type.Name;
        }
    }
}
