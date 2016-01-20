using UnityEngine;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    public abstract class ValueConverter : MonoBehaviour
    {
        public abstract object Convert(object value);

        public abstract object ConvertBack(object value);
    }
}