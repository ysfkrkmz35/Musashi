using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
       
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}
