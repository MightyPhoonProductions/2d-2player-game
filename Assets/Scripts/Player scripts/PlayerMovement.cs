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
    [Tooltip("How fast the player moves left/right.")]
    public float moveSpeed = 5f;

    //==================== JUMP SETTINGS ====================
    [Header("Jump Settings")]
    [Tooltip("How high the player can jump.")]
    public float jumpHeight = 2f;

    [Tooltip("Layer(s) considered as ground for jumping.")]
    public LayerMask groundLayer;

    //==================== PRIVATE COMPONENTS ====================
    private Rigidbody2D rb;
    private Animator anim;
    private float gravityScale;
    private bool isGrounded;

    //==================== UNITY METHODS ====================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;

        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
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

        // Apply movement
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
            // Optional: Trigger death animation or restart
        }
    }
}
