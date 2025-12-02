using UnityEngine;

public class ProceduralObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Points (place child objects here)")]
    public Transform[] spawnPoints;

    [Header("Obstacle Prefabs")]
    public GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    public int maxObstacles = 6;

    public bool allowReuseSpawnPoint = false;

    private void Start()
    {
        SpawnObstacles();
    }

    private void SpawnObstacles()
    {
        int maxPossible = allowReuseSpawnPoint ? maxObstacles : Mathf.Min(maxObstacles, spawnPoints.Length);
        int countToSpawn = Random.Range(1, maxPossible + 1);

        System.Collections.Generic.List<Transform> availablePoints =
            new System.Collections.Generic.List<Transform>(spawnPoints);

        for (int i = 0; i < countToSpawn; i++)
        {
            Transform chosenPoint;

            if (allowReuseSpawnPoint)
            {
                int index = Random.Range(0, spawnPoints.Length);
                chosenPoint = spawnPoints[index];
            }
            else
            {
                if (availablePoints.Count == 0)
                    break;

                int index = Random.Range(0, availablePoints.Count);
                chosenPoint = availablePoints[index];
                availablePoints.RemoveAt(index);
            }

            int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject prefab = obstaclePrefabs[prefabIndex];

            GameObject obstacle = Instantiate(
                prefab,
                chosenPoint.position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            );

            float scale = Random.Range(0.9f, 1.2f);
            obstacle.transform.localScale *= scale;
        }
    }
}