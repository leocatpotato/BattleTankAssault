using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    public Transform[] spawnPoints;
    public GameObject enemyPrefab;
    public int enemiesPerWave = 4;
    public int waveCount = 3;
    public float spawnInterval = 0.5f;
    public float waveInterval = 6f;

    [Header("Paths")]
    public Transform waypointGroup;

    public List<GameObject> alive = new();

    void Start() { StartCoroutine(SpawnRoutine()); }

    IEnumerator SpawnRoutine()
    {
        for (int w = 0; w < waveCount; w++)
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                int spIndex = i % Mathf.Max(1, spawnPoints.Length);
                var p = spawnPoints[spIndex];

                var go = Instantiate(enemyPrefab, p.position, p.rotation);
                alive.Add(go);

                BindPath(go, spIndex);

                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(waveInterval);
            CleanupDead();
        }
    }

    void BindPath(GameObject enemy, int spIndex)
    {
        var ai = enemy.GetComponent<TankEnemyAI>();
        if (ai == null)
            return;

        if (waypointGroup == null)
        {
            ai.SetWaypoints(null);
            return;
        }

        int total = waypointGroup.childCount;
        if (total == 0)
        {
            ai.SetWaypoints(null);
            return;
        }

        int half = Mathf.Max(1, total / 2);

        int startIndex = (spIndex == 0) ? 0 : half;
        int endIndex = (spIndex == 0) ? half : total;

        startIndex = Mathf.Clamp(startIndex, 0, total - 1);
        endIndex = Mathf.Clamp(endIndex, startIndex + 1, total);

        int len = endIndex - startIndex;
        Transform[] path = new Transform[len];

        for (int i = 0; i < len; i++)
        {
            path[i] = waypointGroup.GetChild(startIndex + i);
        }

        ai.SetWaypoints(path);
    }

    void CleanupDead()
    {
        alive.RemoveAll(g => g == null);
    }
}