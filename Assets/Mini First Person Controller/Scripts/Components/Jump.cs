using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Jump : MonoBehaviourPunCallbacks
{
    private Rigidbody rb;
    public float jumpStrength = 2;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    private GroundCheck groundCheck;

    void Reset()
    {
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (jumpPressed && (groundCheck == null || groundCheck.isGrounded))
        {
            rb.AddForce(Vector3.up * 100f * jumpStrength);
            Jumped?.Invoke();
        }
    }
}
