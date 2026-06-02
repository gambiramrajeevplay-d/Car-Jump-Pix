using UnityEngine;

public class BoostTrigger : MonoBehaviour
{
    [Header("Jump Settings")]
    public float forwardForceMultiplier = 18f;
    public float jumpHeight = 5f;
    [Tooltip("1 = instant snap to boost direction, 0.5 = softer blend")]
    public float forwardBlend = 0.85f;

    [Header("Audio")]
    public AudioClip boostCollectedSound;

    private CubeController cubeController;
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasJumped;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cubeController = GetComponent<CubeController>();

        if (rb == null || cubeController == null)
        {
            Debug.LogError("BoostTrigger: Rigidbody or CubeController missing!");
            enabled = false;
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.clip = boostCollectedSound;
        audioSource.priority = 120;
        audioSource.volume = 1f;
    }

    private void FixedUpdate()
    {
        if (cubeController.IsGrounded)
            hasJumped = false;
    }

    public void Jump(Vector3 jumpDirection, float customJumpHeight)
    {
        if (hasJumped) return;
        hasJumped = true;

        if (boostCollectedSound != null)
            audioSource.Play();

        jumpDirection.y = 0f;
        if (jumpDirection.sqrMagnitude > 0.001f)
            jumpDirection.Normalize();

        Vector3 current = rb.velocity;

        // Blend forward into boost direction, set Y to jump height cleanly
        Vector3 targetForward = jumpDirection * forwardForceMultiplier;
        Vector3 jumpVel = new Vector3(
            Mathf.Lerp(current.x, targetForward.x, forwardBlend),
            customJumpHeight,
            Mathf.Lerp(current.z, targetForward.z, forwardBlend)
        );

        cubeController.RequestJump(jumpVel);
    }
}
