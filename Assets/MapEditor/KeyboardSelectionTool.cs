using UnityEngine;
using OrangeBugReloaded.Core;

public class KeyboardSelectionTool : SelectionRectComponent
{
    private bool _isResizing;
    private Point _position1;
    private Point _position2;

    public Rectangle Rectangle
    {
        get { return Rectangle.FromCornerPoints(_position1, _position2); }
        set { Select(value.BottomLeft, value.TopRight); }
    }

    public override Point MinCorner { get { return Point.Min(_position1, _position2); } }

    public override Point Size { get { return Point.Distance(_position1, _position2); } }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            _isResizing = true;

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            var newPosition = Point.Min(_position1, _position2);
            _position2 = Point.Max(_position1, _position2);
            _position1 = newPosition;
            _isResizing = false;
        }

        if (_isResizing)
        {
            if (ButtonHelper.Instance.Get("Left"))
                _position2 += Point.West;
            else if (ButtonHelper.Instance.Get("Right"))
                _position2 += Point.East;
            else if (ButtonHelper.Instance.Get("Up"))
                _position2 += Point.North;
            else if (ButtonHelper.Instance.Get("Down"))
                _position2 += Point.South;
        }
        else
        {
            Point delta = Point.Zero;

            if (ButtonHelper.Instance.Get("Left"))
                delta += Point.West;
            else if (ButtonHelper.Instance.Get("Right"))
                delta += Point.East;
            else if (ButtonHelper.Instance.Get("Up"))
                delta += Point.North;
            else if (ButtonHelper.Instance.Get("Down"))
                delta += Point.South;

            _position1 += delta;
            _position2 += delta;
        }

        OnPropertyChanged("Rectangle");
        OnPropertyChanged("MinCorner");
        OnPropertyChanged("Size");
    }

    public override void Select(Point p1, Point p2)
    {
        _position1 = p1;
        _position2 = p2;
        OnPropertyChanged("Rectangle");
        OnPropertyChanged("MinCorner");
        OnPropertyChanged("Size");
    }
}
