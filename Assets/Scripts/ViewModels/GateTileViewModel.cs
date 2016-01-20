using OrangeBugReloaded.Core.Tiles;

[ViewModel(typeof(GateTile))]
public class GateTileViewModel : TailoredViewModel<LocationViewModel, GateTile>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.IsOpen, OnIsOpenChanged);
        OnIsOpenChanged();
    }

    protected override void OnDispose()
    {
        Object.Subscribe(() => Object.IsOpen, OnIsOpenChanged);
        base.OnDispose();
    }

    private void OnIsOpenChanged()
    {
        Dispatcher.Run(() => Sprite = Object.IsOpen ? "GateTileOpen" : "GateTile");
    }
}
