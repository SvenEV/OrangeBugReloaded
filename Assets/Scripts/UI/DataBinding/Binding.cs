using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    /// <summary>
    /// Binds the value of a property or field of a data context component to
    /// a property or field of another component.
    /// </summary>
    [AddComponentMenu("MVVM/Binding")]
    public class Binding : MonoBehaviour
    {
        private bool _isActivated = false;

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (!Equals(_dataContext, value))
                {
                    Detach();
                    _dataContext = value;
                    if (_isActivated) Attach();
                }
            }
        }

        public string SourcePropertyName;
        public string TargetPropertyName;
        public object FallbackValue;
        public object TargetNullValue;
        public UnityEngine.Object Target;
        public ValueConverter Converter;
        public SourceToTargetBindingMode SourceToTargetMode = SourceToTargetBindingMode.EventBased;
        public TargetToSourceBindingMode TargetToSourceMode = TargetToSourceBindingMode.Disabled;
        [Range(.1f, 10)]
        public float Interval = 1;

        private object _value;
        private object _dataContext;
        private MemberInfo _sourceProp;
        private MemberInfo _targetProp;
        private Coroutine _updaterRoutine;

        private void Start()
        {
            if (DataContext == null)
                DataContext = BindingOperations.FindDataContext(transform);

            _isActivated = true;
            Attach();
        }

        private void Attach()
        {
            if (Target == null)
            {
                Log("Binding target error: Target is null");
                return;
            }

            if (TargetPropertyName == null)
            {
                Log("Binding target error: TargetPropertyName is empty");
                return;
            }

            _targetProp = BindingOperations.GetMember(Target, TargetPropertyName);

            if (_targetProp == null)
            {
                Log("Binding target error: Could not find field or property '{0}' on item '{1}'", TargetPropertyName, Target);
                return;
            }

            if (DataContext == null)
            {
                // Assign FallbackValue
                SetTargetValue();
                return;
            }

            _sourceProp = BindingOperations.GetMember(DataContext, SourcePropertyName);

            if (_sourceProp == null && !string.IsNullOrEmpty(SourcePropertyName))
            {
                Log("Binding source error: Could not find field or property '{0}' on item '{1}'", SourcePropertyName, DataContext);
                return;
            }

            RefreshTargetValue(forceSet: true);

            switch (SourceToTargetMode)
            {
                case SourceToTargetBindingMode.OneTime:
                    // Do nothing (value is never updated)
                    break;

                case SourceToTargetBindingMode.EventBased:
                    var notifier = DataContext as INotifyPropertyChanged;
                    if (notifier != null)
                        notifier.PropertyChanged += OnSourceValueChanged;
                    break;

                case SourceToTargetBindingMode.Periodically:
                    _updaterRoutine = StartCoroutine(RefreshRepeatedlyAsync(false));
                    break;

                case SourceToTargetBindingMode.EveryFrame:
                    _updaterRoutine = StartCoroutine(RefreshRepeatedlyAsync(true));
                    break;

                default:
                    throw new NotImplementedException();
            }

            switch (TargetToSourceMode)
            {
                case TargetToSourceBindingMode.Disabled:
                    // Do nothing, target value changes are not written back to source
                    break;

                case TargetToSourceBindingMode.EventBased:
                    var notifier = Target as INotifyPropertyChanged;
                    if (notifier != null)
                        notifier.PropertyChanged += OnTargetValueChanged;
                    break;
            }
        }

        private void Detach()
        {
            // We do not reset the target value here because
            // every Detach() call is followed by an Attach() call
            // (even if DataContext == null) which already sets
            // the target value.

            _value = null;
            _sourceProp = null;
            _targetProp = null;

            switch (SourceToTargetMode)
            {
                case SourceToTargetBindingMode.EventBased:
                    var notifier = DataContext as INotifyPropertyChanged;
                    if (notifier != null)
                        notifier.PropertyChanged -= OnSourceValueChanged;
                    break;

                case SourceToTargetBindingMode.Periodically:
                case SourceToTargetBindingMode.EveryFrame:
                    if (_updaterRoutine != null)
                        StopCoroutine(_updaterRoutine);
                    break;
            }

            switch (TargetToSourceMode)
            {
                case TargetToSourceBindingMode.EventBased:
                    var notifier = Target as INotifyPropertyChanged;
                    if (notifier != null)
                        notifier.PropertyChanged -= OnTargetValueChanged;
                    break;
            }
        }

        private void OnSourceValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SourcePropertyName || string.IsNullOrEmpty(e.PropertyName))
                RefreshTargetValue();
        }

        private void OnTargetValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TargetPropertyName)
                RefreshSourceValue();
        }

        private void OnTransformParentChanged()
        {
            DataContext = BindingOperations.FindDataContext(transform);
        }

        private IEnumerator RefreshRepeatedlyAsync(bool everyFrame)
        {
            while (true)
            {
                yield return everyFrame ? null : new WaitForSeconds(Interval);
                RefreshTargetValue();
            }
        }

        private void RefreshTargetValue(bool forceSet = false)
        {
            var newValue = string.IsNullOrEmpty(SourcePropertyName) ?
                DataContext :
                BindingOperations.GetValue(_sourceProp, DataContext);

            if (!Equals(_value, newValue) || forceSet)
            {
                _value = newValue;
                SetTargetValue();
            }
        }

        private void RefreshSourceValue()
        {
            var newValue = BindingOperations.GetValue(_targetProp, Target);

            if (!Equals(_value, newValue))
            {
                _value = newValue;
                SetSourceValue();
            }
        }

        /// <summary>
        /// TODO: This method needs further thinking and testing.
        /// </summary>
        private void SetSourceValue()
        {
            try
            {
                var conversionType = _sourceProp.GetPropertyOrFieldType();
                var converter = (Converter == null) ? (Func<object, object>)null : Converter.ConvertBack;
                var valueToSet = BindingOperations.ConvertValue(
                    _value,
                    conversionType,
                    converter, // Only applies in Source->Target binding
                    null);

                BindingOperations.SetValue(_sourceProp, DataContext, valueToSet);
            }
            catch (Exception e)
            {
                Log(e);
            }

            #region Legacy
            /*
            var sourceType = GetMemberType(_sourceProp); // TODO: What if we are binding directly to DataContext (SourcePropertyName = "")?

            var originalValue = GetTargetValue();
            object valueToSet = null;

            // Try to use source value (use IValueConverter if available)
            try
            {
                var v = (Converter != null) ? Converter.ConvertBack(originalValue) : originalValue;
                valueToSet = BindingOperations.ChangeType(v ?? FallbackValue, sourceType);
            }
            catch
            {
                // Conversion failed -> Try fallback value
                try
                {
                    valueToSet = BindingOperations.ChangeType(FallbackValue, sourceType);
                }
                catch
                {
                    LogFormat(this, "Binding error: Neither the source value of type '{0}' nor the fallback value could be converted to target type '{1}'",
                        originalValue.GetType().Name,
                        sourceType.Name);
                    return;
                }
            }

            if (_sourceProp is FieldInfo)
                (_sourceProp as FieldInfo).SetValue(_dataContext, valueToSet);
            else if (_sourceProp is PropertyInfo)
                (_sourceProp as PropertyInfo).SetValue(_dataContext, valueToSet, null);
            else
                throw new NotImplementedException();
                */
            #endregion
        }

        private void SetTargetValue()
        {
            if (DataContext == null)
            {
                BindingOperations.SetValue(_targetProp, Target, FallbackValue);
            }

            try
            {
                var conversionType = _targetProp.GetPropertyOrFieldType();
                var converter = (Converter == null) ? (Func<object, object>)null : Converter.Convert;
                var valueToSet = BindingOperations.ConvertValue(_value, conversionType, converter, TargetNullValue);
                BindingOperations.SetValue(_targetProp, Target, valueToSet);
            }
            catch (Exception e)
            {
                Log(e);
            }

            #region Legacy
            //var targetType = GetMemberType(_targetProp);

            //object valueToSet = null;

            //if (_value == null)
            //{
            //    // Source value null -> Use fallback value
            //    try
            //    {
            //        valueToSet = BindingOperations.ChangeType(FallbackValue, targetType);
            //    }
            //    catch
            //    {
            //        LogFormat(this, "Binding error: Could not convert fallback value to target type '{0}' (source value was null)", targetType.Name);
            //        return;
            //    }
            //}
            //else
            //{
            //    // Try to use source value (use IValueConverter if available)
            //    try
            //    {
            //        var v = (Converter != null) ? Converter.Convert(_value) : _value;
            //        valueToSet = BindingOperations.ChangeType(v ?? FallbackValue, targetType);
            //    }
            //    catch
            //    {
            //        // Conversion failed -> Try fallback value
            //        try
            //        {
            //            valueToSet = BindingOperations.ChangeType(FallbackValue, targetType);
            //        }
            //        catch
            //        {
            //            LogFormat(this, "Binding error: Neither the source value of type '{0}' nor the fallback value could be converted to target type '{1}'",
            //                _value.GetType().Name,
            //                targetType.Name);
            //            return;
            //        }
            //    }
            //}

            //if (_targetProp is FieldInfo)
            //    (_targetProp as FieldInfo).SetValue(Target, valueToSet);
            //else if (_targetProp is PropertyInfo)
            //    (_targetProp as PropertyInfo).SetValue(Target, valueToSet, null);
            //else
            //    throw new NotImplementedException();
            #endregion
        }

        private void Log(object s, params object[] args)
        {
            var title = string.Format("<b><size=9>BINDING ON '{2}': '{0}' \x00BB '{1}':</size></b>", SourcePropertyName, TargetPropertyName, name);
            Debug.LogFormat(this, title + " " + s.ToString(), args);
        }
    }
}