using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class PickupController : MonoBehaviour
{
    [Header("Part Settings")]
    public string partTag;
    public float snapDistance = 1.5f;

    [Header("Hold Settings")]
    public float mouseHoldDistance = 2.5f;
    public float followSpeed = 20f;
    public float scrollSpeed = 0.5f;
    public float minHoldDistance = 0.5f;
    public float maxHoldDistance = 10f;

    private bool isHeld = false;
    private Rigidbody rb;
    private Camera mainCamera;
    private XRGrabInteractable xrGrab;
    private SlotReceiver[] allSlots;
    private Vector3 targetPosition;

    // Finds which slot this part is currently snapped into
    private SlotReceiver GetCurrentSlot()
    {
       /* foreach (SlotReceiver slot in allSlots)
        {
            if (slot.isOccupied &&
                Vector3.Distance(transform.position, slot.transform.position) < 0.1f)
                return slot;
        }
        return null;
      */

        var snapPart = GetComponent<SnapPart>();
        return snapPart != null ? snapPart.CurrentSlot: null;
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        xrGrab = GetComponent<XRGrabInteractable>();
        allSlots = FindObjectsByType<SlotReceiver>(FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        if (xrGrab != null)
        {
            xrGrab.selectEntered.AddListener(OnVRGrab);
            xrGrab.selectExited.AddListener(OnVRRelease);
        }
    }

    private void OnDisable()
    {
        if (xrGrab != null)
        {
            xrGrab.selectEntered.RemoveListener(OnVRGrab);
            xrGrab.selectExited.RemoveListener(OnVRRelease);
        }
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null) return;

        // ── CLICK TO PICK UP ──
        if (Mouse.current.leftButton.wasPressedThisFrame && !isHeld)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 20f))
            {
                if (hit.collider.gameObject == gameObject)
                    PickUp();
            }
        }

        // ── RELEASE ──
        if (Mouse.current.leftButton.wasReleasedThisFrame && isHeld)
        {
            Release();
        }

        // ── DRAG WHILE HELD ──
        if (isHeld)
        {
            // ── SCROLL TO PUSH OR PULL ──
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0f)
            {
                mouseHoldDistance += scroll * scrollSpeed;
                mouseHoldDistance = Mathf.Clamp(
                    mouseHoldDistance,
                    minHoldDistance,
                    maxHoldDistance
                );
            }

            // ── FOLLOW MOUSE IN 3D ──
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            targetPosition = ray.GetPoint(mouseHoldDistance);

            rb.MovePosition(Vector3.Lerp(
                rb.position,
                targetPosition,
                Time.deltaTime * followSpeed
            ));

            // ── GHOST VISIBILITY BASED ON DISTANCE TO SLOT ──
            foreach (SlotReceiver slot in allSlots)
            {
                if (slot.acceptedPartTag != partTag || slot.isOccupied) continue;

                float dist = Vector3.Distance(transform.position, slot.transform.position);

                if (dist < snapDistance * 3f)
                    slot.ShowGhost();
                else
                    slot.HideGhost();
            }
        }
    }

    // ── VR ──────────────────────────────────────────────────────

    private void OnVRGrab(SelectEnterEventArgs args)
    {
        HandlePickUp();
    }

    private void OnVRRelease(SelectExitEventArgs args)
    {
        TrySnap();
        HideAllMatchingGhosts();
    }

    // ── PICK UP / RELEASE ───────────────────────────────────────

    private void HandlePickUp()
    {
        SlotReceiver occupiedSlot = GetCurrentSlot();
        if (occupiedSlot != null)
        {
            occupiedSlot.VacateSlot();
            var snapPart = GetComponent<SnapPart>();
            if (snapPart != null)
                snapPart.CurrentSlot = null;
        }

        ShowAllMatchingGhosts();
    }
    private void PickUp()
    {
        isHeld = true;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;

        HandlePickUp();

        // If this part was snapped, tell its slot it is now vacant
        /*SlotReceiver occupiedSlot = GetCurrentSlot();
        if (occupiedSlot != null)
        {
            occupiedSlot.VacateSlot();

           var  snapPart = GetComponent<SnapPart>();
            if (snapPart != null)
                snapPart.CurrentSlot = null;
        } */

    }

    private void Release()
    {
        isHeld = false;
        HideAllMatchingGhosts();
        TrySnap();
    }

    // ── SNAP LOGIC ──────────────────────────────────────────────

    private void TrySnap()
    {
        SlotReceiver nearestSlot = null;
        float minDist = Mathf.Infinity;

        foreach (SlotReceiver slot in allSlots)
        {
            if (slot.acceptedPartTag != partTag || slot.isOccupied) continue;

            float dist = Vector3.Distance(transform.position, slot.transform.position);
            if (dist < minDist && dist < snapDistance)
            {
                minDist = dist;
                nearestSlot = slot;
            }
        }

        if (nearestSlot != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            nearestSlot.OccupySlot(gameObject);

            // Do NOT disable the script anymore — part can be picked up again
        }
        else
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearDamping = 5f;
        }
    }

    // ── GHOST HELPERS ───────────────────────────────────────────

    private void ShowAllMatchingGhosts()
    {
        foreach (SlotReceiver slot in allSlots)
            if (slot.acceptedPartTag == partTag && !slot.isOccupied)
                slot.ShowGhost();
    }

    private void HideAllMatchingGhosts()
    {
        foreach (SlotReceiver slot in allSlots)
            if (slot.acceptedPartTag == partTag)
                slot.HideGhost();
    }
}