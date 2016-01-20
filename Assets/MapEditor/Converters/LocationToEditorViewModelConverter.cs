using OrangeBugReloaded.Core;
using SvenVinkemeier.Unity.UI.DataBinding;

public class LocationToEditorViewModelConverter : ValueConverter
{
    public override object Convert(object value)
    {
        var location = (ILocation)value;
        return (location == null) ? null : new EditorViewModel(location);
    }

    public override object ConvertBack(object value)
    {
        return ((EditorViewModel)value).Location;
    }
}
