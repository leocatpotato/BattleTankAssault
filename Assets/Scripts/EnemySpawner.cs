using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject enemyPrefab;
    public int enemiesPerWave = 5;
    public int waveCount = 3;
    public float spawnInterval = 0.4f;
    public float waveInterval = 6f;

    public List<GameObject> alive = new();

    void Start() { StartCoroutine(SpawnRoutine()); }

    IEnumerator SpawnRoutine()
    {
        for (int w = 0; w < waveCount; w++)
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var go = Instantiate(enemyPrefab, p.position, p.rotation);
                alive.Add(go);
                yield return new WaitForSeconds(spawnInterval);
            }
            yield return new WaitForSeconds(waveInterval);
            CleanupDead();
        }
    }

    void CleanupDead()
    {
        alive.RemoveAll(g => g == null);
    }
}