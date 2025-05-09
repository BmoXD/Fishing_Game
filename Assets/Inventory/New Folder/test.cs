using UnityEngine;
using UnityEditor;

public class InventoryTester : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private Item itemToTest;
    [SerializeField] private int quantity = 1;
    
    [Header("Status")]
    [SerializeField, ReadOnly] private int currentQuantity = 0;
    [SerializeField, ReadOnly] private int instanceCount = 0;
    
    private void Update()
    {
        // Update current quantity display if item exists in inventory
        if (InventoryManager.Instance != null && itemToTest != null)
        {
            InventoryItem inventoryItem = InventoryManager.Instance.GetInventoryItem(itemToTest);
            currentQuantity = inventoryItem != null ? inventoryItem.Quantity : 0;
            
            // Count instances of this item type
            instanceCount = InventoryManager.Instance.GetItemInstanceCount(itemToTest);
        }
    }
    
#if UNITY_EDITOR
    // Custom inspector with buttons
    [CustomEditor(typeof(InventoryTester))]
    public class InventoryTesterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            InventoryTester tester = (InventoryTester)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Inventory Actions", EditorStyles.boldLabel);
            
            using (new EditorGUI.DisabledScope(tester.itemToTest == null))
            {
                EditorGUILayout.BeginHorizontal();
                
                // Add button (adds to quantity)
                if (GUILayout.Button("Add to Quantity", GUILayout.Height(30)))
                {
                    if (Application.isPlaying && InventoryManager.Instance != null)
                    {
                        bool success = InventoryManager.Instance.AddItem(tester.itemToTest, tester.quantity);
                        Debug.Log(success ? 
                            $"Added {tester.quantity}x {tester.itemToTest.name} to inventory" : 
                            "Failed to add item");
                    }
                    else
                    {
                        Debug.LogWarning("Can only add items in Play Mode!");
                    }
                }
                
                // Add new instance button
                if (GUILayout.Button("Add New Instance", GUILayout.Height(30)))
                {
                    if (Application.isPlaying && InventoryManager.Instance != null)
                    {
                        bool success = InventoryManager.Instance.AddItemAsNewInstance(tester.itemToTest);
                        Debug.Log(success ? 
                            $"Added new instance of {tester.itemToTest.name} to inventory" : 
                            "Failed to add new instance");
                    }
                    else
                    {
                        Debug.LogWarning("Can only add items in Play Mode!");
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Remove buttons row
                EditorGUILayout.BeginHorizontal();
                
                // Remove from quantity button
                if (GUILayout.Button("Remove from Quantity", GUILayout.Height(30)))
                {
                    if (Application.isPlaying && InventoryManager.Instance != null)
                    {
                        bool success = InventoryManager.Instance.RemoveItem(tester.itemToTest, tester.quantity);
                        Debug.Log(success ? 
                            $"Removed {tester.quantity}x {tester.itemToTest.name} from inventory" : 
                            "Failed to remove item (not enough quantity or item doesn't exist)");
                    }
                    else
                    {
                        Debug.LogWarning("Can only remove items in Play Mode!");
                    }
                }
                
                // Remove instance button
                if (GUILayout.Button("Remove Instance", GUILayout.Height(30)))
                {
                    if (Application.isPlaying && InventoryManager.Instance != null)
                    {
                        bool success = InventoryManager.Instance.RemoveItemInstance(tester.itemToTest);
                        Debug.Log(success ? 
                            $"Removed an instance of {tester.itemToTest.name} from inventory" : 
                            "Failed to remove instance (no instances exist)");
                    }
                    else
                    {
                        Debug.LogWarning("Can only remove items in Play Mode!");
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
    }
#endif
}

// Helper attribute to make fields read-only in the inspector
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif