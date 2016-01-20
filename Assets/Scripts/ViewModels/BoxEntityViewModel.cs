using UnityEngine;
using OrangeBugReloaded.Core.Entities;

[ViewModel(typeof(BoxEntity))]
public class BoxEntityViewModel : TailoredViewModel<EntityViewModel, BoxEntity>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Object.Subscribe(() => Object.Owner, OnOwnerChanged);
    }

    private void OnOwnerChanged()
    {
        if (Object.Owner != null)
        {
            // Box has been moved
            Dispatcher.Run(() =>
            {
                Audio.pitch = Random.Range(.9f, 1.1f);
                PlaySound("BoxEntityMove", delay: false);
            });
        }
    }

    protected override void OnDispose()
    {
        Object.Unsubscribe(() => Object.Owner, OnOwnerChanged);
        base.OnDispose();
    }
}
