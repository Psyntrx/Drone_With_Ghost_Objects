using UnityEngine;

public class DroneManager : MonoBehaviour
{
    [Header("Parts")]
    public int totalPartsRequired = 5;

    [Header("Propellers")]
    public Transform propellerFL;
    public Transform propellerFR;
    public Transform propellerBL;
    public Transform propellerBR;
    public float propellerSpinSpeed = 720f;

    [Header("Hover")]
    public float hoverAmplitude = 0.05f;
    public float hoverSpeed = 1.5f;
    public float hoverRiseSpeed = 2f;
    public float hoverHeight = 1.5f;

    // Spin direction per slot — tick = clockwise, untick = counter-clockwise
    public bool propellerFL_Clockwise = false;
    public bool propellerFR_Clockwise = true;
    public bool propellerBL_Clockwise = false;
    public bool propellerBR_Clockwise = true;

    [Header("Fall and Return")]
    public float fallSpeed = 3f;          // How fast drone drops back down
    public float groundSnapDistance = 0.05f; // How close before it locks to ground

    [Header("Audio")]
    public AudioSource droneAudioSource;
    public AudioClip snapSound;
    public AudioClip droneHumSound;

    // Internal state
    private int partsPlaced = 0;
    private bool droneActive = false;
    private bool isFalling = false;
    private Vector3 groundPosition;       // The original position on the ground
    private Rigidbody rb;

    private void Start()
    {
        // Save the exact starting ground position
        groundPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void SpinPropeller(Transform propeller, bool clockwise)
    {
        if (propeller == null) return;
        float direction = clockwise ? 1f : -1f;
        propeller.Rotate(Vector3.forward, direction * propellerSpinSpeed * Time.deltaTime, Space.Self);
    }
    private void Update()
    {
        // ── HOVERING ──
        if (droneActive)
        {
            // Spin propellers
            SpinPropeller(propellerFL, propellerFL_Clockwise);
            SpinPropeller(propellerFR, propellerFR_Clockwise);
            SpinPropeller(propellerBL, propellerBL_Clockwise);
            SpinPropeller(propellerBR, propellerBR_Clockwise);

            // Hover bob
            float targetY = groundPosition.y + hoverHeight
                + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;

            transform.position = Vector3.Lerp(
                transform.position,
                new Vector3(transform.position.x, targetY, transform.position.z),
                Time.deltaTime * hoverRiseSpeed
            );
        }

        // ── FALLING BACK TO GROUND ──
        if (isFalling)
        {
            // Smoothly move drone back down to ground position
            transform.position = Vector3.MoveTowards(
                transform.position,
                groundPosition,
                fallSpeed * Time.deltaTime
            );

            // Slow down propellers gradually while falling
            propellerSpinSpeed = Mathf.Lerp(propellerSpinSpeed, 0f, Time.deltaTime * 2f);

            // Spin propellers while falling (slowing down)
            SpinPropeller(propellerFL, propellerFL_Clockwise);
            SpinPropeller(propellerFR, propellerFR_Clockwise);
            SpinPropeller(propellerBL, propellerBL_Clockwise);
            SpinPropeller(propellerBR, propellerBR_Clockwise);

            // Once close enough to ground — lock it in place
            if (Vector3.Distance(transform.position, groundPosition) < groundSnapDistance)
            {
                transform.position = groundPosition;
                isFalling = false;
                propellerSpinSpeed = 720f; // Reset spin speed for next time
                Debug.Log("Drone returned to ground position.");
            }
        }
    }

    // Called by SlotReceiver when a part is placed
    public void OnPartPlaced()
    {
        if (snapSound != null && droneAudioSource != null)
            droneAudioSource.PlayOneShot(snapSound);

        partsPlaced++;
        Debug.Log($"Parts placed: {partsPlaced} / {totalPartsRequired}");

        if (partsPlaced >= totalPartsRequired)
            ActivateDrone();
    }

    // Called by SlotReceiver when a part is removed
    public void OnPartRemoved()
    {
        partsPlaced = Mathf.Max(0, partsPlaced - 1);
        Debug.Log($"Part removed. Parts placed: {partsPlaced} / {totalPartsRequired}");

        if (droneActive)
            DeactivateDrone();
    }

    private void ActivateDrone()
    {
        droneActive = true;
        isFalling = false;

        // Lock rigidbody — hover is script driven not physics driven
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Save current ground position each time drone activates
        groundPosition = transform.position;

        if (droneAudioSource != null && droneHumSound != null)
        {
            droneAudioSource.clip = droneHumSound;
            droneAudioSource.loop = true;
            droneAudioSource.Play();
        }

        Debug.Log("Drone fully assembled! Activating...");
    }

    private void DeactivateDrone()
    {
        droneActive = false;
        isFalling = true;   // Trigger the fall back to ground

        // Keep kinematic so script controls the fall, not physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (droneAudioSource != null)
            droneAudioSource.Stop();

        Debug.Log("Drone incomplete — returning to ground.");
    }

    public void ResetDrone()
    {
        partsPlaced = 0;
        droneActive = false;
        isFalling = false;
        propellerSpinSpeed = 720f;
        transform.position = groundPosition;

        if (droneAudioSource != null)
            droneAudioSource.Stop();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}