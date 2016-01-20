#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CM = System.ComponentModel;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    [CustomEditor(typeof(Binding))]
    [CanEditMultipleObjects]
    public class BindingEditor : Editor
    {
        private static readonly GUIStyle _titleStyle = new GUIStyle(GUIStyle.none) { fontStyle = FontStyle.BoldAndItalic, margin = new RectOffset(16, 0, 10, 0) };

        private static readonly BindingFlags _flags = /*BindingFlags.DeclaredOnly |*/ BindingFlags.Public | BindingFlags.Instance;

        private static readonly Type[] _illegalTargetComponents = new[] { typeof(Binding), typeof(DataContext) };

        private MemberInfo _targetSelectedProp;

        //private bool _useFallbackValue;

        private SerializedProperty _sourcePropertyName;
        private SerializedProperty _fallbackValue;
        private SerializedProperty _target;
        private SerializedProperty _targetPropertyName;
        private SerializedProperty _converter;
        private SerializedProperty _sourceToTargetMode;
        private SerializedProperty _targetToSourceMode;
        private SerializedProperty _interval;

        private void OnEnable()
        {
            _sourcePropertyName = serializedObject.FindProperty("SourcePropertyName");
            //_fallbackValue = serializedObject.FindProperty("FallbackValue");
            _target = serializedObject.FindProperty("Target");
            _targetPropertyName = serializedObject.FindProperty("TargetPropertyName");
            _converter = serializedObject.FindProperty("Converter");
            _sourceToTargetMode = serializedObject.FindProperty("SourceToTargetMode");
            _targetToSourceMode = serializedObject.FindProperty("TargetToSourceMode");
            _interval = serializedObject.FindProperty("Interval");

            //_useFallbackValue = (target as Binding).FallbackValue != null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var targetProps = RebuildPropertyList(_target.objectReferenceValue, ref _targetSelectedProp, true);
            _targetSelectedProp = targetProps.FirstOrDefault(o => o.Name == _targetPropertyName.stringValue);

            EditorGUILayout.PropertyField(_converter, new GUIContent("Converter", "If necessary a value converter can be specified that converts the source value of the binding to a value of the target type"));

            GUILayout.Label("Source", _titleStyle);
            EditorGUI.indentLevel++;
            RenderSourceSection();
            EditorGUI.indentLevel--;

            GUILayout.Label("Target", _titleStyle);
            EditorGUI.indentLevel++;
            RenderTargetSection(targetProps);
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private void RenderSourceSection()
        {
            EditorGUILayout.PropertyField(_sourceToTargetMode, new GUIContent("Mode", "Determines how the value is updated\n\nEvent Based:\nThe value is updated only if the data context implements INotifyPropertyChanged and raises PropertyChanged-events\n\nOne Time:\nThe value is obtained only once during startup\n\nEvery Frame:\nThe value is updated every frame (this may negatively impact the performance)\n\nPeriodically:\nThe value is updated in a certain interval\n\n"));

            if (_sourceToTargetMode.enumValueIndex == 2) // If 'Periodically'
                EditorGUILayout.PropertyField(_interval, new GUIContent("Interval", "The interval (in seconds) between value updates"));

            EditorGUILayout.PropertyField(_sourcePropertyName, new GUIContent("Property Name", "The property on the data context"));
            //EditorGUILayout.PropertyField(_fallbackValue, new GUIContent("Fallback Value", "The value that is used when the source value is null or the conversion to the target type failed"));
        }

        private void RenderTargetSection(MemberInfo[] propOptions)
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("The binding target can only be edited per instance", MessageType.None);
                return;
            }

            EditorGUILayout.PropertyField(_targetToSourceMode, new GUIContent("Mode", "Determines how the value is updated\n\nDisabled:\nIf the value changes, it is never written back to the DataContext\n\nEvent Based:\nThe value is updated only if the target implements INotifyPropertyChanged and raises PropertyChanged-events"));

            var components = (target as Binding).GetComponents<Component>().Where(c => !_illegalTargetComponents.Contains(c.GetType()));
            _target.objectReferenceValue = PopupEx("Component", (Component)_target.objectReferenceValue, components, c => (c as Component).GetType().Name);

            if (_target.objectReferenceValue == null)
            {
                RenderError("No target component is assigned");
            }
            else if (propOptions.Length == 0)
            {
                RenderError("The target component does not have any public writable fields or properties");
            }
            else
            {
                _targetSelectedProp = PopupEx("Property", _targetSelectedProp, propOptions, GetDisplayName);
                _targetPropertyName.stringValue = (_targetSelectedProp == null) ? "" : _targetSelectedProp.Name;

                if (_targetSelectedProp == null)
                    RenderError("No target property is assigned");
            }
        }

        private void RenderError(string text, params object[] args)
        {
            EditorGUILayout.HelpBox(string.Format(text, args), MessageType.Error);
            EditorGUILayout.Separator();
        }

        private static MemberInfo[] RebuildPropertyList(object obj, ref MemberInfo selectedProp, bool writableOnly)
        {
            if (obj == null)
            {
                selectedProp = null;
                return new MemberInfo[0];
            }
            else
            {
                var t = obj.GetType();
                var props =
                    t.GetFields(_flags).Concat<MemberInfo>(
                    t.GetProperties(_flags).Where(p => !writableOnly || p.CanWrite).Cast<MemberInfo>())
                    .OrderBy(m => m.Name).ToArray();

                // Restore previously selected member
                var previouslySelected = selectedProp;
                selectedProp = props.FirstOrDefault(m => m.Equals(previouslySelected));

                return props;
            }
        }

        private static T PopupEx<T>(string label, T selected, IEnumerable<T> options, Func<T, string> displayNameSelector)
        {
            var optionsArray = options.ToArray();
            var selectedIndex = Array.IndexOf(optionsArray, selected); // Might be -1
            var optionNames = optionsArray.Select(o => displayNameSelector(o)).ToArray();
            var newSelectedIndex = EditorGUILayout.Popup(label, selectedIndex, optionNames);

            return (newSelectedIndex == -1) ? default(T) : optionsArray[newSelectedIndex];
        }

        private static string GetDisplayName(MemberInfo m)
        {
            if (m is FieldInfo)
                return m.Name + " : " + (m as FieldInfo).FieldType.Name;
            else if (m is PropertyInfo)
                return m.Name + " : " + (m as PropertyInfo).PropertyType.Name;
            else
                return m.Name;
        }
    }
}
#endif