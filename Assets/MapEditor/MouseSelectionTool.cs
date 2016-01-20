using UnityEngine;
using OrangeBugReloaded.Core;
using UnityEngine.EventSystems;

public class MouseSelectionTool : SelectionRectComponent
{
    private static readonly Plane _plane = new Plane(Vector3.back, 0);
    private static readonly Vector3 _half = new Vector3(.5f, .5f, 0);

    private bool _isDragging;
    private Point _position1;
    private Point _position2;

    public SelectionRectComponent Selection;

    public override Point MinCorner { get { return Point.Min(_position1, _position2); } }
    public override Point Size { get { return Point.Distance(_position1, _position2); } }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (_plane.Raycast(ray, out distance))
        {
            var p = ray.GetPoint(distance) + _half;
            var point = new Point(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));

            if (Input.GetMouseButtonDown(0))
            {
                _position1 = point;
                _isDragging = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Selection.Select(_position1, _position2);
                _position1 = point;
                _isDragging = false;
            }

            if (_isDragging)
            {
                _position2 = point;
            }
            else
            {
                var delta = point - _position2;
                _position1 += delta;
                _position2 += delta;
            }

            OnPropertyChanged("MinCorner");
            OnPropertyChanged("Size");
        }
    }
}
