using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class FirstPersonMovement : MonoBehaviourPunCallbacks
{
    public float speed = 10;
    private Vector3 currentVelocity = Vector3.zero;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 20;
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody rigidbody;
    PhotonView photonView;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
    }
void Start()
{
    if (!photonView.IsMine)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
}

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        bool runningPressed = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        IsRunning = canRun && runningPressed;

        float targetSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            float x = 0, y = 0;
            if (Keyboard.current.aKey.isPressed) x -= 1;
            if (Keyboard.current.dKey.isPressed) x += 1;
            if (Keyboard.current.sKey.isPressed) y -= 1;
            if (Keyboard.current.wKey.isPressed) y += 1;
            moveInput = new Vector2(x, y).normalized;
        }

        Vector3 targetVelocity = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y) * targetSpeed;
        Vector3 currentFlat = new Vector3(rigidbody.linearVelocity.x, 0, rigidbody.linearVelocity.z);
        Vector3 smoothedVelocity = Vector3.SmoothDamp(currentFlat, targetVelocity, ref currentVelocity, 0.1f);

        rigidbody.linearVelocity = new Vector3(smoothedVelocity.x, rigidbody.linearVelocity.y, smoothedVelocity.z);
    }
}
