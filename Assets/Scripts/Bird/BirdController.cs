using UnityEngine;

public class BirdController : MonoBehaviour
{
    [SerializeField] private float flapForce = 5f;
    [SerializeField] private float maxFallSpeed = -8f;
    [SerializeField] private float tiltUpAngle = 30f;
    [SerializeField] private float tiltDownAngle = -90f;
    [SerializeField] private float tiltSpeed = 200f;

    private Rigidbody2D rb;
    private bool isAlive = true;
    private bool gameStarted;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;
    }

    private void OnEnable()
    {
        InputHandler.OnTap += OnTap;
        GameManager.OnGameStarted += OnGameStarted;
        GameManager.OnBirdDied += OnDied;
    }

    private void OnDisable()
    {
        InputHandler.OnTap -= OnTap;
        GameManager.OnGameStarted -= OnGameStarted;
        GameManager.OnBirdDied -= OnDied;
    }

    private void Update()
    {
        if (!isAlive || !gameStarted)
            return;

        float t = Mathf.InverseLerp(maxFallSpeed, flapForce, rb.linearVelocity.y);
        float targetAngle = Mathf.Lerp(tiltDownAngle, tiltUpAngle, t);
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180f)
            currentAngle -= 360f;
        float newAngle = Mathf.MoveTowards(currentAngle, targetAngle, tiltSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    private void FixedUpdate()
    {
        if (!isAlive || !gameStarted)
            return;

        Vector2 vel = rb.linearVelocity;
        if (vel.y < maxFallSpeed)
        {
            vel.y = maxFallSpeed;
            rb.linearVelocity = vel;
        }
    }

    private void OnTap()
    {
        if (!isAlive || !gameStarted)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, flapForce);
        AudioManager.Instance.PlayWing();
    }

    private void OnGameStarted()
    {
        gameStarted = true;
        rb.simulated = true;
        rb.linearVelocity = new Vector2(0f, flapForce);
    }

    private void OnDied()
    {
        isAlive = false;
        GetComponent<BirdAnimator>().StopAnimation();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isAlive)
            return;

        GameManager.Instance.BirdDied();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive)
            return;

        if (other.CompareTag("ScoreGate"))
            ScoreManager.Instance.AddPoint();
        else if (other.CompareTag("Ceiling"))
            GameManager.Instance.BirdDied();
    }
}
