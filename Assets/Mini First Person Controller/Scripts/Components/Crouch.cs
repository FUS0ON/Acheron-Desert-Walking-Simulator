using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Crouch : MonoBehaviourPunCallbacks
{
    PhotonView photonView;
    public KeyCode key = KeyCode.LeftControl;

    [Header("Slow Movement")]
    [Tooltip("Movement to slow down when crouched.")]
    public FirstPersonMovement movement;
    [Tooltip("Movement speed when crouched.")]
    public float movementSpeed = 2;

    [Header("Low Head")]
    [Tooltip("Head to lower when crouched.")]
    public Transform headToLower;
    [HideInInspector]
    public float? defaultHeadYLocalPosition;
    public float crouchYHeadPosition = 1;
    
    [Tooltip("Collider to lower when crouched.")]
    public CapsuleCollider colliderToLower;
    [HideInInspector]
    public float? defaultColliderHeight;

    [Header("Smooth Crouch")]
    [Tooltip("Скорость анимации приседания (чем больше, тем быстрее).")]
    public float crouchSmoothSpeed = 8f;

    [Header("Ground Check")]
    [Tooltip("GroundCheck component to determine if the player is grounded.")]
    public GroundCheck groundCheck;

    float targetHeadY;
    float targetColliderHeight;
    float currentHeadY;
    float currentColliderHeight;

    public bool IsCrouched { get; private set; }
    public event System.Action CrouchStart, CrouchEnd;


    void Reset()
    {
        // Try to get components.
        movement = GetComponentInParent<FirstPersonMovement>();
        headToLower = movement.GetComponentInChildren<Camera>().transform;
        colliderToLower = movement.GetComponentInChildren<CapsuleCollider>();
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        if (headToLower)
            currentHeadY = headToLower.localPosition.y;
        if (colliderToLower)
            currentColliderHeight = colliderToLower.height;
        photonView = GetComponent<PhotonView>();
    }

void Start()
{
    if (headToLower)
    {
        defaultHeadYLocalPosition = headToLower.localPosition.y;
        currentHeadY = defaultHeadYLocalPosition.Value;
        targetHeadY = currentHeadY;
    }

    if (colliderToLower)
    {
        defaultColliderHeight = colliderToLower.height;
        currentColliderHeight = defaultColliderHeight.Value;
        targetColliderHeight = currentColliderHeight;
    }
}

  void LateUpdate()
{
    if (!photonView.IsMine) return;

    bool crouchPressed = Keyboard.current?.leftCtrlKey?.isPressed ?? false;

    if (!crouchPressed && IsCrouched)
    {
        photonView.RPC("SetCrouchState", RpcTarget.AllBuffered, false);
    }
    else if (crouchPressed && !IsCrouched)
    {
        photonView.RPC("SetCrouchState", RpcTarget.AllBuffered, true);
    }

    AnimateCrouch();
}


    [PunRPC]
    void SetCrouchState(bool crouched)
    {
        if (headToLower && !defaultHeadYLocalPosition.HasValue)
            defaultHeadYLocalPosition = headToLower.localPosition.y;
        if (colliderToLower && !defaultColliderHeight.HasValue)
            defaultColliderHeight = colliderToLower.height;

        if (crouched)
        {
            targetHeadY = crouchYHeadPosition;
            float loweringAmount = defaultHeadYLocalPosition.HasValue
                ? defaultHeadYLocalPosition.Value - crouchYHeadPosition
                : defaultColliderHeight.Value * .5f;
            targetColliderHeight = Mathf.Max(defaultColliderHeight.Value - loweringAmount, 0);

            if (!IsCrouched)
            {
                IsCrouched = true;
                if (groundCheck == null || groundCheck.isGrounded)
                    SetSpeedOverrideActive(true);
                CrouchStart?.Invoke();
            }
        }
        else
        {
            targetHeadY = defaultHeadYLocalPosition.HasValue ? defaultHeadYLocalPosition.Value : targetHeadY;
            targetColliderHeight = defaultColliderHeight.HasValue ? defaultColliderHeight.Value : targetColliderHeight;

            if (IsCrouched)
            {
                IsCrouched = false;
                SetSpeedOverrideActive(false);
                CrouchEnd?.Invoke();
            }
        }

        if (headToLower)
        {
            currentHeadY = Mathf.Lerp(currentHeadY, targetHeadY, Time.deltaTime * crouchSmoothSpeed);
            headToLower.localPosition = new Vector3(headToLower.localPosition.x, currentHeadY, headToLower.localPosition.z);
        }
        if (colliderToLower)
        {
            currentColliderHeight = Mathf.Lerp(currentColliderHeight, targetColliderHeight, Time.deltaTime * crouchSmoothSpeed);
            colliderToLower.height = currentColliderHeight;
            colliderToLower.center = Vector3.up * colliderToLower.height * .5f;
        }
    }


    #region Speed override.
    void SetSpeedOverrideActive(bool state)
    {
        // Stop if there is no movement component.
        if(!movement)
        {
            return;
        }

        // Update SpeedOverride.
        if (state)
        {
            // Try to add the SpeedOverride to the movement component.
            if (!movement.speedOverrides.Contains(SpeedOverride))
            {
                movement.speedOverrides.Add(SpeedOverride);
            }
        }
        else
        {
            // Try to remove the SpeedOverride from the movement component.
            if (movement.speedOverrides.Contains(SpeedOverride))
            {
                movement.speedOverrides.Remove(SpeedOverride);
            }
        }
    }

    float SpeedOverride() => movementSpeed;
    #endregion

    void AnimateCrouch()
{
    if (headToLower)
    {
        currentHeadY = Mathf.Lerp(currentHeadY, targetHeadY, Time.deltaTime * crouchSmoothSpeed);
        headToLower.localPosition = new Vector3(headToLower.localPosition.x, currentHeadY, headToLower.localPosition.z);
    }

    if (colliderToLower)
    {
        currentColliderHeight = Mathf.Lerp(currentColliderHeight, targetColliderHeight, Time.deltaTime * crouchSmoothSpeed);
        colliderToLower.height = currentColliderHeight;
        colliderToLower.center = Vector3.up * colliderToLower.height * .5f;
    }
}

}
