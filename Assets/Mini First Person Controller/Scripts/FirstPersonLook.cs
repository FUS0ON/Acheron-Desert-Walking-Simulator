using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class FirstPersonLook : MonoBehaviourPun, IPunObservable
{
    [SerializeField] Transform character;
    public float sensitivity = 0.2f;
    public float smoothing = 1.5f;

    Vector2 frameVelocity;
    float yaw;
    float pitch;

    float networkYaw;

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        if (!photonView.IsMine)
        {
            if (GetComponentInChildren<Camera>() != null)
                GetComponentInChildren<Camera>().gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
            frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);

            yaw += frameVelocity.x;
            pitch -= frameVelocity.y;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            character.localRotation = Quaternion.Euler(0, yaw, 0);
            transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
        else
        {
            character.localRotation = Quaternion.Lerp(character.localRotation, Quaternion.Euler(0, networkYaw, 0), Time.deltaTime * 10f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(yaw);
        }
        else
        {
            networkYaw = (float)stream.ReceiveNext();
        }
    }
}
