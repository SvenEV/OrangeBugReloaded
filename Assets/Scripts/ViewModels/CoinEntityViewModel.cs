using OrangeBugReloaded.Core.Entities;

[ViewModel(typeof(CoinEntity))]
public class CoinEntityViewModel : TailoredViewModel<EntityViewModel, CoinEntity>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.Owner, OnOwnerChanged);
    }

    protected override void OnDispose()
    {
        Object.Unsubscribe(() => Object.Owner, OnOwnerChanged);
        base.OnDispose();
    }

    private void OnOwnerChanged()
    {
        if (Object.Owner == null)
        {
            // Coin has been collected
            PlaySound("CoinEntityCollect");
        }
    }
}
