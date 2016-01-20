// TODO: Remove file

//using UnityEngine;
//using OrangeBugReloaded.Core;
//using SvenVinkemeier.Unity.UI;
//using System;

//public class SelectionTool : BindableMonoBehaviour
//{
//    public static SelectionTool Current { get; private set; }

//    private Rectangle? _selectedArea;

//    public Transform HoverCursor;
//    public Transform SelectCursor;

//    public event Action<Rectangle?> SelectedAreaChanged;

//    public Rectangle? SelectedArea
//    {
//        get { return _selectedArea; }
//        set
//        {
//            if (Set(ref _selectedArea, value, "SelectedArea"))
//            {
//                if (value.HasValue)
//                {
//                    var r = value.Value;
//                    SelectCursor.localScale = new Vector3(r.Width + 1, r.Height + 1, 1);
//                    SelectCursor.position = new Vector3(r.Left - .5f, r.Bottom - .5f, 0);
//                }
//                else
//                {
//                    // Make the select cursor invisible
//                    SelectCursor.localScale = Vector3.zero;
//                }

//                if (SelectedAreaChanged != null)
//                    SelectedAreaChanged(value);
//            }
//        }
//    }

//    private void Start()
//    {
//        if (Current != null)
//            Debug.LogError("There is more than one SelectionTool in the scene", this);

//        Current = this;

//        MouseHelper.Current.SetDefaultBehavior(SelectionMode.Area,
//            OnSelectedAreaChanged,
//            OnHoveredAreaChanged);
//    }
    
//    private void OnSelectedAreaChanged(Rectangle rect)
//    {
//        SelectedArea = rect;
//    }

//    private void OnHoveredAreaChanged(Rectangle? rect)
//    {
//        if (rect.HasValue)
//        {
//            var r = rect.Value;
//            HoverCursor.localScale = new Vector3(r.Width + 1, r.Height + 1, 1);
//            HoverCursor.position = new Vector3(r.Left - .5f, r.Bottom - .5f, 0);
//        }
//        else
//        {
//            // Make the hover cursor invisible
//            HoverCursor.localScale = Vector3.zero;
//        }
//    }
//}
