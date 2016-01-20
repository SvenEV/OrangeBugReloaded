using System;

namespace OrangeBugReloaded.Core.Designing
{
    /// <summary>
    /// Indicates that a property can be edited with an Orange Bug Map Editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EditableAttribute : Attribute
    {
        /// <summary>
        /// The display name for the property in the map editor.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Initializes a new <see cref="EditableAttribute"/>.
        /// </summary>
        public EditableAttribute()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="EditableAttribute"/>.
        /// </summary>
        public EditableAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
