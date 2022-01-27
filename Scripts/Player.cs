using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    
    private float moveSpeed;
    private float jumpForce;

    private bool jumping;
    
    private void Start()
    {
        moveSpeed = Settings.PlayerMoveSpeed;
        jumpForce = Settings.PlayerJumpForce;
        rb        = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            jumping = true;
    }

    private void FixedUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var move       = new Vector3(horizontal * moveSpeed * Time.fixedDeltaTime, 0f);
        rb.MovePosition(rb.position + move);
        
        if(jumping)
        {
            rb.AddForce(Vector3.up * jumpForce);
            jumping = false;
        }
    }

    private bool IsGrounded()
    {
        var ray = new Ray(rb.position, Vector3.down);
        return Physics.Raycast(ray, 0.6f, LayerMask.GetMask("Static Environment"));
    }
}