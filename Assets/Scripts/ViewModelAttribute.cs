using System;

/// <summary>
/// Indicates that the attributed class is intended to be used
/// as a ViewModel for the specified type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ViewModelAttribute : Attribute
{
    public Type TargetType { get; set; }

    public ViewModelAttribute(Type type)
    {
        TargetType = type;
    }
}
