using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

[ViewModel(typeof(PlayerEntity))]
public class PlayerEntityViewModel : TailoredViewModel<EntityViewModel, PlayerEntity>
{
    private static bool _hold = false;
    private Quaternion _targetRotation;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        Object.Subscribe(() => Object.Perspective, OnPerspectiveChanged);
        OnPerspectiveChanged();
        Camera.main.GetComponent<CameraFollow>().Targets.Add(transform);

        transform.rotation = _targetRotation;
    }

    protected override void OnDispose()
    {
        Object.Unsubscribe(() => Object.Perspective, OnPerspectiveChanged);
        Camera.main.GetComponent<CameraFollow>().Targets.Remove(transform);
        base.OnDispose();
    }

    private void OnPerspectiveChanged()
    {
        var p = Object.Perspective;

        _targetRotation = Quaternion.Euler(new Vector3(0, 0,
            (p == Point.East) ? 0 :
            (p == Point.South) ? 270 :
            (p == Point.West) ? 180 :
            (p == Point.North) ? 90 : 0));
    }

    private void Update()
    {

        // Restore region with backspace key
        if (Input.GetKeyDown(KeyCode.Backspace) && !_hold)
        {
            _hold = true;
            ((MapLocation)Object.Owner.Location).RestoreRegionAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                    Debug.LogError(t.Exception);

                _hold = false;
            }); // Intended fire and forget
        }

        if (ButtonHelper.Instance.Get("Right"))
            Object.TryMoveTowardsAsync(Point.East);
        else if (ButtonHelper.Instance.Get("Left"))
            Object.TryMoveTowardsAsync(Point.West);
        else if (ButtonHelper.Instance.Get("Up"))
            Object.TryMoveTowardsAsync(Point.North);
        else if (ButtonHelper.Instance.Get("Down"))
            Object.TryMoveTowardsAsync(Point.South);

        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, 6 * Time.deltaTime);

        if (Input.GetMouseButtonDown(0) && !GameController.Instance.IsInEditMode && !EventSystem.current.IsPointerOverGameObject())
        {
            var m = new Vector2(
                Input.mousePosition.x / Screen.width - .5f,
                Input.mousePosition.y / Screen.height - .5f);

            if (m.x > 0 && m.x > Mathf.Abs(m.y))
                Object.TryMoveTowardsAsync(Point.East);
            else if (m.x < 0 && -m.x > Mathf.Abs(m.y))
                Object.TryMoveTowardsAsync(Point.West);
            else if (m.y > 0 && m.y > Mathf.Abs(m.x))
                Object.TryMoveTowardsAsync(Point.North);
            else if (m.y < 0 && -m.y > Mathf.Abs(m.x))
                Object.TryMoveTowardsAsync(Point.South);
        }
    }
}
