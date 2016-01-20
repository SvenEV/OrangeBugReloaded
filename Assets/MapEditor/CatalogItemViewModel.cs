using OrangeBugReloaded.Core.Designing;
using UnityEngine;

public class CatalogItemViewModel
{
    public GameObjectDescription GameObjectDescription { get; private set; }

    public string Name { get { return GameObjectDescription.Metadata.Name; } }

    public Sprite Sprite { get { return SpriteDictionary.Instance[GameObjectDescription.Type.Name]; } }

    public CatalogItemViewModel(GameObjectDescription info)
    {
        GameObjectDescription = info;
    }

    public override bool Equals(object obj)
    {
        return obj is CatalogItemViewModel && ((CatalogItemViewModel)obj).GameObjectDescription == GameObjectDescription;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}