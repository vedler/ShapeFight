using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace TrueSync {

    [CustomPropertyDrawer(typeof(TSVector2))]
    public class TSVector2Drawer : PropertyDrawer {

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            position.width /= 2f;

            object rawPropValue = this.fieldInfo.GetValue(property.serializedObject.targetObject);

            if (rawPropValue == null) {
                return;
            }

            TSVector2 propValue = TSVector2.zero;
            if (rawPropValue is TSVector2) {
                propValue = (TSVector2) rawPropValue;
            } else if (rawPropValue.GetType().IsArray) {
                EditorGUI.indentLevel -= 1;

                TSVector2[] propArray = ((TSVector2[]) rawPropValue);

                Match match = Regex.Match(property.propertyPath, ".*?\\[(\\d+)\\]$");
                int index = int.Parse(match.Groups[1].ToString());

                if (index < propArray.Length) {
                    propValue = propArray[index];
                } else {
                    propValue = TSVector2.zero;
                }
            }


            var lastPos = position;

            EditorGUI.BeginChangeCheck();
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
        }

    }

}