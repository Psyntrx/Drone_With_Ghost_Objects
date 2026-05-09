using UnityEngine;

public class SlotReceiver : MonoBehaviour
{
    [Header("Slot Settings")]
    public string acceptedPartTag;
    public bool isOccupied = false;

    [Header("Snap Correction")]
    public Vector3 snapPositionOffset = Vector3.zero;

    [Header("References")]
    public GameObject ghostObject;

    private DroneManager droneManager;
    private GameObject occupyingPart; // track what's placed here

    private void Start()
    {
        droneManager = Object.FindAnyObjectByType<DroneManager>();

        if (droneManager == null)
            Debug.LogWarning("SlotReceiver: No DroneManager found in scene!");

        if (ghostObject != null)
            ghostObject.SetActive(false);
    }

    public void ShowGhost()
    {
        if (!isOccupied && ghostObject != null)
            ghostObject.SetActive(true);
    }

    public void HideGhost()
    {
        if (ghostObject != null)
            ghostObject.SetActive(false);
    }

    public void OccupySlot(GameObject part)
    {
        isOccupied = true;
        occupyingPart = part;
        HideGhost();

        // Use a snap offset to correct pivot misalignment
        part.transform.position = transform.position + snapPositionOffset;
        part.transform.rotation = transform.rotation;

        if (droneManager != null)
            droneManager.OnPartPlaced();
    }

    // Call this to remove a part from this slot
    public void VacateSlot()
    {
        isOccupied = false;
        occupyingPart = null;

        if (droneManager != null)
            droneManager.OnPartRemoved();
    }
}