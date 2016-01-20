using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using OrangeBugReloaded.Core;
using System.Linq;
using UnityEngine.UI;

public class SelectionTool_New : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public static SelectionTool_New Current { get; private set; }

    // To be assigned by inspector
    public RectTransform HoveredAreaIndicator;

    private readonly Stack<SelectToken> _tokens = new Stack<SelectToken>();
    private ToolTransformHelper _helper;
    private Point _p1;
    private Point _p2;
    private bool _isIndicatorVisible;
    private Image _indicatorImage;
    private Color _indicatorImageColor;
    private Color _indicatorImageColorTransparent;

    public void OnDrag(PointerEventData e)
    {
        var token = _tokens.Peek();

        if (token == null)
            return;

        _p2 = _helper.CanvasToMapPoint(e.position);
        _p1 = (token.SelectionMode == SelectionMode.Area) ?
            _helper.CanvasToMapPoint(e.pressPosition) : _p2;
    }

    public void OnPointerDown(PointerEventData e)
    {
        var token = _tokens.Peek();

        if (token == null)
            return;

        _p1 = _p2 = _helper.CanvasToMapPoint(e.pressPosition);
        _isIndicatorVisible = true;
    }

    public void OnPointerUp(PointerEventData e)
    {
        var token = _tokens.Peek();

        if (token == null)
            return;

        _p1 = _helper.CanvasToMapPoint(e.pressPosition);
        _p2 = _helper.CanvasToMapPoint(e.position);

        if (token != null)
        {
            _isIndicatorVisible = false;
            token.SetResult(Rectangle.FromCornerPoints(_p1, _p2));
            if (!token.IsPermanent)
                _tokens.Pop();
        }
    }

    private void Start()
    {
        _helper = GetComponent<ToolTransformHelper>();
        _indicatorImage = HoveredAreaIndicator.GetComponent<Image>();
        _indicatorImageColor = _indicatorImage.color;
        _indicatorImageColorTransparent = new Color(_indicatorImageColor.r, _indicatorImageColor.g, _indicatorImageColor.b, 0);
        Current = this;
    }

    private void Update()
    {
        // Transform corner points of the selection
        // to canvas-space.
        var rect = Rectangle.FromCornerPoints(_p1, _p2);

        var c1 = _helper.MapToCanvasPoint(rect.BottomLeft);
        var c2 = _helper.MapToCanvasPoint(rect.TopRight + Point.One);

        var hoveredAreaIndicatorPosition = new Vector3(c1.x, c1.y, 0);
        var hoveredAreaIndicatorSize = new Vector2(c2.x - c1.x, c2.y - c1.y);

        // Smooth animation
        HoveredAreaIndicator.position = Vector3.Lerp(
            HoveredAreaIndicator.position,
            hoveredAreaIndicatorPosition,
            20 * Time.deltaTime);

        HoveredAreaIndicator.sizeDelta = Vector2.Lerp(
            HoveredAreaIndicator.sizeDelta,
            hoveredAreaIndicatorSize,
            20 * Time.deltaTime);

        _indicatorImage.color = Color.Lerp(
            _indicatorImage.color,
            _isIndicatorVisible ? _indicatorImageColor : _indicatorImageColorTransparent,
            8 * Time.deltaTime);
    }

    public SelectToken RequestSelect(SelectionMode mode, Action<Rectangle> callback, bool isPermanent = false)
    {
        var token = new SelectToken
        {
            SelectionMode = mode,
            Handler = callback,
            IsPermanent = isPermanent
        };
        _tokens.Push(token);

        return token;
    }

    private void OnEnable()
    {
        HoveredAreaIndicator.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        HoveredAreaIndicator.gameObject.SetActive(false);
        _tokens.Clear();
    }
}

public enum SelectionMode
{
    Point, Area
}

public class SelectToken
{
    public SelectionMode SelectionMode { get; set; }

    public Action<Rectangle> Handler { get; set; }

    /// <summary>
    /// Indicates whether this <see cref="SelectToken"/> is valid
    /// for only one selection (false) or for an arbitrary number of
    /// selections until the token is cancelled (true).
    /// </summary>
    public bool IsPermanent { get; set; }

    public bool IsCancellationRequested { get; private set; }

    public Rectangle Result { get; private set; }

    public void Cancel()
    {
        IsCancellationRequested = true;
    }

    public void SetResult(Rectangle rect)
    {
        Result = rect;
        if (Handler != null)
            Handler(rect);
    }
}