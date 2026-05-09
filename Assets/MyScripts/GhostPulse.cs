using UnityEngine;

public class GhostPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    private Vector3 originalScale;

    private void OnEnable()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Sine wave pulsing scale
        float pulse = Mathf.Sin(Time.time * pulseSpeed);
        float scaleFactor = Mathf.Lerp(minScale, maxScale, (pulse + 1f) / 2f);
        transform.localScale = originalScale * scaleFactor;
    }

    private void OnDisable()
    {
        // Reset scale when hidden
        transform.localScale = originalScale;
    }
}