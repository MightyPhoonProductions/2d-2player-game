using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //==================== PLAYER SETTINGS ====================
    public enum PlayerID { Player1, Player2 }
    [Header("Player Settings")]
    [Tooltip("Select which player this controller is for.")]
    public PlayerID playerID;

    //==================== MOVEMENT SETTINGS ====================
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    // Dash (Player1 only)
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    // Invisibility (Player2 only)
    private bool canGoInvisible = true;
    private bool isInvisible;
    private float invisibleTime = 2f;
    private float invisibleCooldown = 5f;
    [Tooltip("Objects with this tag can be walked through while invisible.")]
    public string throughTag = "ThroughObject";

    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;

    //==================== JUMP SETTINGS ====================
    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public LayerMask groundLayer;

    //==================== PRIVATE COMPONENTS ====================
    private Rigidbody2D rb;
    private Animator anim;
    private float gravityScale;
    private bool isGrounded;

    [SerializeField] private TrailRenderer tr;

    //==================== UNITY METHODS ====================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb != null ? rb.gravityScale : 1f;

        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDashing)
        {
            return; // lock input while dashing
        }

        HandleMovement();
        HandleJump();

        // Player1 Dash
        if (playerID == PlayerID.Player1 && Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // Player2 Invisibility
        if (playerID == PlayerID.Player2 && Input.GetKeyDown(KeyCode.RightShift) && canGoInvisible)
        {
            StartCoroutine(GoInvisible());
        }
    }

    //==================== MOVEMENT ====================
    void HandleMovement()
    {
        float moveDirection = 0f;

        if (playerID == PlayerID.Player1)
        {
            if (Input.GetKey(KeyCode.A)) moveDirection = -1f;
            if (Input.GetKey(KeyCode.D)) moveDirection = 1f;
        }
        else if (playerID == PlayerID.Player2)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) moveDirection = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) moveDirection = 1f;
        }

        // Apply movement (preserve current Y)
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveDirection * moveSpeed;
        rb.linearVelocity = velocity;

        // Flip sprite based on direction
        if (moveDirection != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveDirection);
            transform.localScale = scale;
        }

        // Update animation (optional)
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveDirection));
        }
    }

    //==================== DASHING (PLAYER1 ONLY) ====================
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDir = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(dashDir * dashingPower, rb.linearVelocity.y);

        if (tr != null) tr.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        if (tr != null) tr.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    //==================== INVISIBILITY (PLAYER2 ONLY, FIXED) ====================
    private IEnumerator GoInvisible()
    {
        canGoInvisible = false;
        isInvisible = true;

        // Make Player2 semi-transparent
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0.4f; // 40% visible
            spriteRenderer.color = c;
        }

        // Find all colliders with the throughTag
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(throughTag);
        foreach (var obj in taggedObjects)
        {
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null)
            {
                Physics2D.IgnoreCollision(playerCollider, col, true);
            }
        }

        // Stay invisible for X seconds
        yield return new WaitForSeconds(invisibleTime);

        // Restore visibility
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f; // fully visible again
            spriteRenderer.color = c;
        }

        // Re-enable collisions with tagged objects
        foreach (var obj in taggedObjects)
        {
            if (obj != null)
            {
                Collider2D col = obj.GetComponent<Collider2D>();
                if (col != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, col, false);
                }
            }
        }

        isInvisible = false;

        // Cooldown before invisibility can be used again
        yield return new WaitForSeconds(invisibleCooldown);
        canGoInvisible = true;
    }

    //==================== JUMPING ====================
    void HandleJump()
    {
        isGrounded = IsGrounded();

        bool jumpKeyPressed = false;

        if (playerID == PlayerID.Player1)
        {
            jumpKeyPressed = Input.GetKeyDown(KeyCode.W);
        }
        else if (playerID == PlayerID.Player2)
        {
            jumpKeyPressed = Input.GetKeyDown(KeyCode.UpArrow);
        }

        if (jumpKeyPressed && isGrounded)
        {
            if (anim != null)
            {
                anim.SetTrigger("Jump");
            }

            float jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * gravityScale) * jumpHeight);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
        }
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.3f;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null) return false;

        Bounds bounds = collider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
        float width = bounds.extents.x;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            new Vector2(width * 2, extraHeight),
            0f,
            Vector2.down,
            extraHeight,
            groundLayer
        );

        return hit.collider != null;
    }

    //==================== DEBUGGING ====================
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Collider2D collider = GetComponent<Collider2D>();

        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
            float extraHeight = 0.1f;
            float width = bounds.extents.x;

            Gizmos.DrawWireCube(
                origin + Vector2.down * extraHeight / 2,
                new Vector2(width * 2, extraHeight)
            );
        }
    }

    //==================== DEATH & COLLISION ====================
    IEnumerator Dead()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy hit!");
        }
    }
}
