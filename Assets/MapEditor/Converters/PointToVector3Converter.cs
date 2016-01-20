using OrangeBugReloaded.Core;
using SvenVinkemeier.Unity.UI.DataBinding;
using UnityEngine;

public class PointToVector3Converter : ValueConverter
{
    /// <summary>
    /// The offset that is added when converting from
    /// <see cref="Point"/> to <see cref="Vector3"/> and
    /// subtracted when converting back.
    /// </summary>
    public Vector3 Offset;

    public override object Convert(object value)
    {
        if (value == null)
            return Vector3.zero;

        var p = (Point)value;
        return new Vector3(p.X, p.Y, 0) + Offset;
    }

    public override object ConvertBack(object value)
    {
        if (value == null)
            return Point.Zero;

        var v = (Vector3)value;
        v -= Offset;

        return new Point(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }
}
