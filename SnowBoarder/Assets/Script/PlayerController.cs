using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //--LEVEL 1--//
    [SerializeField] float torqueAmount;
    public int stopTime;

    [Header("Speed")]
    [SerializeField] float slowSpeed;     // tốc độ khi giảm tốc
    [SerializeField] float boostSpeed;    // tốc độ khi tăng tốc
    [SerializeField] float defaultSpeed;  // tốc độ bình thường

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    float groundCheckRadius = 0.2f;

    [Header("Collision")]
    [SerializeField] float treeSlowDuration = 2f;   // thời gian giảm tốc khi va cây
    [SerializeField] float rockStopDuration = 1f;   // thời gian dừng khi va đá

    Rigidbody2D rb2d;
    SurfaceEffector2D surfaceEffector;
    bool canMove = true;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        surfaceEffector = FindFirstObjectByType<SurfaceEffector2D>();

        // Set tốc độ mặc định ban đầu
        surfaceEffector.speed = defaultSpeed;
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
            surfaceEffector.speed = defaultSpeed; // tốc độ mặc định
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

    // Xử lý va chạm với chướng ngại vật
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canMove) return;

        if (collision.gameObject.CompareTag("Tree"))
        {
            StartCoroutine(SlowDownTemporarily()); // Va cây → giảm tốc
        }
        else if (collision.gameObject.CompareTag("Rock"))
        {
            StartCoroutine(StopTemporarily()); // Va đá → dừng tạm thời

            // TODO: Thêm xử lý mất mạng sau này:
            // FindObjectOfType<GameManager>().PlayerDied();
        }
    }

    // Giảm tốc độ tạm thời khi va cây
    IEnumerator SlowDownTemporarily()
    {
        surfaceEffector.speed = slowSpeed;
        yield return new WaitForSeconds(treeSlowDuration);
        surfaceEffector.speed = defaultSpeed;
    }

    // Dừng hẳn tạm thời khi va đá
    IEnumerator StopTemporarily()
    {
        surfaceEffector.speed = 0f;
        yield return new WaitForSeconds(rockStopDuration);
        surfaceEffector.speed = defaultSpeed;
    }
}
