using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 20f;
    public LayerMask pickupLayer;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogError("PlayerInteraction: No Main Camera found! " +
                           "Make sure your camera is tagged as MainCamera.");
    }

    private void Update()
    {
        if (mainCamera == null) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, pickupLayer))
            {
                Debug.Log("Clicked: " + hit.collider.gameObject.name);
            }
        }
    }
}