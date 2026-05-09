using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float fastSpeed = 15f;     // Hold Shift to move faster
    public float lookSpeed = 2f;

    private bool isLooking = false;
    private Vector2 lastMousePos;

    private void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        // ── RIGHT CLICK TO LOOK AROUND ──
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isLooking = true;
            lastMousePos = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
            isLooking = false;

        if (isLooking)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 delta = currentMousePos - lastMousePos;
            lastMousePos = currentMousePos;

            transform.eulerAngles += new Vector3(-delta.y * lookSpeed * Time.deltaTime * 10f,
                                                  delta.x * lookSpeed * Time.deltaTime * 10f,
                                                  0f);
        }

        // ── WASD TO MOVE ──
        float speed = Keyboard.current.leftShiftKey.isPressed ? fastSpeed : moveSpeed;
        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) move += transform.forward;
        if (Keyboard.current.sKey.isPressed) move -= transform.forward;
        if (Keyboard.current.aKey.isPressed) move -= transform.right;
        if (Keyboard.current.dKey.isPressed) move += transform.right;
        if (Keyboard.current.eKey.isPressed) move += transform.up;
        if (Keyboard.current.qKey.isPressed) move -= transform.up;

        transform.position += move * speed * Time.deltaTime;
    }
}