#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty itemIDProp = serializedObject.FindProperty("itemID");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(itemIDProp, new GUIContent("Item ID"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));

        var type = (ItemType)serializedObject.FindProperty("type").enumValueIndex;

        if (type == ItemType.Default)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("basePricePerGram"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minWeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxWeight"));
        }

        if (type == ItemType.Consumable)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deleteOnConsume"));
        }

        if (type == ItemType.Consumable || type == ItemType.Tool)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("functionalityScript"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onScreenHints"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
