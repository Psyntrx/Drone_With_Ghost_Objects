using UnityEngine;
using UnityEngine.UI;

public class ResetManager : MonoBehaviour
{
    [Header("UI")]
    public Button resetButton;

    [Header("Parts")]
    public GameObject[] parts;
    public Vector3[] defaultPositions;
    public Quaternion[] defaultRotations;

    [Header("Player Reset")]
    public Transform xrOrigin;
    public Vector3 defaultPlayerPosition = Vector3.zero;
    public Quaternion defaultPlayerRotation = Quaternion.identity;

    private void Start()
    {
        resetButton.onClick.AddListener(ResetAll);
    }

    public void ResetAll()
    {
        // Reset all slots
        SlotReceiver[] allSlots = FindObjectsByType<SlotReceiver>(FindObjectsSortMode.None);
        foreach (SlotReceiver slot in allSlots)
        {
            slot.isOccupied = false;
            slot.HideGhost();
        }

        // Reset all parts
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == null) continue;

            Rigidbody rb = parts[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            parts[i].transform.position = defaultPositions[i];
            parts[i].transform.rotation = defaultRotations[i];

            SnapPart snapPart = parts[i].GetComponent<SnapPart>();
            if (snapPart != null)
                snapPart.CurrentSlot = null;
        }

        // Reset drone
        DroneManager droneManager = FindAnyObjectByType<DroneManager>();
        if (droneManager != null)
            droneManager.ResetDrone();

        // Reset player
        if (xrOrigin != null)
        {
            xrOrigin.position = defaultPlayerPosition;
            xrOrigin.rotation = defaultPlayerRotation;
        }

        Debug.Log("Full reset complete.");
    }
}