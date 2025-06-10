using UnityEngine;

public class SomeInteractableObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        UIManager.Instance.ShowDialog("Test","Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah");
        Debug.Log("Interacted with " + gameObject.name);
    }
}