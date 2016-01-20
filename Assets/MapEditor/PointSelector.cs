using UnityEngine;
using OrangeBugReloaded.Core;
using UnityEngine.UI;
using SvenVinkemeier.Unity.UI;
using System;

/// <summary>
/// To be used in conjunction with a UI Button that triggers
/// the request to select a point or an area.
/// </summary>
public class PointSelector : BindableMonoBehaviour
{
    public Button Button;

    private Point _selectedPoint;
    private Rectangle _selectedArea;
    private SelectToken _selectToken;

    public Point SelectedPoint
    {
        get { return _selectedPoint; }
        set { Set(ref _selectedPoint, value, "SelectedPoint"); }
    }

    public Rectangle SelectedArea
    {
        get { return _selectedArea; }
        set { Set(ref _selectedArea, value, "SelectedArea"); }
    }
    
    public void BeginSelectPoint()
    {
        _selectToken = SelectionTool_New.Current.RequestSelect(SelectionMode.Point, OnPointSelected);
        Button.interactable = false;
    }

    public void BeginSelectArea()
    {
        _selectToken = SelectionTool_New.Current.RequestSelect(SelectionMode.Area, OnAreaSelected);
        Button.interactable = false;
    }

    private void OnPointSelected(Rectangle rect)
    {
        SelectedPoint = rect.BottomLeft;
        Button.interactable = true;
    }

    private void OnAreaSelected(Rectangle rect)
    {
        SelectedArea = rect;
        Button.interactable = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _selectToken != null)
        {
            _selectToken.Cancel();
            Button.enabled = true;
        }
    }
}
