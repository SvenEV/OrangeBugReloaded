using System;
using SvenVinkemeier.Unity.UI.DataBinding;
using UnityEngine;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Foundation;

public class PropertyTemplateSelector : DataTemplateSelector
{
    public GameObject StringPropertyTemplate;
    public GameObject BoolPropertyTemplate;
    public GameObject PointPropertyTemplate;
    public GameObject RectanglePropertyTemplate;
    public GameObject EnumPropertyTemplate;

    public override GameObject SelectTemplate(object value)
    {
        var prop = (PropertyViewModel)value;

        if (prop.Type == typeof(string))
            return StringPropertyTemplate;
        else if (prop.Type == typeof(bool))
            return BoolPropertyTemplate;
        else if (prop.Type == typeof(Point))
            return PointPropertyTemplate;
        else if (prop.Type == typeof(Rectangle))
            return RectanglePropertyTemplate;
        else if (prop.Type.IsDerivedFrom<Enum>())
            return EnumPropertyTemplate;
        else
            Debug.LogWarning("PropertyTemplateSelector doesn't provide a template for type " + prop.Type);

        return null;
    }
}
