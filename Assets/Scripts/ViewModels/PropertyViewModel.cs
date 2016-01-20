using System;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Foundation;

public class PropertyViewModel : INotifyPropertyChanged
{
    private object _o;
    private PropertyInfo _property;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name { get; private set; }

    public Type Type { get { return _property.PropertyType; } }

    /// <summary>
    /// A collection of values that can be assigned to <see cref="Value"/>.
    /// </summary>
    public object[] Options { get; set; }

    public object Value
    {
        get { return _property.GetValue(_o, null); }
        set
        {
            _property.SetValue(_o, value, null);
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }
    }

    public PropertyViewModel(object o, PropertyInfo property)
    {
        _o = o;
        _property = property;

        // Try to get display name from EditableAttribute, fallback to property name
        var attr = o.GetType().GetCustomAttribute<EditableAttribute>();
        Name = (attr != null && attr.DisplayName != null) ? attr.DisplayName : _property.Name;

        if (typeof(Enum).IsAssignableFrom(Type))
            Options = Enum.GetValues(Type).Cast<object>().ToArray();
    }
}
