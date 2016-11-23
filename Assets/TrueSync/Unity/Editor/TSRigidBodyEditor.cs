using UnityEngine;
using UnityEditor;

namespace TrueSync {

    /**
    *  @brief Custom editor to {@link TSRigidBody}.
    **/
    [CustomEditor(typeof(TSRigidBody))]
    [CanEditMultipleObjects]
    public class TSRigidBodyEditor : Editor {

        SerializedProperty freezeZAxis;

        void OnEnable() {
            freezeZAxis = serializedObject.FindProperty("freezeZAxis");
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (PhysicsManager.instance != null && PhysicsManager.instance is Physics2DWorldManager) {
                EditorGUILayout.Separator();

                serializedObject.Update();
                EditorGUILayout.PropertyField(freezeZAxis, new GUIContent("Freeze Z Orientation"));
                serializedObject.ApplyModifiedProperties();
            }
        }

    }

}