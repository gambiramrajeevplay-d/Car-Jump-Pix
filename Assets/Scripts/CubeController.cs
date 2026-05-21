using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 14f;
    public float reverseSpeed = 7f;

    [Tooltip("How fast speed changes")]
    public float acceleration = 10f;

    [Header("Turning")]
    public float rotationSpeed = 170f;

    [Tooltip("Higher = smoother steering")]
    public float turnSmoothness = 7f;

    [Tooltip("Extra turn boost")]
    public float turnMultiplier = 1.3f;

    [Header("Stability")]
    [Tooltip("Higher = less drifting")]
    public float sidewaysFriction = 3f;

    [Tooltip("Smooth steering input")]
    public float steeringSmoothness = 10f;

    private Rigidbody rb;

    private float moveInput;
    private float turnInput;

    private Vector3 currentVelocity;

    private float currentTurn;

    [Header("Grip")]
    [Range(0f, 1f)]
    public float tireGrip = 0.92f;

    // SMOOTH INPUT
    private float smoothMoveInput;
    private float smoothTurnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.angularDrag = 5f;

        // ADD THIS
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
    }
    void Update()
    {
        // AUTO MOVE FORWARD
        moveInput = 1f;

        // ONLY STEERING INPUT
        float targetTurn =
            Input.GetAxisRaw("Horizontal");

        // SMOOTH STEERING
        smoothTurnInput = Mathf.Lerp(
            smoothTurnInput,
            targetTurn,
            steeringSmoothness * Time.deltaTime
        );

        turnInput = smoothTurnInput;

        // OPTIONAL REVERSE
        //if (Input.GetKey(KeyCode.S))
        //{
        //    moveInput = -1f;
        //}
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
        StabilizeVelocity();
    }
    void Move()
    {
        float currentSpeed =
            moveInput < 0 ?
            reverseSpeed :
            moveSpeed;

        // TARGET FORWARD VELOCITY
        Vector3 targetVelocity =
            transform.forward *
            moveInput *
            currentSpeed;

        // KEEP GRAVITY
        targetVelocity.y = rb.velocity.y;

        // EXTRA SMOOTH AUTO MOVEMENT
        rb.velocity = Vector3.Lerp(
            rb.velocity,
            targetVelocity,
            acceleration * 0.5f * Time.fixedDeltaTime
        );
    }

    void Rotate()
    {
        // NO TURNING WHEN STOPPED
        if (Mathf.Abs(moveInput) < 0.05f)
            return;

        // FIX REVERSE STEERING
        float direction =
            moveInput > 0 ? 1f : -1f;

        // SPEED BASED TURNING
        float speedPercent =
            Mathf.Clamp01(
                rb.velocity.magnitude / moveSpeed
            );

        // SMOOTHER TURN CURVE
        float turnStrength =
            Mathf.Lerp(0.7f, 1f, speedPercent);

        // TARGET TURN
        float targetTurn =
            turnInput *
            direction *
            rotationSpeed *
            turnMultiplier *
            turnStrength;

        // SMOOTH TURNING
        currentTurn = Mathf.Lerp(
            currentTurn,
            targetTurn,
            turnSmoothness * Time.fixedDeltaTime
        );

        Quaternion turnRotation =
            Quaternion.Euler(
                0f,
                currentTurn * Time.fixedDeltaTime,
                0f
            );

        // SMOOTH ROTATION
        rb.MoveRotation(
            Quaternion.Lerp(
                rb.rotation,
                rb.rotation * turnRotation,
                0.9f
            )
        );

        // REMOVE SHAKE
        Vector3 angVel = rb.angularVelocity;
        angVel.x = 0f;
        angVel.z = 0f;
        rb.angularVelocity = angVel;
    }
    void StabilizeVelocity()
    {
        Vector3 localVelocity =
            transform.InverseTransformDirection(rb.velocity);

        // REDUCE SIDEWAYS SLIDE
        localVelocity.x *= tireGrip;

        rb.velocity =
            transform.TransformDirection(localVelocity);
    }

}