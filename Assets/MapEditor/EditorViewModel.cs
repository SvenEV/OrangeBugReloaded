using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Foundation;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class EditorViewModel
{
    private static readonly BindingFlags _propertyFlags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;

    public ILocation Location { get; private set; }

    public string TileName { get; private set; }
    public string TileDescription { get; private set; }
    public PropertyViewModel[] TileProperties { get; private set; }
    public string EntityName { get; private set; }
    public string EntityDescription { get; private set; }
    public PropertyViewModel[] EntityProperties { get; private set; }

    public EditorViewModel(ILocation location)
    {
        if (location == null)
            throw new ArgumentNullException("tile");

        Location = location;

        var tile = Location.Tile;

        if (tile != null)
        {
            var tileType = tile.GetType();
            var tileMeta = tileType.GetCustomAttribute<GameObjectMetadataAttribute>();
            var entityType = (tile.Entity == null) ? null : tile.Entity.GetType();
            var entityMeta = (tile.Entity == null) ? null : entityType.GetCustomAttribute<GameObjectMetadataAttribute>();

            if (tileMeta == null)
                Debug.LogError("Missing metadata on " + tile.GetType().Name);

            if (tile.Entity != null && entityMeta == null)
                Debug.LogError("Missing metadata on " + tile.Entity.GetType().Name);

            TileName = tileMeta.Name;
            TileDescription = tileMeta.Description;
            TileProperties = GetEditableProperties(tile);

            if (tile.Entity != null)
            {
                EntityName = entityMeta.Name;
                EntityDescription = entityMeta.Description;
                EntityProperties = GetEditableProperties(tile.Entity);
            }
        }
    }

    private PropertyViewModel[] GetEditableProperties(object o)
    {
        return o.GetType()
            .GetProperties(_propertyFlags)
            .Where(p => p.CanWrite && p.GetCustomAttributes(typeof(EditableAttribute), false).Any())
            .Select(p => new PropertyViewModel(o, p))
            .ToArray();
    }
}
