using UnityEngine;
using Cinemachine;

public class BillboardEffect : MonoBehaviour
{
    void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    void OnCameraUpdated(CinemachineBrain brain)
    {
        if (Camera.main == null) return;
        // Make the object face the camera
        transform.forward = Camera.main.transform.forward;
    }
}
