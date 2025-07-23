using UnityEngine;
using Photon.Pun;

[ExecuteInEditMode]
public class GroundCheck : MonoBehaviourPun
{
    [Tooltip("Maximum distance from the ground.")]
    public float distanceThreshold = .15f;

    [Tooltip("Whether this transform is grounded now.")]
    public bool isGrounded = true;

    public event System.Action Grounded;

    const float OriginOffset = .001f;
    Vector3 RaycastOrigin => transform.position + Vector3.up * OriginOffset;
    float RaycastDistance => distanceThreshold + OriginOffset;

    void LateUpdate()
    {
        if (!photonView.IsMine) return;

        bool isGroundedNow = Physics.Raycast(RaycastOrigin, Vector3.down, RaycastDistance);

        if (isGroundedNow && !isGrounded)
        {
            Grounded?.Invoke();
        }

        isGrounded = isGroundedNow;
    }

    void OnDrawGizmosSelected()
    {
        Debug.DrawLine(RaycastOrigin, RaycastOrigin + Vector3.down * RaycastDistance, isGrounded ? Color.white : Color.red);
    }
}
