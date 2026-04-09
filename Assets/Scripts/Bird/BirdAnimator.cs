using UnityEngine;

public class BirdAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 0.12f;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;
    private bool isAnimating = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isAnimating || frames == null || frames.Length == 0)
            return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    public void StopAnimation()
    {
        isAnimating = false;
        if (frames != null && frames.Length > 1)
            spriteRenderer.sprite = frames[1];
    }

    public void ResetAnimation()
    {
        isAnimating = true;
        currentFrame = 0;
        timer = 0f;
    }
}
