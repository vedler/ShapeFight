using System;
using UnityEditor;
using UnityEngine;

namespace TrueSync {

    [CustomPropertyDrawer(typeof(FP))]
    public class TSFPDrawer : PropertyDrawer {

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty rawValue = property.FindPropertyRelative("_serializedValue");

            FP propValue = (FP) this.fieldInfo.GetValue(property.serializedObject.targetObject);

            EditorGUI.BeginChangeCheck();
            propValue = EditorGUI.FloatField(position, label, (float) propValue);

            if (EditorGUI.EndChangeCheck()) {
                rawValue.stringValue = propValue.RawValue.ToString();
            }
        }

    }

}