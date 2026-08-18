// Copyright (c) 2026 Purabe Works
// Released under the MIT License. See LICENSE.txt for details.
using UdonSharpEditor;
using UnityEditor;

namespace PurabeWorks.SpawnObject.Editor
{
    [CustomEditor(typeof(SpawnObject))]
    [CanEditMultipleObjects]
    public class SpawnObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty vRCObjectPool;
        private SerializedProperty randomSpawn;
        private SerializedProperty moveItemToHand;
        private SerializedProperty spawnPoint;
        private SerializedProperty spawnDelayFrames;
        private SerializedProperty audioSource;
        private SerializedProperty audioClip;

        private void OnEnable()
        {
            vRCObjectPool = serializedObject.FindProperty("vRCObjectPool");
            randomSpawn = serializedObject.FindProperty("randomSpawn");
            moveItemToHand = serializedObject.FindProperty("moveItemToHand");
            spawnPoint = serializedObject.FindProperty("spawnPoint");
            spawnDelayFrames = serializedObject.FindProperty("spawnDelayFrames");
            audioSource = serializedObject.FindProperty("_audioSource");
            audioClip = serializedObject.FindProperty("_audioClip");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(vRCObjectPool);
            EditorGUILayout.PropertyField(randomSpawn);
            EditorGUILayout.PropertyField(moveItemToHand);

            bool moveToHand = !moveItemToHand.hasMultipleDifferentValues
                && moveItemToHand.boolValue;
            using (new EditorGUI.DisabledScope(moveToHand))
            {
                EditorGUILayout.PropertyField(spawnPoint);
            }

            if (moveToHand)
            {
                EditorGUILayout.HelpBox(
                    "手元へ移動が有効なため、出現先設定は使用されません。",
                    MessageType.Info);
            }

            EditorGUILayout.PropertyField(spawnDelayFrames);
            EditorGUILayout.PropertyField(audioSource);
            EditorGUILayout.PropertyField(audioClip);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
