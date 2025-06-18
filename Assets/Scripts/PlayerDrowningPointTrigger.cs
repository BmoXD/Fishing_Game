using UnityEngine;

public class DrowningPointTrigger : MonoBehaviour
{
    public ThirdPersonController playerController;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Drowning point entered trigger: "+other);
        if (other.CompareTag("Water") && playerController != null)
        {
            Debug.Log("Let's start drowning");
            playerController.OnDrowningPointEnterWater();
        }
    }
}