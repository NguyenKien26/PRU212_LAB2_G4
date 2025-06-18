using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour
{
    //--LEVEL 1--//
    [SerializeField]
    float torqueAmount;
    public int stopTime;

    [SerializeField]
    float slowSpeed; // tốc độ khi giảm tốc
    [SerializeField]
    float jumpForce;
    [SerializeField]
    LayerMask groundLayer;
    [SerializeField]
    Transform groundCheck;
    float groundCheckRadius = 0.2f;
    [SerializeField]
    float boostSpeed; // tốc độ tăng tốc

    Rigidbody2D rb2d;
    SurfaceEffector2D surfaceEffector;
    bool canMove = true;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        surfaceEffector = FindFirstObjectByType<SurfaceEffector2D>();
    }

    void Update()
    {
        if (canMove)
        {
            RotatePlayer();
            RespondToSpeedControl();
            HandleJump();
        }
    }

    public void DisablePlayer()
    {
        canMove = false;
        surfaceEffector.speed = 0f;
    }

    void RotatePlayer()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb2d.AddTorque(torqueAmount);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rb2d.AddTorque(-torqueAmount);
        }
    }

    void RespondToSpeedControl()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            surfaceEffector.speed = boostSpeed; // tăng tốc
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            surfaceEffector.speed = slowSpeed; // giảm tốc
        }
        else
        {
            surfaceEffector.speed = stopTime; // tốc độ mặc định
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}
