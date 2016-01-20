using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OrangeBugReloaded.Core.Foundation
{
    /// <summary>
    /// A special implementation of <see cref="INotifyPropertyChanged"/> that
    /// allows <see cref="INotifyPropertyChanged.PropertyChanged"/> event subscriptions
    /// for specific property names.
    /// </summary>
    public interface IBindable : INotifyPropertyChanged
    {
        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="propertySelector">Property selector</param>
        /// <param name="handler">Event handler</param>
        void Subscribe<T>(Expression<Func<T>> propertySelector, Action handler);

        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="propertySelector">Property selector</param>
        /// <param name="handler">Event handler</param>
        void Subscribe<T>(Expression<Func<T>> propertySelector, BindablePropertyChangedEventHandler handler);

        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="handler">Event handler</param>
        void Subscribe(string propertyName, Action handler);

        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="handler">Event handler</param>
        void Subscribe(string propertyName, BindablePropertyChangedEventHandler handler);

        /// <summary>
        /// Unsubscribes from the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="propertySelector">Property selector</param>
        /// <param name="handler">Event handler</param>
        void Unsubscribe<T>(Expression<Func<T>> propertySelector, Action handler);

        /// <summary>
        /// Unsubscribes from the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="propertySelector">Property selector</param>
        /// <param name="handler">Event handler</param>
        void Unsubscribe<T>(Expression<Func<T>> propertySelector, BindablePropertyChangedEventHandler handler);

        /// <summary>
        /// Unsubscribes from the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="handler">Event handler</param>
        void Unsubscribe(string propertyName, Action handler);

        /// <summary>
        /// Unsubscribes from the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// with the specified property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="handler">Event handler</param>
        void Unsubscribe(string propertyName, BindablePropertyChangedEventHandler handler);
    }

    /// <summary>
    /// PropertyChanged event handler.
    /// </summary>
    /// <param name="sender">The object that raised the <see cref="INotifyPropertyChanged.PropertyChanged"/> event</param>
    /// <param name="e">Event arguments</param>
    public delegate void BindablePropertyChangedEventHandler(object sender, BindablePropertyChangedEventArgs e);

    /// <summary>
    /// Extends <see cref="PropertyChangedEventArgs"/> by
    /// providing the values before and after a property change.
    /// </summary>
    public class BindablePropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// The property value before the change.
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// The property value after the change.
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindablePropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        public BindablePropertyChangedEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
