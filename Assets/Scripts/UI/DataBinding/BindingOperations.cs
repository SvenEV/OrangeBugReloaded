using OrangeBugReloaded.Core.Foundation;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    public static class BindingOperations
    {
        private static readonly Type[] _convertTypes = new[] { typeof(object), typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(string) };

        public static MemberInfo GetMember(object o, string propertyName)
        {
            if (o == null)
                throw new ArgumentNullException("o");

            return string.IsNullOrEmpty(propertyName) ? null :
                o.GetType().GetProperty(propertyName) as MemberInfo ??
                o.GetType().GetField(propertyName);
        }

        public static Type GetPropertyOrFieldType(this MemberInfo member)
        {
            if (member is FieldInfo)
                return (member as FieldInfo).FieldType;
            else if (member is PropertyInfo)
                return (member as PropertyInfo).PropertyType;
            else
                throw new NotImplementedException();
        }

        public static object GetValue(MemberInfo member, object o)
        {
            object value;

            if (member == null)
                value = o;
            else if (member is FieldInfo)
                value = (member as FieldInfo).GetValue(o);
            else if (member is PropertyInfo)
                value = (member as PropertyInfo).GetValue(o, null);
            else
                throw new NotImplementedException();

            return value;
        }

        public static void SetValue(MemberInfo member, object o, object value)
        {
            if (member == null)
                throw new ArgumentNullException("member");

            var valueToSet = ChangeType(value, member.GetPropertyOrFieldType());

            if (member is FieldInfo)
                (member as FieldInfo).SetValue(o, valueToSet);
            else if (member is PropertyInfo)
                (member as PropertyInfo).SetValue(o, valueToSet, null);
            else
                throw new NotImplementedException();
        }

        public static object ConvertValue(object value, Type targetType, Func<object, object> converter = null, object nullValue = null)
        {
            object resultValue = null;

            if (value == null && nullValue != null)
            {
                // Use targetNullValue if available
                resultValue = nullValue;
            }
            else
            {
                // Value might still be null here.
                // If conversion fails, exception is propagated to caller.
                resultValue = (converter != null) ? converter(value) : value;
            }

            return ChangeType(resultValue, targetType);
        }

        public static object FindDataContext(Transform transform)
        {
            var dataContextComponent = transform.GetComponentInParent<DataContext>();
            return (dataContextComponent == null) ? null : dataContextComponent.DataContextObject;
        }

        public static object ChangeType(object value, Type targetType)
        {
            if (value == null)
            {
                // If value is null, return default(targetType) (or "" in case of string)
                return
                    targetType == typeof(string) ? "" :
                    targetType.CheckIsValueType() ? Activator.CreateInstance(targetType) : null;
            }
            else if (targetType.IsAssignableFrom(value.GetType()))
            {
                // If only cast required, do nothing
                return value;
            }
            else if (targetType.Equals(typeof(string)))
            {
                // If conversion to string, use ToString()
                return value.ToString();
            }
            else
            {
                // Else try Convert.ChangeType(...)
                try
                {
                    return Convert.ChangeType(value, targetType);
                }
                catch (Exception e)
                {
                    // InvalidCastException: Conversion is not supported or value doesn't implement IConvertible
                    // FormatException: value is not in a format recognized by targetType
                    // OverflowException: value represents a number that is out of the range of targetType
                    throw new Exception(string.Format("Cannot convert '{0}' from type '{1} to type '{2}'",
                        value ?? "null",
                        value == null ? "null" : value.GetType().ToString(),
                        targetType), e);
                }
            }
        }

        public static bool CanChangeType(Type from, Type to, out bool conversionMightFail)
        {
            if (to.Equals(typeof(string)) || to.IsAssignableFrom(from))
            {
                // Conversion to string and simple casts always work
                conversionMightFail = false;
                return true;
            }
            else if (_convertTypes.Contains(to) || typeof(IConvertible).IsAssignableFrom(from))
            {
                // This conversion is supported but might fail depending on the actual values
                conversionMightFail = true;
                return true;
            }

            conversionMightFail = true;
            return false;
        }
    }
}
