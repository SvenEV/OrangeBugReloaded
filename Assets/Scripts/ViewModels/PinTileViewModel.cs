using OrangeBugReloaded.Core.Tiles;
using OrangeBugReloaded.Core;

[ViewModel(typeof(PinTile))]
public class PinTileViewModel : TailoredViewModel<LocationViewModel, PinTile>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.Color, OnColorChanged);
        OnColorChanged();
    }

    protected override void OnDispose()
    {
        Object.Subscribe(() => Object.Color, OnColorChanged);
        base.OnDispose();
    }

    private void OnColorChanged()
    {
        Sprite = ("PinTile" + (Object.Color == InkColor.Red ? "" : Object.Color.ToString()));
    }
}
