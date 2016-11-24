using UnityEditor;
using UnityEngine;

namespace TrueSync {

    [CustomPropertyDrawer(typeof(TSVector))]
    public class TSVectorDrawer : PropertyDrawer {

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            position.width /= 3f;

            TSVector propValue = (TSVector)this.fieldInfo.GetValue(property.serializedObject.targetObject);

            EditorGUI.BeginChangeCheck();
            var lastPos = position;

            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent("X")).x;
            propValue.x = EditorGUI.FloatField(position, "X", (float)propValue.x);

            if (EditorGUI.EndChangeCheck()) {
                property.FindPropertyRelative("x._serializedValue").stringValue = propValue.x.RawValue.ToString();
            }

            EditorGUI.BeginChangeCheck();
            lastPos.x += lastPos.width;
            position = lastPos;

            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent("Y")).x + 1;
            propValue.y = EditorGUI.FloatField(position, "Y", (float)propValue.y);

            if (EditorGUI.EndChangeCheck()) {
                property.FindPropertyRelative("y._serializedValue").stringValue = propValue.y.RawValue.ToString();
            }

            EditorGUI.BeginChangeCheck();
            lastPos.x += lastPos.width;
            position = lastPos;

            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent("Z")).x + 1;
            propValue.z = EditorGUI.FloatField(position, "Z", (float)propValue.z);

            if (EditorGUI.EndChangeCheck()) {
                property.FindPropertyRelative("z._serializedValue").stringValue = propValue.z.RawValue.ToString();
            }

            EditorGUI.EndProperty();
        }

    }

}