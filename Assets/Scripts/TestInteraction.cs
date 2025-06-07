using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        UIManager.Instance.ShowDialog("Interaction", "This is a test lololololololo! Blah blah Blah blah Blah blah Blah blah Blah blah Blah blah Blah blah Blah blah Blah blah Blah blah Blah blah ");
    }
}