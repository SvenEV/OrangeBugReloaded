using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core;

[ViewModel(typeof(BalloonEntity))]
public class BalloonEntityViewModel : TailoredViewModel<EntityViewModel, BalloonEntity>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.Color, OnColorChanged);
        Object.Subscribe(() => Object.Owner, OnOwnerChanged);
        OnColorChanged();
    }

    private void OnOwnerChanged()
    {
        if (Object.Owner == null)
        {
            // Balloon has been destroyed
            Dispatcher.Run(() => PlaySound("BalloonEntityPop"));
        }
    }

    private void OnColorChanged()
    {
        Dispatcher.Run(() =>
        {
            Sprite = "BalloonEntity" + (Object.Color == InkColor.Red ? "" : Object.Color.ToString());
            PlaySound("BalloonEntitySplash");
        });
    }

    protected override void OnDispose()
    {
        Object.Unsubscribe(() => Object.Color, OnColorChanged);
        Object.Unsubscribe(() => Object.Owner, OnOwnerChanged);
        base.OnDispose();
    }
}
