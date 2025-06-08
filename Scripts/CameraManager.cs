using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera playerCamera;
    [SerializeField] private CinemachineCamera dialogueCamera;

    private void Start()
    {
        if (playerCamera != null)
        {
            playerCamera.Priority = 10;
        }
        if (dialogueCamera != null)
        {
            dialogueCamera.Priority = 0;
        }
    }

    public void SwitchToDialogueCamera(Transform anchor)
    {
        if (dialogueCamera == null)
        {
            Debug.LogWarning("Dialogue camera is not assigned.");
            return;
        }
        if (anchor != null)
        {
            dialogueCamera.transform.position = anchor.position;
            dialogueCamera.transform.rotation = anchor.rotation;
        }
        dialogueCamera.Priority = 20;
    }

    public void SwitchToPlayerCamera()
    {
        if (dialogueCamera != null)
        {
            dialogueCamera.Priority = 0;
        }
        if (playerCamera != null)
        {
            playerCamera.Priority = 10;
        }
    }
}