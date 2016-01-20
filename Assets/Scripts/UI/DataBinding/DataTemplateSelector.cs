using UnityEngine;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    public abstract class DataTemplateSelector : MonoBehaviour
    {
        public abstract GameObject SelectTemplate(object value);
    }
}