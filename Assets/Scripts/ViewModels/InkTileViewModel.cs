using UnityEngine;
using OrangeBugReloaded.Core.Tiles;
using OrangeBugReloaded.Core;

[ViewModel(typeof(InkTile))]
public class InkTileViewModel : TailoredViewModel<LocationViewModel, InkTile>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.Color, OnColorChanged);
        Object.Subscribe(() => Object.IsUsed, OnColorChanged);
        OnColorChanged();
    }

    protected override void OnDispose()
    {
        Object.Unsubscribe(() => Object.Color, OnColorChanged);
        Object.Unsubscribe(() => Object.IsUsed, OnColorChanged);
        base.OnDispose();
    }

    private void OnColorChanged()
    {
        Dispatcher.Run(() =>
        {
            Sprite = Object.IsUsed ? "PathTile" :
                ("InkTile" + (Object.Color == InkColor.Red ? "" : Object.Color.ToString()));
        });
    }
}
