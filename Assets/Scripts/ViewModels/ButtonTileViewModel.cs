using OrangeBugReloaded.Core.Tiles;
using OrangeBugReloaded.Core.Entities;

[ViewModel(typeof(ButtonTile))]
class ButtonTileViewModel : TailoredViewModel<LocationViewModel, ButtonTile>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.Sensitivity, OnSensitivityChanged);
        Object.Subscribe(() => Object.IsOn, OnIsOnChanged);
        OnSensitivityChanged();
    }

    protected override void OnDispose()
    {
        Object.Unsubscribe(() => Object.Sensitivity, OnSensitivityChanged);
        Object.Unsubscribe(() => Object.IsOn, OnIsOnChanged);
        base.OnDispose();
    }

    private void OnIsOnChanged()
    {
        Dispatcher.Run(() =>
        {
            if (Object.IsOn)
                PlaySound("ButtonTilePress");
            else
                PlaySound("ButtonTileRelease");
        });
    }

    private void OnSensitivityChanged()
    {
        switch (Object.Sensitivity)
        {
            case EntityFilterMode.EntitiesExceptPlayer:
                Sprite = "ButtonTile";
                break;

            case EntityFilterMode.Entities:
                Sprite = "SensitiveButtonTile";
                break;

            default:
                Sprite = "";
                break;
        }
    }
}
