using SvenVinkemeier.Unity.UI.DataBinding;
using OrangeBugReloaded.Core;
using System.Text.RegularExpressions;

public class PointToStringConverter : ValueConverter
{
    public override object Convert(object value)
    {
        if (value is Point)
        {
            var p = (Point)value;
            return "[" + p.X + ", " + p.Y + "]";
        }

        return null;
    }

    public override object ConvertBack(object value)
    {
        var match = Regex.Match(value.ToString(), @"^\[\s*([+-]?\d+)\s*,\s*([+-]?\d+)\s*\]$");

        if (!match.Success)
            return Point.Zero;

        return new Point(
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value));
    }
}
