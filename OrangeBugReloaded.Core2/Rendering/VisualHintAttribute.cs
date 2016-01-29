using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OrangeBugReloaded.Core.Rendering
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VisualHintAttribute : Attribute
    {
        /// <summary>
        /// Expression that is used to calculate the final name.
        /// Parts in curly braces, such as "{IsOn}", are replaced by their
        /// respective property values of the annotated object.
        /// </summary>
        public string Expression { get; }

        public VisualHintAttribute(string expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Gets the name calculated by the expression of the <see cref="VisualHintAttribute"/>
        /// that the specified object is annotated with.
        /// If the specified object does not have the <see cref="VisualHintAttribute"/>
        /// its type name is returned.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetVisualName(object o)
        {
            var visualHint = o.GetType().GetTypeInfo().GetCustomAttribute<VisualHintAttribute>();

            if (visualHint?.Expression == null)
            {
                return o.GetType().Name;
            }
            else
            {
                return Regex.Replace(visualHint.Expression, @"\{(\w+)\s*(?:\?\s*(\w*)\s*\:\s*(\w*))?\}", match =>
                {
                    var prop = o.GetType().GetProperty(match.Groups[1].Value);
                    var value = prop?.GetValue(o);

                    if (value.GetType() == typeof(bool) && match.Groups.Count >= 4)
                        return (bool)value ? match.Groups[2].Value : match.Groups[3].Value;
                    else
                        return value.ToString();
                });
            }
        }
    }
}
