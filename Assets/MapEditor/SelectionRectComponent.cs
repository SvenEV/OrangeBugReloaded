using OrangeBugReloaded.Core;
using SvenVinkemeier.Unity.UI;

public abstract class SelectionRectComponent : BindableMonoBehaviour
{
    /// <summary>
    /// The bottom left corner position of the selection rectangle.
    /// </summary>
    public abstract Point MinCorner { get; }

    /// <summary>
    /// The size of the selection rectangle.
    /// </summary>
    public abstract Point Size { get; }

    /// <summary>
    /// Resizes and moves the selection rectangle to fit
    /// the specified points.
    /// </summary>
    /// <param name="p1">Point 1</param>
    /// <param name="p2">Point 2</param>
    public virtual void Select(Point p1, Point p2) { }
}
