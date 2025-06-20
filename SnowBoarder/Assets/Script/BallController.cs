using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] float maxBallSpeed = 5f;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1.5f;
        // Đặt tốc độ ban đầu ngay khi game bắt đầu
        rb.linearVelocity = transform.right * maxBallSpeed;
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;

        // Chỉ giới hạn tốc độ theo trục X (ngang)
        if (Mathf.Abs(velocity.x) > maxBallSpeed)
        {
            velocity.x = Mathf.Sign(velocity.x) * maxBallSpeed;
        }

        rb.linearVelocity = velocity;
    }
}
