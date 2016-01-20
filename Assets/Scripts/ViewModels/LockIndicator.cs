using UnityEngine;
using OrangeBugReloaded.Core;

public class LockIndicator : MonoBehaviour
{
    private LocationViewModel _locationVM;
    private MapLocation _location;
    private SpriteRenderer _lockSprite;

    private void Start()
    {
        _locationVM = GetComponent<LocationViewModel>();

        if (!(_locationVM.Tile.Location is MapLocation))
            return;

        _location = (MapLocation)_locationVM.Tile.Location;

        if (_location != null)
        {
            _location.TileLock.Subscribe("IsLocked", OnTileLockChanged);
        }

        _lockSprite = GetComponent<SpriteRenderer>();

        if (_location != null)
            OnTileLockChanged();
    }

    private void OnTileLockChanged()
    {
        return;
        Dispatcher.Run(() =>
        {
            if (_location.TileLock.IsLocked)
                _lockSprite.color = Color.red;
            else
                _lockSprite.color = Color.white;
        });
    }
}
