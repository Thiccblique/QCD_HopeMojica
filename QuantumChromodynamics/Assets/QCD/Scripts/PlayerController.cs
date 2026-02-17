using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;

    [Header("Jump")]
    public float jumpForce = 20f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private float lastMoveDirection = 1f;
    private bool isGrounded;
    private bool hasDoubleJumped;
    private bool hasJumped;

    [Header("Wall Jump")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;

    public float wallJumpForceX = 10f;
    public float wallJumpForceY = 14f;
    public float wallSlideSpeed = 2f;

    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallDirection;
    public LayerMask wallLayer;

    [Header("Dash")]
    public float dashForce = 30f;
    public float dashDuration = 0.35f;
    public float dashCooldown = 1f;

    private bool isDashing;
    private bool canDash = true;
    private float dashTime;
    private float dashCooldownTimer;

    [Header("Color Probes")]
    public ProbeManager probeManager;
    public GameObject probe1;
    public GameObject probe2;
    public GameObject probe3;
    private ProbeBehavior pB1;
    private ProbeBehavior pB2;
    private ProbeBehavior pB3;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        probeManager = transform.Find("ProbePosition").GetComponent<ProbeManager>();
        probe1 = probeManager.probe1;
        probe2 = probeManager.probe2;
        probe3 = probeManager.probe3;
        pB1 = probe1.GetComponent<ProbeBehavior>();
        pB2 = probe2.GetComponent<ProbeBehavior>();
        pB3 = probe3.GetComponent<ProbeBehavior>();
        hasDoubleJumped = false;
        hasJumped = false;
    }

    void Update()
    {

        moveInput = Input.GetAxisRaw("Horizontal");

        // Track last movement direction
        if (moveInput != 0)
        {
            lastMoveDirection = Mathf.Sign(moveInput);
        }

        // Check if touching ground
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        isTouchingWall = Physics2D.OverlapCircle(
            wallCheck.position,
            wallCheckDistance,
            wallLayer
        );

        // Wall direction
        wallDirection = transform.localScale.x;

        // Wall slide
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        // Wall jump
        if (isWallSliding && Input.GetButtonDown("Jump"))
        {
            WallJump();
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            hasJumped = true;
        }

        if(Input.GetButtonDown("Jump") && hasJumped && !hasDoubleJumped && !isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            hasDoubleJumped = true;
        }

        if(hasDoubleJumped && isGrounded)
        {
            hasDoubleJumped = false;
            hasJumped = false;
        }

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f && isGrounded)
            {
                canDash = true;
            }
        }

        // Start dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartDash();
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            bool newState = !pB1.lightObject.activeSelf;
            pB1.lightObject.SetActive(newState);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            bool newState = !pB2.lightObject.activeSelf;
            pB2.lightObject.SetActive(newState);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            bool newState = !pB3.lightObject.activeSelf;
            pB3.lightObject.SetActive(newState);
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
            return;

        // WALL SLIDE
        if (isWallSliding)
        {
            canDash = true;

            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -1f, 1f),
                -wallSlideSpeed
            );
        }

        // NORMAL MOVEMENT
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTime = dashDuration;
        dashCooldownTimer = dashCooldown;

        rb.gravityScale = 0f;

        float dashDirection = moveInput != 0 ? Mathf.Sign(moveInput) : lastMoveDirection;
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);

        Invoke(nameof(EndDash), dashDuration);
    }
    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 4f; // Set back to your normal gravity
    }
    void WallJump()
    {
        isWallJumping = true;

        rb.linearVelocity = new Vector2(
            -wallDirection * wallJumpForceX,
            wallJumpForceY
        );

        Invoke(nameof(StopWallJump), 0.2f);
    }

    void StopWallJump()
    {
        isWallJumping = false;
    }
}
