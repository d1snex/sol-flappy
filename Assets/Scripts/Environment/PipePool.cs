using System.Collections.Generic;
using UnityEngine;

public class PipePool : MonoBehaviour
{
    [SerializeField] private GameObject pipePrefab;
    [SerializeField] private int initialPoolSize = 5;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject go = Instantiate(pipePrefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    public GameObject GetPipe()
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
        }
        else
        {
            go = Instantiate(pipePrefab, transform);
        }
        go.SetActive(true);
        return go;
    }

    public void ReturnPipe(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }
}
