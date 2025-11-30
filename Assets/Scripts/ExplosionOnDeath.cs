using UnityEngine;

public class ExplosionOnDeath : MonoBehaviour
{
    public GameObject explosionPrefab;
    public AudioClip explosionAudio;
    public float destroyDelay = 2f;

    private Health _health;

    private void OnEnable()
    {
        _health = GetComponent<Health>();

        if (_health != null)
            _health.onDied += HandleDied;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.onDied -= HandleDied;
    }

    private void HandleDied(Health h)
    {
        GameObject fx = null;
        if (explosionPrefab != null)
        {
            fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (explosionAudio != null)
        {
            GameObject audioObj = fx != null ? fx : new GameObject("ExplosionSound");

            AudioSource src = audioObj.AddComponent<AudioSource>();
            src.clip = explosionAudio;
            src.spatialBlend = 1.0f;
            src.volume = 1.0f;
            src.Play();

            if (fx == null)
                Destroy(audioObj, explosionAudio.length);
        }

        Destroy(gameObject, destroyDelay);
    }
}
