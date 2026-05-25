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

    private bool isGrounded;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody Missing!");
            return;
        }

        rb.maxAngularVelocity = 2f;
    }


    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundDistance,
            groundLayer
        );

        if (!isGrounded)
        {
            rb.AddForce(
                Vector3.down * extraGravity,
                ForceMode.Acceleration
            );
        }
    }



    //void AirControl()
    //{
    //    if (!IsGrounded())
    //    {
    //        Vector3 vel = rb.velocity;

    //        // EXTRA GRAVITY
    //        vel.y -= extraGravity * Time.deltaTime;

    //        rb.velocity = vel;

    //        // REMOVE AIR SHAKE
    //        Vector3 angVel = rb.angularVelocity;
    //        angVel.x = 0f;
    //        angVel.z = 0f;
    //        rb.angularVelocity = angVel;
    //    }
    //}

    public void Jump(Vector3 jumpDirection, float customJumpHeight)
    {
        if (rb == null)
            return;

        Vector3 velocity = rb.velocity;

        // PLAY SOUND
        if (boostCollectedSound)
        {
            AudioSource.PlayClipAtPoint(
                boostCollectedSound,
                transform.position
            );
        }

        // RESET FALL SPEED
        if (velocity.y < 0f)
            velocity.y = 0f;

        // JUMP HEIGHT
        velocity.y = customJumpHeight;

        // FORWARD BOOST
        jumpDirection.y = 0f;
        jumpDirection = jumpDirection.normalized;

        velocity +=
            jumpDirection *
            forwardForceMultiplier;

        rb.velocity = velocity;
    }
}