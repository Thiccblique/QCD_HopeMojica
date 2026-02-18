using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;

    [Header("Jump")]
    public float jumpForce = 20f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private float lastMoveDirection = 1f;
    private bool isGrounded;
    private int jumpsRemaining;
    public int maxJumps = 2;
    private float jumpTakeOffCooldown = 0.20f;
    private float jumpTakeOffCooldownReset = 0.20f;

    [Header("Wall Jump")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;

    public float wallJumpForceX = 16f;
    public float wallJumpForceY = 14f;
    public float wallSlideSpeed = 1f;
    private float wallJumpLockTime = 0.2f;
    private float wallJumpLockCounter;

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

    [Header("Respawn")]
    private Vector3 spawnPosition;

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
        jumpsRemaining = maxJumps;
        spawnPosition = transform.position;
    }

    void Update()
    {
        Debug.Log(jumpsRemaining.ToString());
        Debug.Log(maxJumps.ToString());

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
        RaycastHit2D wallHitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D wallHitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);

        isTouchingWall = wallHitRight || wallHitLeft;

        if (wallHitRight)
            wallDirection = 1f;
        else if (wallHitLeft)
            wallDirection = -1f;

        // Wall slide
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && wallJumpLockCounter <= 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            // WALL JUMP
            if (isTouchingWall && !isGrounded)
            {
                WallJump();
                canDash = true;
                jumpsRemaining = maxJumps - 1; // allow one air jump after wall jump
                return;
            }

            // NORMAL / DOUBLE JUMP
            if (jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
                Debug.Log("Jump subtracted!");
            }
        }

        // Reset jumps when grounded
        if (isGrounded)
        {
            jumpTakeOffCooldown -= Time.deltaTime;
            if (jumpsRemaining != 2 && jumpTakeOffCooldown <= 0)
            {
                Debug.Log("Is Grounded!");
                jumpsRemaining = maxJumps;
                jumpTakeOffCooldown = jumpTakeOffCooldownReset;
            }
        }

        if (isTouchingWall)
        {
            jumpsRemaining = 1;
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

        // Respawn
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
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

        if (wallJumpLockCounter > 0)
        {
            wallJumpLockCounter -= Time.fixedDeltaTime;
            return; // Ignore player input during lock
        }

        if (isWallSliding && wallJumpLockCounter <= 0)
        {
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
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
        rb.gravityScale = 10f; // Set back to your normal gravity
    }
    void WallJump()
    {
        wallJumpLockCounter = wallJumpLockTime;

        rb.linearVelocity = new Vector2(
            -wallDirection * wallJumpForceX,
            wallJumpForceY
        );
    }

    public void Respawn()
    {
        transform.position = spawnPosition;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        rb.gravityScale = 10f;
        jumpsRemaining = maxJumps;
        wallJumpLockCounter = 0f;
        isWallSliding = false;
        isWallJumping = false;
    }
}
