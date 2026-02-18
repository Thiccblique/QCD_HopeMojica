using UnityEngine;

/// <summary>
/// Handles player movement, jumping, wall mechanics, dashing, and animations.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;

    [Header("Jump")]
    public float jumpForce = 20f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    public int maxJumps = 2;

    private Rigidbody2D rb;
    private float moveInput;
    private float lastMoveDirection = 1f;
    private bool isGrounded;
    private int jumpsRemaining;
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
    private float wallDirection;
    public LayerMask wallLayer;

    [Header("Dash")]
    public float dashForce = 30f;
    public float dashDuration = 0.35f;
    public float dashCooldown = 1f;

    private bool isDashing;
    private bool canDash = true;
    private float dashCooldownTimer;

    [Header("Color Probes")]
    private ProbeManager probeManager;
    private GameObject probe1;
    private GameObject probe2;
    private GameObject probe3;
    private ProbeBehavior pB1;
    private ProbeBehavior pB2;
    private ProbeBehavior pB3;

    [Header("Respawn")]
    private Vector3 spawnPosition;

    [Header("Animations")]
    public Animator animator;
    public Transform spriteChild;

    private bool facingRight = true;
    private string currentAnimationState = "";

    void Awake()
    {
        // Initialize references
        rb = GetComponent<Rigidbody2D>();
        probeManager = transform.Find("ProbePosition").GetComponent<ProbeManager>();
        probe1 = probeManager.probe1;
        probe2 = probeManager.probe2;
        probe3 = probeManager.probe3;
        pB1 = probe1.GetComponent<ProbeBehavior>();
        pB2 = probe2.GetComponent<ProbeBehavior>();
        pB3 = probe3.GetComponent<ProbeBehavior>();
        
        // Initialize jump state
        jumpsRemaining = maxJumps;
        
        // Set spawn position for respawn
        spawnPosition = transform.position;
    }

    void Update()
    {
        // Get movement input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Update facing direction and flip sprite
        if (moveInput != 0)
        {
            lastMoveDirection = Mathf.Sign(moveInput);
            FlipCharacter();
        }

        // Check ground and wall contact
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        DetectWall();

        // Handle wall slide
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && wallJumpLockCounter <= 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        // Handle jumping
        if (Input.GetButtonDown("Jump"))
        {
            if (isTouchingWall && !isGrounded)
            {
                WallJump();
                canDash = true;
                jumpsRemaining = maxJumps - 1;
            }
            else if (jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
            }
        }

        // Reset jumps when grounded
        if (isGrounded)
        {
            jumpTakeOffCooldown -= Time.deltaTime;
            if (jumpsRemaining != maxJumps && jumpTakeOffCooldown <= 0)
            {
                jumpsRemaining = maxJumps;
                jumpTakeOffCooldown = jumpTakeOffCooldownReset;
            }
        }

        // Reduce jumps when touching wall
        if (isTouchingWall)
        {
            jumpsRemaining = 1;
        }

        // Handle dash cooldown
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f && isGrounded)
            {
                canDash = true;
            }
        }

        // Handle dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartDash();
        }

        // Handle respawn
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }

        // Handle color probe toggles
        ToggleProbe(KeyCode.Alpha1, pB1);
        ToggleProbe(KeyCode.Alpha2, pB2);
        ToggleProbe(KeyCode.Alpha3, pB3);

        // Update animations based on current state
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Don't apply movement during dash
        if (isDashing)
            return;

        // Don't allow input during wall jump lock
        if (wallJumpLockCounter > 0)
        {
            wallJumpLockCounter -= Time.fixedDeltaTime;
            return;
        }

        // Apply wall slide velocity
        if (isWallSliding && wallJumpLockCounter <= 0)
        {
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
            return;
        }

        // Apply normal movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashCooldownTimer = dashCooldown;

        // Disable gravity during dash
        rb.gravityScale = 0f;

        // Apply dash force in facing direction
        float dashDirection = moveInput != 0 ? Mathf.Sign(moveInput) : lastMoveDirection;
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);

        Invoke(nameof(EndDash), dashDuration);
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 10f;
    }
    void WallJump()
    {
        // Lock input during wall jump and apply force away from wall
        wallJumpLockCounter = wallJumpLockTime;

        rb.linearVelocity = new Vector2(
            -wallDirection * wallJumpForceX,
            wallJumpForceY
        );
    }

    void DetectWall()
    {
        // Raycast both directions to detect walls
        RaycastHit2D wallHitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D wallHitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);

        // Determine if touching wall and which direction
        isTouchingWall = wallHitRight || wallHitLeft;

        if (wallHitRight)
            wallDirection = 1f;
        else if (wallHitLeft)
            wallDirection = -1f;
    }

    public void Respawn()
    {
        // Reset position and velocity
        transform.position = spawnPosition;
        rb.linearVelocity = Vector2.zero;
        
        // Reset movement states
        isDashing = false;
        rb.gravityScale = 10f;
        jumpsRemaining = maxJumps;
        wallJumpLockCounter = 0f;
        isWallSliding = false;
    }

    void ToggleProbe(KeyCode key, ProbeBehavior probe)
    {
        // Toggle probe light on/off
        if (Input.GetKeyDown(key))
        {
            bool newState = !probe.lightObject.activeSelf;
            probe.lightObject.SetActive(newState);
        }
    }

    void FlipCharacter()
    {
        // Flip sprite based on movement direction
        if (spriteChild == null) return;

        if (moveInput > 0 && !facingRight)
        {
            facingRight = true;
            spriteChild.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (moveInput < 0 && facingRight)
        {
            facingRight = false;
            spriteChild.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    void FlipCharacterOpposite()
    {
        // Flip opposite direction for wall (face away from wall)
        if (spriteChild == null) return;

        if (moveInput > 0)
            spriteChild.localScale = new Vector3(-1f, 1f, 1f);
        else if (moveInput < 0)
            spriteChild.localScale = new Vector3(1f, 1f, 1f);
    }

    void UpdateAnimation()
    {
        // Don't change animation during dash
        if (isDashing)
            return;

        // Play appropriate animation based on state
        if (isWallSliding)
        {
            PlayAnimation("Wall");
            FlipCharacterOpposite();
        }
        else if (!isGrounded)
        {
            PlayAnimation("Jump");
        }
        else if (moveInput != 0)
        {
            PlayAnimation("Walk");
        }
        else
        {
            PlayAnimation("Idle");
        }
    }

    void PlayAnimation(string animationName)
    {
        // Only update animation if state changed
        if (currentAnimationState != animationName)
        {
            animator.SetTrigger(animationName);
            currentAnimationState = animationName;
        }
    }
}
