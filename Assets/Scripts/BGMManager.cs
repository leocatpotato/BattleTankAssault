using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Volumes")]
    [Range(0f, 1f)] public float normalVolume = 0.7f;
    [Range(0f, 1f)] public float pausedVolume = 0.35f;

    AudioSource _source;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        if (_source != null)
        {
            _source.loop = true;
            if (_source.clip != null && !_source.isPlaying)
                _source.Play();

            _source.volume = normalVolume;
        }
    }

    public void SetPaused(bool paused)
    {
        if (_source == null) return;

        _source.volume = paused ? pausedVolume : normalVolume;
    }

    public void ApplyMasterVolume(float master)
    {
        if (_source == null) return;
        _source.volume = (Instance != null && Instance._source != null)
                         ? (master * (Time.timeScale == 0f ? pausedVolume : normalVolume))
                         : master;
    }
}