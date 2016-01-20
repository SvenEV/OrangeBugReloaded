using UnityEngine;
using OrangeBugReloaded.Core.Entities;
using System;

public sealed class EntityViewModel : OrangeBugGameObjectViewModel
{
    private Vector3 _targetPosition;

    public Entity Entity { get; private set; }

    public void Initialize(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException("entity");

        Entity = entity;
        name = Entity.GetType().Name;
        Sprite = entity.GetType().Name;
        AddTailoredViewModel(Entity);

        Entity.Subscribe(() => Entity.Owner, OnOwnerChanged);
        OnOwnerChanged();

        transform.position = _targetPosition;
    }

    protected override void OnDispose()
    {
        Entity.Unsubscribe(() => Entity.Owner, OnOwnerChanged);
        base.OnDispose();
    }

    private void OnOwnerChanged()
    {
        if (Entity.Owner != null)
            _targetPosition = new Vector3(Entity.Owner.Location.Position.X, Entity.Owner.Location.Position.Y, 0);
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPosition, 8 * Time.deltaTime);
    }
}
