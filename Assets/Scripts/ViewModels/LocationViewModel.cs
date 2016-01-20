using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Tiles;
using OrangeBugReloaded.Core.Foundation;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using OrangeBugReloaded.Core.Entities;

public class LocationViewModel : OrangeBugGameObjectViewModel
{
    // To be assigned by inspector
    public EntityViewModel EntityPrefab;

    private MapViewModel _mapVM;
    protected ILocation _location;

    public Tile Tile { get { return _location.Tile; } }

    public EntityViewModel EntityVM { get; private set; }

    public virtual void Initialize(ILocation location, MapViewModel mapVM)
    {
        _location = location;
        _mapVM = mapVM;

        transform.position = new Vector3(location.Position.X, location.Position.Y, 0);

        _location.Subscribe(() => _location.Tile, OnTileChanged);
        OnTileChanged(this, new BindablePropertyChangedEventArgs("Tile", null, _location.Tile));

        if (_location is MapLocation)
        {
            var ml = (MapLocation)_location;
            var n = ml.Map.GetIfLoaded(ml.Position + Point.North);
            var s = ml.Map.GetIfLoaded(ml.Position + Point.South);
            var e = ml.Map.GetIfLoaded(ml.Position + Point.East);
            var w = ml.Map.GetIfLoaded(ml.Position + Point.West);

            transform.Find("NorthBound").gameObject.SetActive(n != null && n.RegionName != ml.RegionName);
            transform.Find("SouthBound").gameObject.SetActive(s != null && s.RegionName != ml.RegionName);
            transform.Find("EastBound").gameObject.SetActive(e != null && e.RegionName != ml.RegionName);
            transform.Find("WestBound").gameObject.SetActive(w != null && w.RegionName != ml.RegionName);
        }
    }

    protected override void OnDispose()
    {
        _location.Unsubscribe(() => _location.Tile, OnTileChanged);

        if (EntityVM != null)
            EntityVM.Dispose();

        base.OnDispose();
    }

    private void OnTileChanged(object sender, BindablePropertyChangedEventArgs e)
    {
        var oldTile = e.OldValue as Tile;
        var newTile = e.NewValue as Tile;

        if (oldTile != null)
        {
            oldTile.EntityAttaching -= OnEntityAttaching;
            oldTile.EntityDetaching -= OnEntityDetaching;

            RemoveTailoredViewModel();

            if (oldTile.Entity != null)
                RemoveEntityVisuals();
        }

        if (newTile != null)
        {
            newTile.EntityAttaching += OnEntityAttaching;
            newTile.EntityDetaching += OnEntityDetaching;

            AddTailoredViewModel(newTile);

            // We can't be sure to get the initial EntityAttaching
            // event due to async dispatcher stuff, so check manually too
            if (newTile.Entity != null)
                CreateEntityVisuals(newTile.Entity);

            Sprite = newTile.GetType().Name;
        }
    }

    private Task OnEntityDetaching(Tile sender, EntityMoveContext e)
    {
        switch (e.CurrentMove.Reason)
        {
            case EntityMoveInfo.MoveReason.Move:
                // Nothing to do
                break;

            case EntityMoveInfo.MoveReason.DestructionMove:
                // This is (usually) the final move of the entity
                // after which it gets destroyed, so fade out the
                // visuals
                var oldEntityVM = EntityVM;
                EntityVM = null;
                Dispatcher.Run(() => StartCoroutine(FadeOutAndDestroy(oldEntityVM)));
                break;
        }

        return OrangeBugGameObject.Done;
    }

    private Task OnEntityAttaching(Tile sender, EntityMoveContext e)
    {
        switch (e.CurrentMove.Reason)
        {
            case EntityMoveInfo.MoveReason.Move:
                // Steal EntityVM from source location
                var sourceOwner = _mapVM.Locations[e.CurrentMove.Source.Position];
                EntityVM = sourceOwner.EntityVM;
                sourceOwner.EntityVM = null;
                break;

            case EntityMoveInfo.MoveReason.InitialPlacement:
                // This is the first time the entity is placed on
                // the map => create visuals for it (if not yet done on initialization)
                if (EntityVM == null)
                    CreateEntityVisuals(e.CurrentMove.Entity);
                break;
        }

        return OrangeBugGameObject.Done;
    }

    private void CreateEntityVisuals(Entity entity)
    {
        EntityVM = Instantiate(EntityPrefab);
        EntityVM.transform.parent = transform.parent;
        EntityVM.Initialize(entity);
    }

    private void RemoveEntityVisuals()
    {
        EntityVM.Dispose();
        Destroy(EntityVM.gameObject);
        EntityVM = null;
    }

    private static IEnumerator FadeOutAndDestroy(EntityViewModel obj)
    {
        yield return new WaitForSeconds(.2f);

        var start = Time.time;
        var duration = 1f;

        while (Time.time < start + duration)
        {
            var t = Mathf.InverseLerp(start, start + duration, Time.time);
            obj.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        obj.Dispose();
        Destroy(obj.gameObject);
    }
}