using UnityEngine;
using UnityEngine.UI;
using SvenVinkemeier.Unity.UI;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core;
using System;

public class MapEditor : BindableMonoBehaviour
{
    private IMap _map;
    private SelectToken _initialPointSelectToken;
    private MapDesigner _designer;
    private MapViewModel _visualizer;
    private ILocation _selectedLocation;

    // Tools
    private Tool _selectedTool;
    public SelectionTool_New SelectTool;
    public CameraTool CameraTool;
    public DrawTool DrawTool;

    public Tool SelectedTool
    {
        get { return _selectedTool; }
        set
        {
            switch (_selectedTool)
            {
                case Tool.Camera:
                    CameraTool.enabled = false;
                    break;

                case Tool.Select:
                    SelectTool.enabled = false;
                    break;

                case Tool.Draw:
                    DrawTool.PointDrawn -= OnPointDrawn;
                    DrawTool.enabled = false;
                    break;
            }

            switch (value)
            {
                case Tool.Camera:
                    CameraTool.enabled = true;
                    break;

                case Tool.Select:
                    SelectTool.enabled = true;
                    SelectTool.RequestSelect(SelectionMode.Area, OnAreaSelected, true);
                    break;

                case Tool.Draw:
                    DrawTool.enabled = true;
                    DrawTool.PointDrawn += OnPointDrawn;
                    break;
            }

            _selectedTool = value;
        }
    }

    public CanvasScaler Canvas;

    public MapDesigner Designer
    {
        get { return _designer; }
        set { Set(ref _designer, value, "Designer"); }
    }

    /// <summary>
    /// The <see cref="ILocation"/> that is selected.
    /// If the selection contains zero or more than one
    /// location this property is null.
    /// </summary>
    public ILocation SelectedLocation
    {
        get { return _selectedLocation; }
        private set { Set(ref _selectedLocation, value, "SelectedLocation"); }
    }


    public void SwitchToSelectTool()
    {
        SelectedTool = Tool.Select;
    }

    public void SwitchToCameraTool()
    {
        SelectedTool = Tool.Camera;
    }

    public void SwitchToDrawTool()
    {
        SelectedTool = Tool.Draw;
    }

    public void OpenDesigner(IMap map)
    {
        if (!Application.isEditor)
            Canvas.GetComponent<CanvasScaler>().scaleFactor = 1.5f;

        _map = map;
        _visualizer = GetComponent<MapViewModel>();

        SelectedTool = Tool.Select;
        _initialPointSelectToken = SelectTool.RequestSelect(SelectionMode.Point, OnInitialPointSelected);
    }

    public void BeginCloseDesigner()
    {
        // If construction site area has not been selected yet
        if (_initialPointSelectToken != null)
            _initialPointSelectToken.Cancel();

        // Close the map designer
        if (Designer != null)
        {
            Designer.SelectedLocations.ItemAdded += OnSelectionChanged;
            Designer.SelectedLocations.ItemRemoved += OnSelectionChanged;
            Designer.DisposeAsync().ContinueWith(_ => Dispatcher.Run(FinishCloseDesigner));
        }
        else
        {
            FinishCloseDesigner();
        }
    }


    private void FinishCloseDesigner()
    {
        if (_visualizer != null)
            _visualizer.LocationsProvider = null; // Remove tile & entity visuals

        gameObject.SetActive(false);
        GameController.Instance.IsInEditMode = false;
    }

    private void Update()
    {
        if (Designer == null)
            return;

        if (Input.GetKeyDown(KeyCode.F2))
            BeginCloseDesigner();
        else if (Input.GetKeyDown(KeyCode.Return))
            Designer.Fill();
        else if (Input.GetKeyDown(KeyCode.Delete))
            Designer.Clear();
    }

    private void OnInitialPointSelected(Rectangle area)
    {
        MapDesigner.CreateAsync(_map, area).ContinueWith(t => Dispatcher.Run(() =>
        {
            Designer = t.Result;
            SelectedTool = Tool.Camera;

            if (Designer == null)
            {
                // Failed to create designer (might be a lock or permission issue)
                BeginCloseDesigner();
            }
            else
            {
                _visualizer.LocationsProvider = Designer.TileBlock;
                Designer.SelectedLocations.ItemAdded += OnSelectionChanged;
                Designer.SelectedLocations.ItemRemoved += OnSelectionChanged;
            }
        }));
    }

    private void OnSelectionChanged(TileBlockLocation _)
    {
        if (Designer.SelectedLocations.Count == 1)
            SelectedLocation = Designer.SelectedLocations[0];
        else
            SelectedLocation = null;
    }

    private void OnAreaSelected(Rectangle rect)
    {
        // The user has selected a different area
        if (Designer != null)
            Designer.Select(rect);
    }

    private void OnPointDrawn(Point p)
    {
        if (Designer != null && Designer.SelectedCatalogItemInstance != null)
        {
            Designer.Select(Rectangle.FromCornerPoints(p, p));
            Designer.Fill();
        }
    }

    public enum Tool
    {
        Camera,
        Select,
        Draw
    }
}