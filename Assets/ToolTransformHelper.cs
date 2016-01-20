using OrangeBugReloaded.Core;
using UnityEngine;

/// <summary>
/// Provides helper functions for tools to transform points between
/// canvas-space, world-space and map-space.
/// </summary>
public class ToolTransformHelper : MonoBehaviour
{
    private static readonly Vector3 _half = new Vector3(.5f, .5f, 0);

    private Camera _camera;
    private Canvas _canvas;
    private RectTransform _canvasRectTransform;

    private void Start()
    {
        _camera = Camera.main;
        _canvas = GetComponentInParent<Canvas>();
        _canvasRectTransform = _canvas.GetComponent<RectTransform>();
    }

    public Point CanvasToMapPoint(Vector2 canvasPoint)
    {
        var worldPoint = CanvasToWorldPoint(canvasPoint) + _half;
        return new Point(Mathf.FloorToInt(worldPoint.x), Mathf.FloorToInt(worldPoint.y));
    }

    public Vector2 MapToCanvasPoint(Point mapPoint)
    {
        var worldPoint = new Vector3(mapPoint.X, mapPoint.Y, 0) - _half;
        return WorldToCanvasPoint(worldPoint);
    }

    public Vector3 CanvasToWorldPoint(Vector2 canvasPoint)
    {
        return _camera.ViewportToWorldPoint(new Vector3(
            canvasPoint.x / _canvasRectTransform.rect.width / _canvas.scaleFactor,
            canvasPoint.y / _canvasRectTransform.rect.height / _canvas.scaleFactor,
            0));
    }

    public Vector2 WorldToCanvasPoint(Vector3 worldPoint)
    {
        var v = _camera.WorldToViewportPoint(worldPoint);
        return new Vector2(
            v.x * _canvasRectTransform.rect.width * _canvas.scaleFactor,
            v.y * _canvasRectTransform.rect.height * _canvas.scaleFactor);
    }
}
