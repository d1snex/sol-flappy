using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private Transform[] tiles;
    [SerializeField] private float tileWidth = 3.36f;

    private bool scrolling;
    private float screenLeftX;
    private float screenRightX;

    private void Start()
    {
        CacheScreenBounds();
    }

    private void OnEnable()
    {
        GameManager.OnGameStarted += StartScrolling;
        GameManager.OnBirdDied += StopScrolling;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= StartScrolling;
        GameManager.OnBirdDied -= StopScrolling;
    }

    private void Update()
    {
        if (!scrolling)
            return;

        float move = scrollSpeed * Time.deltaTime;
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].position += Vector3.left * move;
        }

        WrapTiles();
    }

    private void CacheScreenBounds()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            float halfWidth = cam.orthographicSize * cam.aspect;
            screenLeftX = cam.transform.position.x - halfWidth;
            screenRightX = cam.transform.position.x + halfWidth;
        }
        else
        {
            // Fallback for portrait 9:16
            screenLeftX = -2.85f;
            screenRightX = 2.85f;
        }
    }

    private void WrapTiles()
    {
        // Loop to wrap multiple tiles per frame if needed
        for (int pass = 0; pass < tiles.Length; pass++)
        {
            // Find leftmost and rightmost
            int leftIdx = 0;
            float leftX = tiles[0].position.x;
            float rightX = tiles[0].position.x;

            for (int i = 1; i < tiles.Length; i++)
            {
                if (tiles[i].position.x < leftX)
                {
                    leftX = tiles[i].position.x;
                    leftIdx = i;
                }
                if (tiles[i].position.x > rightX)
                    rightX = tiles[i].position.x;
            }

            // Tile's right edge
            float tileRightEdge = leftX + tileWidth * 0.5f;

            // Wrap if tile's right edge is off the left side of the screen
            if (tileRightEdge < screenLeftX)
            {
                Vector3 pos = tiles[leftIdx].position;
                pos.x = rightX + tileWidth;
                tiles[leftIdx].position = pos;
            }
            else
            {
                break; // No more tiles need wrapping
            }
        }
    }

    private void StartScrolling() => scrolling = true;
    private void StopScrolling() => scrolling = false;
}
