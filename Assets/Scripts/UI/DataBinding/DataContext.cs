using System.ComponentModel;
using UnityEngine;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    [DisallowMultipleComponent]
    [AddComponentMenu("MVVM/Data Context")]
    public class DataContext : MonoBehaviour
    {
        [SerializeField]
        private Object _unityReference = null;

        [SerializeField]
        private string _propertyName = "";

        [SerializeField]
        private ValueConverter _converter = null;

        private object _dataContextObject; // The explicitly assigned value
        private object _inheritedDataContext; // The value inherited from another DataContext
        private object _actualObject; // The actual value that is distributed down the hierarchy

        public object DataContextObject
        {
            get { return _dataContextObject; }
            set
            {
                if (!Equals(_dataContextObject, value))
                {
                    Detach(_dataContextObject);
                    _dataContextObject = value;
                    Attach(_dataContextObject);
                    RefreshActualObject();
                }
            }
        }

        internal object ActualObject { get { return _actualObject; } }

        public string PropertyName
        {
            get { return _propertyName ?? ""; }
            set
            {
                if (!Equals(_propertyName, value ?? ""))
                {
                    _propertyName = value ?? "";
                    RefreshActualObject();
                }
            }
        }

        public object InheritedDataContext
        {
            get { return _inheritedDataContext; }
            set
            {
                if (!Equals(_inheritedDataContext, value))
                {
                    Detach(_inheritedDataContext);
                    _inheritedDataContext = value;
                    Attach(_inheritedDataContext);
                    RefreshActualObject();
                }
            }
        }

        private void Start()
        {
            // If a component is explicitly assigned in the Unity Editor, use that
            if (_unityReference != null)
                _dataContextObject = _unityReference;

            if (_dataContextObject != null)
                Attach(_dataContextObject);

            RefreshActualObject(forceSpread: true);
        }

        private void OnDestroy()
        {
            _inheritedDataContext = null;
            _dataContextObject = null;
            RefreshActualObject();
        }

        private void Attach(object o)
        {
            var notifier = o as INotifyPropertyChanged;
            if (notifier != null)
                notifier.PropertyChanged += OnDataContextObjectPropertyChanged;
        }

        private void Detach(object o)
        {
            var notifier = o as INotifyPropertyChanged;
            if (notifier != null)
                notifier.PropertyChanged -= OnDataContextObjectPropertyChanged;
        }

        private void OnDataContextObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _propertyName || string.IsNullOrEmpty(e.PropertyName))
                RefreshActualObject();
        }

        private void RefreshActualObject(bool forceSpread = false)
        {
            var newActualObject = _actualObject;

            if (_dataContextObject == null)
            {
                // Use inherited object (if available)
                if (_inheritedDataContext == null)
                {
                    newActualObject = null;
                }
                else
                {
                    var member = BindingOperations.GetMember(_inheritedDataContext, _propertyName);

                    if (member == null && !string.IsNullOrEmpty(_propertyName))
                        Debug.LogWarningFormat(this,
                            "DataContext error: Could not find field or property '{0}' on item '{1}'",
                            _propertyName,
                            _inheritedDataContext);
                    else
                        newActualObject = string.IsNullOrEmpty(_propertyName) ?
                            _inheritedDataContext :
                            BindingOperations.GetValue(member, _inheritedDataContext);
                }
            }
            else
            {
                // Use explicitly assigned object
                var member = BindingOperations.GetMember(_dataContextObject, _propertyName);

                if (member == null && !string.IsNullOrEmpty(_propertyName))
                    Debug.LogWarningFormat(this,
                        "DataContext error: Could not find field or property '{0}' on item '{1}'",
                        _propertyName,
                        _dataContextObject);
                else
                    newActualObject = string.IsNullOrEmpty(_propertyName) ?
                        _dataContextObject :
                        BindingOperations.GetValue(member, _dataContextObject);
            }

            if (_converter != null)
                newActualObject = _converter.Convert(newActualObject);

            // If actualObject has changed, distribute it down the hierarchy
            if (!Equals(newActualObject, _actualObject) || forceSpread)
            {
                _actualObject = newActualObject;
                SpreadDataContext(_actualObject, transform);
            }
        }

        private void SpreadDataContext(object dataContext, Transform node)
        {
            var dataContextComponent = node.GetComponent<DataContext>();

            if (node != transform && dataContextComponent != null)
            {
                // Found another data context, do not spread this one further down the hierarchy
                dataContextComponent.InheritedDataContext = dataContext;
                return;
            }

            var bindings = node.GetComponents<Binding>();

            foreach (var b in bindings)
            {
                b.DataContext = dataContext;
                //Debug.LogFormat(transform, "Set '{0}' as DataContext on '{1}'", dataContext, b.name);
            }

            foreach (Transform t in node)
                SpreadDataContext(dataContext, t);
        }
    }
}

#if UNITY_EDITOR
namespace SvenVinkemeier.Unity.UI.DataBinding
{
    using UnityEditor;

    [CustomEditor(typeof(DataContext))]
    public class DataContextEditor : Editor
    {
        private SerializedProperty _unityReferenceProperty;
        private SerializedProperty _propertyNameProperty;
        private SerializedProperty _converterProperty;

        private void OnEnable()
        {
            _unityReferenceProperty = serializedObject.FindProperty("_unityReference");
            _propertyNameProperty = serializedObject.FindProperty("_propertyName");
            _converterProperty = serializedObject.FindProperty("_converter");
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                var dc = ((DataContext)target).ActualObject;
                EditorGUILayout.LabelField(dc == null ? "null" : dc.ToString());
            }
            else
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(_unityReferenceProperty, new GUIContent("Object", "The object used as the data context for bindings on descendant GameObjects. If empty, this value is inherited from the nearest parent DataContext."));
                EditorGUILayout.PropertyField(_propertyNameProperty, new GUIContent("Property Name", ""));
                EditorGUILayout.PropertyField(_converterProperty);

                if (_unityReferenceProperty.objectReferenceValue == null)
                {
                    if (string.IsNullOrEmpty(_propertyNameProperty.stringValue))
                        EditorGUILayout.HelpBox("In its current configuration, this DataContext component is redundant", MessageType.Warning);
                    else
                        EditorGUILayout.HelpBox("The data context is inherited from a parent DataContext component. The value of the property '" + _propertyNameProperty.stringValue + "' is passed to all descendant GameObjects.", MessageType.None);
                }
                else
                {
                    if (string.IsNullOrEmpty(_propertyNameProperty.stringValue))
                        EditorGUILayout.HelpBox("The specified object is passed to all descendant GameObjects.", MessageType.None);
                    else
                        EditorGUILayout.HelpBox("The value of the property '" + _propertyNameProperty.stringValue + "' of the specified object is passed to all descendant GameObjects.", MessageType.None);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif