// TODO: Remove file

//using OrangeBugReloaded.Core;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.EventSystems;

//public class MouseHelper : MonoBehaviour
//{
//    private static MouseHelper _current;

//    public static MouseHelper Current
//    {
//        get
//        {
//            if (_current == null)
//            {
//                var container = GameObject.Find("Services") ?? new GameObject("Services");
//                var go = new GameObject("MouseHelper");
//                go.transform.parent = container.transform;
//                _current = go.AddComponent<MouseHelper>();
//            }

//            return _current;
//        }
//    }

//    private static readonly Plane _plane = new Plane(Vector3.back, 0);
//    private static readonly Vector3 _half = new Vector3(.5f, .5f, 0);

//    private readonly Queue<SelectToken> _tokens = new Queue<SelectToken>();
//    private SelectToken _defaultBehavior;
//    private Action<Rectangle?> _hoveredAreaChangedHandler;

//    private void Start()
//    {
//        StartCoroutine(Loop());
//    }

//    public SelectToken RequestSelect(SelectionMode mode, Action<Rectangle> callback)
//    {
//        var token = new SelectToken
//        {
//            SelectionMode = mode,
//            Handler = callback
//        };
//        _tokens.Enqueue(token);

//        return token;
//    }

//    public void SetDefaultBehavior(SelectionMode mode, Action<Rectangle> selectedAreaChangedHandler, Action<Rectangle?> hoveredAreaChangedHandler)
//    {
//        _defaultBehavior = new SelectToken { SelectionMode = mode, Handler = selectedAreaChangedHandler };
//        _hoveredAreaChangedHandler = hoveredAreaChangedHandler;
//    }

//    private IEnumerator Loop()
//    {
//        while (true)
//        {
//            var token = _tokens.Any() ? _tokens.Dequeue() : _defaultBehavior;

//            if (token != null)
//            {
//                switch (token.SelectionMode)
//                {
//                    case SelectionMode.Point:
//                        yield return StartCoroutine(SelectPoint(token));
//                        break;

//                    case SelectionMode.Area:
//                        yield return StartCoroutine(SelectArea(token));
//                        break;
//                }
//            }

//            yield return null;
//        }
//    }

//    private IEnumerator SelectPoint(SelectToken token)
//    {
//        Point? lastPoint = null;

//        while (!token.IsCancellationRequested && (token != _defaultBehavior || !_tokens.Any()))
//        {
//            var point = GetPointUnderPointer();

//            if (_hoveredAreaChangedHandler != null && point != lastPoint)
//                _hoveredAreaChangedHandler(point);

//            if (Input.GetMouseButtonDown(0) && point != null)
//            {
//                token.SetResult(point.Value);
//                break;
//            }

//            lastPoint = point;
//            yield return null;
//        }
//    }

//    private IEnumerator SelectArea(SelectToken token)
//    {
//        var isDragging = false;
//        var p1 = Point.Zero;
//        var p2 = Point.Zero;

//        while (!token.IsCancellationRequested && (token != _defaultBehavior || !_tokens.Any()))
//        {
//            var point = GetPointUnderPointer();
//            var oldP1 = p1;
//            var oldP2 = p2;

//            if (point != null)
//            {
//                p2 = point.Value;

//                if (Input.GetMouseButtonUp(0) && isDragging)
//                {
//                    token.SetResult(Rectangle.FromCornerPoints(p1, p2));
//                    break;
//                }

//                if (Input.GetMouseButtonDown(0))
//                    isDragging = true;

//                if (!isDragging)
//                    p1 = p2;

//                if (_hoveredAreaChangedHandler != null && (p1 != oldP1 || p2 != oldP2))
//                    _hoveredAreaChangedHandler(Rectangle.FromCornerPoints(p1, p2));
//            }

//            yield return null;
//        }
//    }

//    private Point? GetPointUnderPointer()
//    {
//        if (EventSystem.current.IsPointerOverGameObject())
//            return null;

//        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//        float distance;

//        if (_plane.Raycast(ray, out distance))
//        {
//            var p = ray.GetPoint(distance) + _half;
//            var point = new Point(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
//            return point;
//        }

//        return null;
//    }
//}

