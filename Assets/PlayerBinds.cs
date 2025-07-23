using UnityEngine;
using UnityEngine.InputSystem; 
using Photon.Pun;

public class PlayerBinds : MonoBehaviour
{
    public bool isFly = false;
    public float flySpeed = 8f;
    private bool flyTogglePressed = false;
    private Rigidbody rb;
    private Animator animator;

    private PhotonView photonView;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            transform.position = new Vector3(4f, 1.2f, 4f);
        }
        if (Keyboard.current != null &&
            Keyboard.current.zKey.isPressed &&
            Keyboard.current.oKey.isPressed &&
            Keyboard.current.vKey.isPressed)
        {
            if (!flyTogglePressed)
            {
                isFly = !isFly;
                flyTogglePressed = true;
            }
        }
        else
        {
            flyTogglePressed = false;
        }
        if (rb != null)
        {
            rb.useGravity = !isFly;
            if (isFly)
                rb.linearVelocity = Vector3.zero;
        }
        if (isFly)
        {
            Vector3 move = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) move += transform.forward;
            if (Keyboard.current.sKey.isPressed) move -= transform.forward;
            if (Keyboard.current.aKey.isPressed) move -= transform.right;
            if (Keyboard.current.dKey.isPressed) move += transform.right;
            if (Keyboard.current.spaceKey.isPressed) move += Vector3.up;
            if (Keyboard.current.leftCtrlKey.isPressed) move += Vector3.down;

            if (move != Vector3.zero)
            {
                transform.position += move.normalized * flySpeed * Time.deltaTime;
            }
        }
    }
}
