using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MouseExample : MonoBehaviour
{
    void Update()
    {
        // Check if the left mouse button was clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the mouse was clicked over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Clicked on the UI");
            }
        }
        Debug.Log(EventSystem.current.IsPointerOverGameObject());
    }
}