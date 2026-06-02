using UnityEngine;
using UnityEngine.EventSystems;
using Script;
[RequireComponent(typeof(Rigidbody))]
public class CubeController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 14f;
    public float reverseSpeed = 7f;
    public float acceleration = 10f;

    [Header("Turning")]
    public float rotationSpeed = 170f;
    public float turnSmoothness = 7f;
    public float turnMultiplier = 1.3f;

    [Header("Stability")]
    public float sidewaysFriction = 3f;
    public float steeringSmoothness = 10f;

    [Header("Grip")]
    [Range(0f, 1f)]
    public float tireGrip = 0.92f;

    [Header("Ground Check")]
    public float groundDistance = 2.2f;
    public LayerMask groundLayer;

    [Header("Jump Arc")]
    [Tooltip("Multiplies Physics.gravity while airborne. 3-4 = snappy arc, no extra gravity component needed")]
    public float gravityScale = 3.5f;

    private Rigidbody rb;
    private float smoothTurnInput;
    private float currentTurn;
    private float fixedDelta;

    private const int GROUND_CHECK_INTERVAL = 2;
    private int groundCheckTimer = 0;

    public bool IsGrounded { get; private set; }

    private bool jumpPending = false;
    private Vector3 jumpVelocity = Vector3.zero;

    // Suppress Move() for N frames after jump so it doesn't fight the launch velocity
    private int suppressMoveFrames = 0;
    private const int JUMP_SUPPRESS_FRAMES = 4;

    private GameObject leftButton;
    private GameObject rightButton;

    [Header("Skid Marks")]
    public TrailRenderer leftSkid;
    public TrailRenderer rightSkid;

    [Tooltip("How much turning is required before skids appear")]
    public float skidTurnThreshold = 0.6f;

    [Tooltip("Minimum speed required for skids")]
    public float skidSpeedThreshold = 5f;

    public static float MobileHorizontalInput = 0f;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
        rb.angularDrag = 5f;
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);

        // Use Unity's built-in gravity scaling via rb — cleaner than AddForce every frame
        rb.useGravity = false;

        fixedDelta = Time.fixedDeltaTime;
        leftButton = GameObject.FindGameObjectWithTag("LeftButton");
        rightButton = GameObject.FindGameObjectWithTag("RightButton");

        bool isTV = AndroidTV.IsAndroidOrFireTv();

        if (leftButton != null)
            leftButton.SetActive(!isTV);

        if (rightButton != null)
            rightButton.SetActive(!isTV);

        if (!isTV)
        {
            SetupMobileButtons();
        }
    }

    private void Update()
    {
        float targetTurn;

        if (AndroidTV.IsAndroidOrFireTv())
        {
            targetTurn = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            targetTurn = MobileHorizontalInput;
            //targetTurn = Input.GetAxisRaw("Horizontal");
        }

        smoothTurnInput = Mathf.Lerp(
            smoothTurnInput,
            targetTurn,
            steeringSmoothness * Time.deltaTime
        );
    }
    private void SetupMobileButtons()
    {
        if (leftButton != null)
        {
            EventTrigger trigger =
                leftButton.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = leftButton.AddComponent<EventTrigger>();

            AddTrigger(trigger,
                EventTriggerType.PointerDown,
                () => MobileHorizontalInput = -1f);

            AddTrigger(trigger,
                EventTriggerType.PointerUp,
                () => MobileHorizontalInput = 0f);
        }

        if (rightButton != null)
        {
            EventTrigger trigger =
                rightButton.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = rightButton.AddComponent<EventTrigger>();

            AddTrigger(trigger,
                EventTriggerType.PointerDown,
                () => MobileHorizontalInput = 1f);

            AddTrigger(trigger,
                EventTriggerType.PointerUp,
                () => MobileHorizontalInput = 0f);
        }
    }

    private void AddTrigger(
        EventTrigger trigger,
        EventTriggerType type,
        UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry =
            new EventTrigger.Entry();

        entry.eventID = type;

        entry.callback.AddListener(
            (data) => action());

        trigger.triggers.Add(entry);
    }
    private void FixedUpdate()
    {
        // Throttled ground check
        groundCheckTimer++;
        if (groundCheckTimer >= GROUND_CHECK_INTERVAL)
        {
            groundCheckTimer = 0;
            IsGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                groundDistance,
                groundLayer
            );
        }

        // Always apply scaled gravity ourselves (replaces rb.useGravity)
        // gravityScale controls arc tightness without any extra AddForce noise
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        if (suppressMoveFrames > 0)
            suppressMoveFrames--;

        if (jumpPending)
        {
            rb.velocity = jumpVelocity;
            jumpPending = false;
            // Suppress Move() for a few frames so launch velocity isn't
            // immediately overwritten by the movement lerp
            suppressMoveFrames = JUMP_SUPPRESS_FRAMES;
        }
        else if (IsGrounded && suppressMoveFrames == 0)
        {
            Move();
            StabilizeVelocity();
        }

        Rotate();
        UpdateSkidMarks();
    }

    public void RequestJump(Vector3 velocity)
    {
        jumpVelocity = velocity;
        jumpPending = true;
    }

    private void Move()
    {
        Vector3 targetVelocity = transform.forward * moveSpeed;
        float lerpT = acceleration * fixedDelta;

        Vector3 velocity = rb.velocity;
        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, lerpT);
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, lerpT);
        rb.velocity = velocity;
    }

    private void Rotate()
    {
        if (Mathf.Abs(smoothTurnInput) < 0.01f)
            return;

        float turnMultiplierInAir = IsGrounded ? 1f : 0.4f;
        float speedPercent = Mathf.Clamp01(rb.velocity.magnitude / moveSpeed);
        float turnStrength = Mathf.Lerp(0.7f, 1f, speedPercent);

        float targetTurn =
            smoothTurnInput *
            rotationSpeed *
            turnMultiplier *
            turnStrength *
            turnMultiplierInAir;

        currentTurn = Mathf.Lerp(currentTurn, targetTurn, turnSmoothness * fixedDelta);

        rb.MoveRotation(
            rb.rotation * Quaternion.Euler(0f, currentTurn * fixedDelta, 0f)
        );
    }

    private void StabilizeVelocity()
    {
        Vector3 right = transform.right;
        float sidewaysSpeed = Vector3.Dot(rb.velocity, right);

        Vector3 velocity = rb.velocity;
        velocity -= right * (sidewaysSpeed * (1f - tireGrip));
        rb.velocity = velocity;
    }
    private void UpdateSkidMarks()
    {
        if (leftSkid == null || rightSkid == null)
            return;

        bool shouldSkid =
            IsGrounded &&
            Mathf.Abs(smoothTurnInput) > skidTurnThreshold &&
            rb.velocity.magnitude > skidSpeedThreshold;

        leftSkid.emitting = shouldSkid;
        rightSkid.emitting = shouldSkid;
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * groundDistance
        );
    }
#endif
}
