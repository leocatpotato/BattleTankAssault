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
        if (ai == null) return;

        if (waypointGroup == null) { ai.SetWaypoints(null); return; }

        int n = waypointGroup.childCount;
        if (n < 6) { ai.SetWaypoints(null); return; }

        Transform[] pathA = new Transform[3] { waypointGroup.GetChild(0), waypointGroup.GetChild(1), waypointGroup.GetChild(2) };
        Transform[] pathB = new Transform[3] { waypointGroup.GetChild(3), waypointGroup.GetChild(4), waypointGroup.GetChild(5) };

        if (spIndex == 0) ai.SetWaypoints(pathA);
        else if (spIndex == 1) ai.SetWaypoints(pathB);
        else ai.SetWaypoints((spIndex % 2 == 0) ? pathA : pathB);
    }

    void CleanupDead()
    {
        alive.RemoveAll(g => g == null);
    }
}