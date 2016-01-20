using UnityEngine;
using System.ComponentModel;
using System;

namespace SvenVinkemeier.Unity.UI
{
    public abstract class BindableMonoBehaviour : MonoBehaviour, INotifyPropertyChanged, IDisposable
    {
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

        public void Dispose()
        {
            OnDispose();
            Destroy(this);
        }

        protected virtual void OnDispose() { }
    }
}