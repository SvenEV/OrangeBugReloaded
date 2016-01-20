using UnityEngine;
using OrangeBugReloaded.Core;

public class KeyboardDrawingTool : SelectionRectComponent
{
    private Point _position;

    public Rectangle Rectangle
    {
        get { return new Rectangle(_position, Point.Zero); }
        set { Select(value.BottomLeft, value.TopRight); }
    }

    public override Point MinCorner { get { return _position; } }

    public override Point Size { get { return Point.Zero; } }

    private void Update()
    {
        if (ButtonHelper.Instance.Get("Left"))
            _position += Point.West;
        else if (ButtonHelper.Instance.Get("Right"))
            _position += Point.East;
        else if (ButtonHelper.Instance.Get("Up"))
            _position += Point.North;
        else if (ButtonHelper.Instance.Get("Down"))
            _position += Point.South;

        OnPropertyChanged("Rectangle");
        OnPropertyChanged("MinCorner");
        OnPropertyChanged("Size");
    }

    public override void Select(Point p1, Point p2)
    {
        _position = p1;
        OnPropertyChanged("Rectangle");
        OnPropertyChanged("MinCorner");
        OnPropertyChanged("Size");
    }
}
