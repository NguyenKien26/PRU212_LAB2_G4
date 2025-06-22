using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float torqueAmount;
    public int stopTime;

    [Header("Speed")]
    [SerializeField] float slowSpeed;
    [SerializeField] float boostSpeed;
    [SerializeField] float defaultSpeed;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    float groundCheckRadius = 0.2f;

    [Header("Collision")]
    [SerializeField] float treeSlowDuration = 2f;
    [SerializeField] float rockStopDuration = 1f;

    [Header("Flip")]
    [SerializeField] float flipDuration = 0.8f;
    [SerializeField] float flipJumpForce = 2f;
    [SerializeField] int flipScore = 100;

    [Header("Coin")]
    [SerializeField] int coinScore = 10;

    [Header("Crash Detection")]
    [SerializeField] float maxHeadAngle = 150f;
    [SerializeField] Transform headCheck;
    [SerializeField] float headCheckRadius = 0.2f;
    [SerializeField] float crashVelocityThreshold = 8f;

    Rigidbody2D rb2d;
    SurfaceEffector2D surfaceEffector;
    bool canMove = true;
    bool isFlipping = false;
    float initialXPosition;
    float currentDistance;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        surfaceEffector = FindFirstObjectByType<SurfaceEffector2D>();
        if (surfaceEffector == null)
        {
            Debug.LogError("SurfaceEffector2D not found in scene!");
        }

        initialXPosition = transform.position.x;
        currentDistance = 0f;

        if (surfaceEffector != null)
        {
            surfaceEffector.speed = defaultSpeed;
        }
    }

    void Update()
    {
        if (canMove && GameManager.Instance != null)
        {
            if (!isFlipping) RotatePlayer();
            RespondToSpeedControl();
            HandleJump();
            HandleFlip();
            UpdateDistanceAndSpeed();
            CheckHeadCrash();
        }
        else if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager is null, skipping Update in PlayerController");
        }
        else if (!canMove)
        {
            Debug.Log($"Player stopped. canMove: {canMove}, Velocity: {rb2d.linearVelocity.magnitude}, IsFlipping: {isFlipping}");
            TriggerGameOver();
        }
    }

    public void DisablePlayer()
    {
        canMove = false;
        if (surfaceEffector != null)
        {
            surfaceEffector.speed = 0f;
        }
        Debug.Log("Player disabled.");
    }

    // Đặt lại trạng thái người chơi
    public void ResetPlayerState()
    {
        Debug.Log("ResetPlayerState called in PlayerController");
        canMove = true;
        isFlipping = false;
        if (surfaceEffector != null)
        {
            surfaceEffector.speed = defaultSpeed;
        }
        rb2d.linearVelocity = Vector2.zero;
        rb2d.angularVelocity = 0f;
        transform.rotation = Quaternion.identity; // Đặt lại góc xoay
        initialXPosition = transform.position.x; // Cập nhật vị trí ban đầu
        currentDistance = 0f;
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
        if (surfaceEffector == null) return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            surfaceEffector.speed = boostSpeed;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            surfaceEffector.speed = slowSpeed;
        }
        else
        {
            surfaceEffector.speed = defaultSpeed;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            Debug.Log("Jump performed.");
        }
    }

    bool IsGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        return grounded;
    }

    void HandleFlip()
    {
        if (Input.GetKeyDown(KeyCode.F) && !IsGrounded() && !isFlipping)
        {
            StartCoroutine(PerformFlip());
        }
    }

    IEnumerator PerformFlip()
    {
        isFlipping = true;
        Debug.Log("Starting flip.");

        rb2d.AddForce(Vector2.up * flipJumpForce, ForceMode2D.Impulse);

        float startAngle = transform.eulerAngles.z;
        float endAngle = startAngle + 360f;
        float elapsedTime = 0f;

        while (elapsedTime < flipDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flipDuration;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            transform.eulerAngles = new Vector3(0, 0, currentAngle);
            yield return null;
        }

        transform.eulerAngles = new Vector3(0, 0, endAngle % 360);
        isFlipping = false;

        if (GameManager.Instance != null)
        {
            Vector3 flipTextPosition = headCheck != null ? headCheck.position : transform.position + new Vector3(0, 1f, 0);
            GameManager.Instance.AddScore(flipScore, flipTextPosition, $"+{flipScore} Flip!", Color.cyan);
            Debug.Log($"Flip completed! Earned {flipScore} points.");
        }
    }

    void UpdateDistanceAndSpeed()
    {
        currentDistance = transform.position.x - initialXPosition;
        if (currentDistance < 0) currentDistance = 0;

        float speed = rb2d.linearVelocity.x;
        if (speed < 0) speed = 0;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateDistance(currentDistance);
            GameManager.Instance.UpdateSpeed(speed);
        }
    }

    void CheckHeadCrash()
    {
        if (!canMove || isFlipping) return;

        float angle = transform.eulerAngles.z;
        if (angle > 180f) angle -= 360f;
        angle = Mathf.Abs(angle);

        if (angle > maxHeadAngle && !IsGrounded())
        {
            Debug.Log($"Player crashed head-first! Angle: {angle}, Triggering GameOver.");
            TriggerGameOver();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canMove) return;

        Debug.Log($"Collision with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Tree"))
        {
            StartCoroutine(SlowDownTemporarily());
        }
        else if (collision.gameObject.CompareTag("Rock"))
        {
            StartCoroutine(StopTemporarily());
        }
        else if (collision.gameObject.CompareTag("Finish"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReachFinish(GameManager.Instance.currentScore, currentDistance);
            }
        }
        else if (collision.gameObject.layer == groundLayer.value && !isFlipping)
        {
            float velocityMagnitude = rb2d.linearVelocity.magnitude;
            bool headCrash = headCheck != null && Physics2D.OverlapCircle(headCheck.position, headCheckRadius, groundLayer);
            float angle = Mathf.Abs(transform.eulerAngles.z > 180f ? transform.eulerAngles.z - 360f : transform.eulerAngles.z);

            if ((velocityMagnitude > crashVelocityThreshold || headCrash) && angle > maxHeadAngle)
            {
                Debug.Log($"Crash detected! Velocity: {velocityMagnitude}, HeadCrash: {headCrash}, Angle: {angle}, Triggering GameOver.");
                TriggerGameOver();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canMove) return;

        if (collision.CompareTag("Coin"))
        {
            if (GameManager.Instance != null)
            {
                Vector3 coinTextPosition = collision.transform.position + new Vector3(0, 0.5f, 0);
                GameManager.Instance.AddScore(coinScore, coinTextPosition, $"+{coinScore} Coin!", Color.yellow);
                Destroy(collision.gameObject);
            }
        }
    }

    void TriggerGameOver()
    {
        canMove = false;
        if (surfaceEffector != null)
        {
            surfaceEffector.speed = 0f;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(GameManager.Instance.currentScore, currentDistance);
        }
        Debug.Log("GameOver triggered.");
    }

    IEnumerator StopTemporarily()
    {
        if (surfaceEffector != null)
        {
            surfaceEffector.speed = 0f;
        }
        canMove = false;
        yield return new WaitForSeconds(rockStopDuration);
        TriggerGameOver();
    }

    IEnumerator SlowDownTemporarily()
    {
        if (surfaceEffector != null)
        {
            surfaceEffector.speed = slowSpeed;
            yield return new WaitForSeconds(treeSlowDuration);
            surfaceEffector.speed = defaultSpeed;
        }
    }

    void OnDrawGizmos()
    {
        if (headCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}