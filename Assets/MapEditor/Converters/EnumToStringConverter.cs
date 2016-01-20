using SvenVinkemeier.Unity.UI.DataBinding;
using System;

public class EnumToStringConverter : ValueConverter
{
    public Type Type { get; set; }

    public override object Convert(object value)
    {
        return (value == null) ? null : value.ToString();
    }

    public override object ConvertBack(object value)
    {
        return Enum.Parse(Type, value.ToString());
    }
}
