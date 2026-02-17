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

    [Header("Dash")]
    public float dashForce = 30f;
    public float dashDuration = 0.35f;
    public float dashCooldown = 1f;

    private bool isDashing;
    private bool canDash = true;
    private float dashTime;
    private float dashCooldownTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    void FixedUpdate()
    {
        if (isDashing)
            return; // Ignore normal movement while dashing

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
        rb.gravityScale = 20f; // Set back to your normal gravity
    }
}
