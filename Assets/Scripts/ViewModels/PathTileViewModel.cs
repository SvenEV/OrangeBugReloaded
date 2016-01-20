using OrangeBugReloaded.Core.Tiles;

[ViewModel(typeof(PathTile))]
public class PathTileViewModel : TailoredViewModel<LocationViewModel, PathTile>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.CausesInfection, OnIsBlockerChanged);
        OnIsBlockerChanged();
    }

    protected override void OnDispose()
    {
        Object.Unsubscribe(() => Object.CausesInfection, OnIsBlockerChanged);
        base.OnDispose();
    }

    private void OnIsBlockerChanged()
    {
        Sprite = Object.CausesInfection ? "PathBlockerizedTile" : "PathTile";
    }
}
