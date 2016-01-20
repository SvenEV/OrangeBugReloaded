using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace OrangeBugReloaded.Core.Foundation
{
    /// <summary>
    /// A base class for <see cref="INotifyPropertyChanged"/> implementing classes.
    /// </summary>
    public abstract class BindableBase : IBindable
    {
        // For each property name we store a list of handlers
        private Dictionary<string, List<PropertyChangedToken>> _handlers =
            new Dictionary<string, List<PropertyChangedToken>>();

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        class PropertyChangedToken
        {
            public PropertyChangedEventHandler WrapperHandler { get; set; }

            public Delegate UserHandler { get; set; }
        }

        /// <summary>
        /// Sets the specified variable to the specified value
        /// and raises the <see cref="PropertyChanged"/> event
        /// for the calling property if necessary.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="storage">Variable</param>
        /// <param name="value">Value</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>True if the value changed; false if the specified value is already the current value</returns>
        protected bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                var oldValue = storage;
                storage = value;
                OnPropertyChanged(propertyName, oldValue, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        protected void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            PropertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName, oldValue, newValue));
        }

        /// <inheritdoc/>
        public void Subscribe<T>(Expression<Func<T>> propertySelector, Action handler) =>
            Subscribe(GetPropertyName(propertySelector), handler);

        /// <inheritdoc/>
        public void Subscribe<T>(Expression<Func<T>> propertySelector, BindablePropertyChangedEventHandler handler) =>
            Subscribe(GetPropertyName(propertySelector), handler);

        /// <inheritdoc/>
        public void Subscribe(string propertyName, Action handler)
        {
            var wrapperHandler = new PropertyChangedEventHandler((sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    handler();
            });
            SubscribeCore(propertyName, handler, wrapperHandler);
        }

        /// <inheritdoc/>
        public void Subscribe(string propertyName, BindablePropertyChangedEventHandler handler)
        {
            var wrapperHandler = new PropertyChangedEventHandler((sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    handler(sender, (BindablePropertyChangedEventArgs)e);
            });
            SubscribeCore(propertyName, handler, wrapperHandler);
        }

        private void SubscribeCore(string propertyName, Delegate handler, PropertyChangedEventHandler wrapperHandler)
        {
            PropertyChanged += wrapperHandler;

            List<PropertyChangedToken> list;

            if (!_handlers.TryGetValue(propertyName, out list))
                list = _handlers[propertyName] = new List<PropertyChangedToken>();

            list.Add(new PropertyChangedToken { WrapperHandler = wrapperHandler, UserHandler = handler });
        }


        /// <inheritdoc/>
        public void Unsubscribe<T>(Expression<Func<T>> propertySelector, Action handler) =>
            UnsubscribeCore(GetPropertyName(propertySelector), handler);

        /// <inheritdoc/>
        public void Unsubscribe<T>(Expression<Func<T>> propertySelector, BindablePropertyChangedEventHandler handler) =>
            UnsubscribeCore(GetPropertyName(propertySelector), handler);

        /// <inheritdoc/>
        public void Unsubscribe(string propertyName, Action handler) =>
            UnsubscribeCore(propertyName, handler);

        /// <inheritdoc/>
        public void Unsubscribe(string propertyName, BindablePropertyChangedEventHandler handler) =>
            UnsubscribeCore(propertyName, handler);

        private void UnsubscribeCore(string propertyName, Delegate handler)
        {
            List<PropertyChangedToken> list;

            if (!_handlers.TryGetValue(propertyName, out list))
                return;

            var token = list.FirstOrDefault(o => o.UserHandler == handler);

            if (token == null)
                return;

            PropertyChanged -= token.WrapperHandler;
            list.Remove(token);

            if (list.Count == 0)
                _handlers.Remove(propertyName);
        }

        internal void UnsubscribeAll()
        {
            _handlers.Clear();
            PropertyChanged = null;
        }

        /// <summary>
        /// Performs a <see cref="object.MemberwiseClone"/> and then
        /// removes all <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// handlers from the clone.
        /// </summary>
        /// <returns></returns>
        protected static T CloneWithoutHandlers<T>(T obj) where T : BindableBase
        {
            var clone = (T)obj.MemberwiseClone();
            clone._handlers = new Dictionary<string, List<PropertyChangedToken>>();
            clone.PropertyChanged = null;
            return clone;
        }

        private static string GetPropertyName<T>(Expression<Func<T>> propertySelector)
        {
            var propertyName = (propertySelector?.Body as MemberExpression)?.Member?.Name;

            if (propertyName == null)
                throw new ArgumentException("Not a valid property expression", nameof(propertySelector));

            return propertyName;
        }
    }
}