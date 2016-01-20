using UnityEngine;
using UnityEngine.EventSystems;
using System;
using OrangeBugReloaded.Core;

public class DrawTool : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private ToolTransformHelper _helper;
    private Point _p;

    public event Action<Point> PointDrawn;

    public void OnDrag(PointerEventData e)
    {
        var newPoint = _helper.CanvasToMapPoint(e.position);

        if (newPoint != _p)
        {
            _p = newPoint;
            if (PointDrawn != null)
                PointDrawn(_p);
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        _p = _helper.CanvasToMapPoint(e.position);
        if (PointDrawn != null)
            PointDrawn(_p);
    }

    private void Start()
    {
        _helper = GetComponent<ToolTransformHelper>();
    }
}
