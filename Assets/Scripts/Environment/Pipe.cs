using UnityEngine;

public class Pipe : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float despawnX = -7f;

    private PipePool pool;
    private bool active;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void FixedUpdate()
    {
        if (!active)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = Vector2.left * scrollSpeed;

        if (transform.position.x < despawnX)
            ReturnToPool();
    }

    public void Activate(PipePool pipePool)
    {
        pool = pipePool;
        active = true;
    }

    private void ReturnToPool()
    {
        active = false;
        rb.linearVelocity = Vector2.zero;
        pool.ReturnPipe(gameObject);
    }

    private void OnEnable()
    {
        GameManager.OnBirdDied += StopMoving;
    }

    private void OnDisable()
    {
        GameManager.OnBirdDied -= StopMoving;
        active = false;
    }

    private void StopMoving()
    {
        active = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
