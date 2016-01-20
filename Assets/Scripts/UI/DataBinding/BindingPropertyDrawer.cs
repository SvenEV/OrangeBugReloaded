#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SvenVinkemeier.Unity.UI.DataBinding;
using System.Linq;

// Temporarily disabled:
//[CustomPropertyDrawer(typeof(object), true)]
public class BindingPropertyDrawer : PropertyDrawer
{
    private const float _popupHeight = 80;
    private const float _buttonWidth = 10;

    private static GUIStyle _buttonStyle = GUIStyle.none;

    private bool _isOpen;
    private float _defaultHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Multi-editing is not supported, fall back to default property drawer
        // Binding to individual properties such as position.x is not supported too
        if (property.serializedObject.isEditingMultipleObjects || property.name != property.propertyPath)
            EditorGUI.PropertyField(position, property, label);

        var left = new Rect(position.x, position.y, position.width - _buttonWidth - 4, _defaultHeight);
        var right = new Rect(position.x + left.width + 4, position.y, _buttonWidth, 16);

        EditorGUI.PropertyField(left, property, label);

        if (_isOpen)
        {
            var contentRect = new Rect(position.x, position.y + _defaultHeight, position.width, position.height);

            var go = property.serializedObject.targetObject as Component;
            var binding = go.GetComponents<Binding>()
                .FirstOrDefault(b => b.Target == go && b.TargetPropertyName == property.name);

            if (binding == null)
            {
                EditorGUI.LabelField(contentRect, "No binding exists");
            }
            else
            {
                EditorGUI.LabelField(contentRect, "Found binding");
            }
        }

        if (GUI.Button(right, new GUIContent("B", "Edit the binding to this property"), _buttonStyle))
            _isOpen = !_isOpen;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        _defaultHeight = EditorGUI.GetPropertyHeight(property, label);
        return _defaultHeight + (_isOpen ? _popupHeight : 0);
    }
}
#endif