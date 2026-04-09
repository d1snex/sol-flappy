using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1.8f;
    [SerializeField] private float minGapY = -1.5f;
    [SerializeField] private float maxGapY = 1.5f;
    [SerializeField] private float gapSize = 2.5f;
    [SerializeField] private float spawnX = 6.5f;
    [SerializeField] private float pipeHeight = 6.4f;

    private PipePool pool;
    private float timer;
    private bool spawning;

    private void Awake()
    {
        pool = GetComponent<PipePool>();
    }

    private void OnEnable()
    {
        GameManager.OnGameStarted += StartSpawning;
        GameManager.OnBirdDied += StopSpawning;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= StartSpawning;
        GameManager.OnBirdDied -= StopSpawning;
    }

    private void Update()
    {
        if (!spawning)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            SpawnPipes();
        }
    }

    private void SpawnPipes()
    {
        float gapCenterY = Random.Range(minGapY, maxGapY);
        float halfGap = gapSize * 0.5f;

        GameObject go = pool.GetPipe();
        go.transform.position = new Vector3(spawnX, 0f, 0f);

        Transform topPipe = go.transform.Find("TopPipe");
        Transform bottomPipe = go.transform.Find("BottomPipe");
        Transform scoreGate = go.transform.Find("ScoreGate");

        topPipe.localPosition = new Vector3(0f, gapCenterY + halfGap + pipeHeight * 0.5f, 0f);
        bottomPipe.localPosition = new Vector3(0f, gapCenterY - halfGap - pipeHeight * 0.5f, 0f);
        scoreGate.localPosition = new Vector3(0f, gapCenterY, 0f);

        go.GetComponent<Pipe>().Activate(pool);
    }

    private void StartSpawning()
    {
        spawning = true;
        timer = spawnInterval;
    }

    private void StopSpawning()
    {
        spawning = false;
    }
}
