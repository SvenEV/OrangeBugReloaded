using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraTool : MonoBehaviour, IDragHandler, IScrollHandler
{
    private Camera _camera;
    private Canvas _canvas;

    [Space]
    public float CameraZoom;
    public float MinimumCameraZoom = 2;
    public float MaximumCameraZoom = 40;
    public float ZoomDamping = .1f;
    public AnimationCurve ZoomAmount = AnimationCurve.Linear(0, .5f, 1, 10);

    [Space]
    public Vector3 CameraPosition;
    public float DragSpeed = .00435f;
    public float DragDamping = .1f;

    private void Start()
    {
        _camera = Camera.main;
        _canvas = GetComponentInParent<Canvas>();
        CameraPosition = _camera.transform.position;
        CameraZoom = Mathf.Clamp(_camera.orthographicSize, MinimumCameraZoom, MaximumCameraZoom);
    }

    private void Update()
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, CameraPosition, DragDamping);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, CameraZoom, ZoomDamping);
    }

    public void OnDrag(PointerEventData e)
    {
        // TODO
        //var pressWorldPoint = CanvasToWorldPoint(e.pressPosition);
        //var currentWorldPoint = CanvasToWorldPoint(e.position);
        //CameraPosition += (pressWorldPoint - currentWorldPoint);

        CameraPosition -= new Vector3(e.delta.x, e.delta.y, 0) * DragSpeed * CameraZoom;
    }

    public void OnScroll(PointerEventData e)
    {
        var normalizedZoom = Mathf.InverseLerp(MinimumCameraZoom, MaximumCameraZoom, CameraZoom); 
        CameraZoom = Mathf.Clamp(
            CameraZoom - e.scrollDelta.y * ZoomAmount.Evaluate(normalizedZoom),
            MinimumCameraZoom,
            MaximumCameraZoom);
    }
}
