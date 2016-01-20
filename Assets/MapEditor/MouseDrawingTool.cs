using UnityEngine;
using OrangeBugReloaded.Core;
using UnityEngine.EventSystems;

public class MouseDrawingTool : SelectionRectComponent
{
    private static readonly Plane _plane = new Plane(Vector3.back, 0);
    private static readonly Vector3 _half = new Vector3(.5f, .5f, 0);

    private Point _position;

    public SelectionRectComponent Selection;

    public override Point MinCorner { get { return _position; } }
    public override Point Size { get { return Point.Zero; } }

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

            _position = point;
            Selection.Select(_position, _position);

            if (Input.GetMouseButton(0))
            {
                GetComponentInParent<MapEditor>().Designer.Fill();
            }

            OnPropertyChanged("MinCorner");
            OnPropertyChanged("Size");
        }
    }
}
