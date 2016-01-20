using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Designing;
using UnityEngine;
using OrangeBugReloaded.Core.Foundation;

/// <summary>
/// A ViewModel for <see cref="TileBlockLocation"/>s intended to be used
/// to visualize tiles, entities and the selection state in the context of designing.
/// See also <see cref="MapEditor"/>.
/// </summary>
public class DesignerLocationViewModel : LocationViewModel
{
    // To be assigned by inspector
    public GameObject SelectionIndicator;

    public override void Initialize(ILocation location, MapViewModel mapVM)
    {
        base.Initialize(location, mapVM);
        name = string.Format("TileBlockLocation " + location.Position);

        var tileBlockLocation = (TileBlockLocation)_location;
        tileBlockLocation.Subscribe(() => tileBlockLocation.IsSelected, OnIsSelectedChanged);
        OnIsSelectedChanged(this, new BindablePropertyChangedEventArgs("IsSelected", false, tileBlockLocation.IsSelected));
    }

    protected override void OnDispose()
    {
        var tileBlockLocation = (TileBlockLocation)_location;
        tileBlockLocation.Unsubscribe(() => tileBlockLocation.IsSelected, OnIsSelectedChanged);
        base.OnDispose();
    }

    private void OnIsSelectedChanged(object sender, BindablePropertyChangedEventArgs e)
    {
        var isSelected = (bool)e.NewValue;
        SelectionIndicator.SetActive(isSelected);
        SpriteRenderer.sortingOrder = isSelected ? 103 : 101;
    }
}
