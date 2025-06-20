using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

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

    [Header("Flip")]
    [SerializeField] float flipDuration = 0.8f;     // Thời gian thực hiện lộn nhào
    [SerializeField] float flipJumpForce = 2f;      // Lực nhảy nhẹ khi lộn nhào
    [SerializeField] int flipScore = 100;           // Điểm thưởng khi lộn nhào

    [Header("Crash Detection")]
    [SerializeField] float maxHeadAngle = 150f;     // Góc tối đa trước khi coi là cắm đầu (tăng để ít nhạy hơn)
    [SerializeField] Transform headCheck;           // Vị trí kiểm tra đầu
    [SerializeField] float headCheckRadius = 0.2f;  // Bán kính kiểm tra va chạm đầu
    [SerializeField] float crashVelocityThreshold = 8f; // Ngưỡng vận tốc khi va chạm (tăng để chỉ phát hiện va chạm mạnh)

    Rigidbody2D rb2d;
    SurfaceEffector2D surfaceEffector;
    bool canMove = true;
    bool isFlipping = false; // Trạng thái lộn nhào
    float initialXPosition; // Vị trí X ban đầu
    float currentDistance; // Khoảng cách hiện tại

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        surfaceEffector = FindFirstObjectByType<SurfaceEffector2D>();
        if (surfaceEffector == null)
        {
            Debug.LogError("SurfaceEffector2D not found in scene!");
        }

        // Lưu vị trí X ban đầu
        initialXPosition = transform.position.x;
        currentDistance = 0f;

        // Set tốc độ mặc định ban đầu
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
            UpdateDistance();
            CheckHeadCrash(); // Kiểm tra cắm đầu
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
        //Debug.Log($"IsGrounded: {grounded}, GroundCheckPos: {groundCheck.position}");
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
            Debug.Log($"Flipping: Angle: {currentAngle}, Time: {elapsedTime}/{flipDuration}");
            yield return null;
        }

        transform.eulerAngles = new Vector3(0, 0, endAngle % 360);
        isFlipping = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(flipScore);
            Debug.Log($"Flip completed! Earned {flipScore} points.");
        }
        else
        {
            Debug.LogError("GameManager.Instance is null in PerformFlip!");
        }
    }

    void UpdateDistance()
    {
        currentDistance = transform.position.x - initialXPosition;
        if (currentDistance < 0) currentDistance = 0;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateDistance(currentDistance);
        }
        else
        {
            Debug.LogError("GameManager.Instance is null in UpdateDistance!");
        }
    }

    void CheckHeadCrash()
    {
        if (!canMove || isFlipping) return; // Bỏ qua khi đang lộn nhào

        float angle = transform.eulerAngles.z;
        if (angle > 180f) angle -= 360f;
        angle = Mathf.Abs(angle);

        //Debug.Log($"Angle: {angle}, IsFlipping: {isFlipping}, IsGrounded: {IsGrounded()}, Velocity: {rb2d.linearVelocity.magnitude}");

        if (angle > maxHeadAngle && !IsGrounded())
        {
            Debug.Log($"Player crashed head-first! Angle: {angle}, Triggering GameOver.");
            TriggerGameOver();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canMove) return;

        Debug.Log($"Collision with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}, Velocity: {rb2d.linearVelocity.magnitude}, IsFlipping: {isFlipping}");

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
            else
            {
                Debug.LogError("GameManager.Instance is null in OnCollisionEnter2D (Finish)!");
            }
        }
        else if (collision.gameObject.layer == groundLayer.value && !isFlipping)
        {
            // Kiểm tra va chạm mạnh hoặc cắm đầu
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
        else
        {
            Debug.LogError("GameManager.Instance is null in TriggerGameOver!");
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