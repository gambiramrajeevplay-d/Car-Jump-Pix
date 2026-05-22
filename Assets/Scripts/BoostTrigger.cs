using UnityEngine;

public class BoostTrigger : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Jump Settings")]
    public float forwardForceMultiplier = 25f;
    public float jumpHeight = 8f;

    [Header("Ground Check")]
    public float groundDistance = 2.2f;
    public LayerMask groundLayer;

    [Header("Air Physics")]
    public float extraGravity = 90f;

    [Header("Audio")]
    public AudioClip boostCollectedSound;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //MANUAL JUMP
        //if ((Input.GetKeyDown(KeyCode.Space) ||
        //    Input.GetKeyDown(KeyCode.Joystick1Button0))
        //    && IsGrounded())
        //{
        //    Jump(transform.forward);
        //}

        AirControl();
    }

    bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            groundDistance,
            groundLayer
        );
    }

    void AirControl()
    {
        if (!IsGrounded())
        {
            Vector3 vel = rb.velocity;

            // EXTRA GRAVITY
            vel.y -= extraGravity * Time.deltaTime;

            rb.velocity = vel;

            // REMOVE AIR SHAKE
            Vector3 angVel = rb.angularVelocity;
            angVel.x = 0f;
            angVel.z = 0f;
            rb.angularVelocity = angVel;
        }
    }

    public void Jump(Vector3 jumpDirection, float customJumpHeight)
    {
        if (rb == null)
            return;

        Vector3 velocity = rb.velocity;

        rb.angularVelocity = Vector3.zero;

        // SOUND
        if (boostCollectedSound)
        {
            GameObject audioObj =
                new GameObject("JumpSound");

            audioObj.transform.position =
                transform.position;

            AudioSource source =
                audioObj.AddComponent<AudioSource>();

            source.clip = boostCollectedSound;
            source.spatialBlend = 1f;
            source.Play();

            Destroy(
                audioObj,
                boostCollectedSound.length
            );
        }

        // REMOVE FALL SPEED
        if (velocity.y < 0f)
            velocity.y = 0f;

        // USE CUSTOM HEIGHT
        velocity.y = customJumpHeight;

        // DIRECTION
        jumpDirection.y = 0f;
        jumpDirection.Normalize();

        velocity +=
            jumpDirection *
            forwardForceMultiplier;

        rb.velocity = velocity;
    }
}