using UnityEngine;
using System.ComponentModel;

namespace SvenVinkemeier.Unity.UI.Tests
{
    public class SampleDataContext : BindableMonoBehaviour
    {
        private readonly ColorItem[] _colors = new[]
        {
            new ColorItem { Color = Color.red, DisplayName = "Firered" },
            new ColorItem { Color = Color.green, DisplayName = "Grassgreen" },
            new ColorItem { Color = Color.blue, DisplayName = "Waterblue" },
        };

        private ColorItem _selectedColor;

        public ColorItem[] Colors { get { return _colors; } }

        public ColorItem SelectedColor
        {
            get { return _selectedColor; }
            set { Set(ref _selectedColor, value, "SelectedColor"); }
        }
    }

    public class ColorItem : INotifyPropertyChanged
    {
        private string _displayName;

        public Color Color { get; set; }

        public string DisplayName
        {
            get { return _displayName; }
            set { Set(ref _displayName, value, "DisplayName"); }
        }





        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the specified variable to the specified value
        /// and raises the <see cref="PropertyChanged"/> event
        /// for the calling property if necessary.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="storage">Variable</param>
        /// <param name="value">Value</param>
        /// <param name="propertyName">Property name, can be left empty</param>
        /// <returns></returns>
        protected bool Set<T>(ref T storage, T value, string propertyName)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null)
                ev(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}