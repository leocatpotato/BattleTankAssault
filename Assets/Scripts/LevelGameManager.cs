using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelGameManager : MonoBehaviour
{
    public static LevelGameManager Instance { get; private set; }

    [Header("Player")]
    public Health playerHealth;

    [Header("UI")]
    public TMP_Text hpText;
    public TMP_Text enemyCountText;
    public TMP_Text scoreText;

    [Header("Level Complete Panel")]
    public GameObject levelCompletePanel;
    public TMP_Text levelCompleteTitle;
    public Button nextLevelButton;
    public Button backToMenuButton;
    public Button restartLevelButton;

    [Header("Pause / Options")]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMenuButton;
    public Button pauseQuitButton;
    public Button pauseOptionsButton;
    public Slider volumeSlider;

    [Header("Level Info")]
    public int totalEnemiesInLevel = 6;
    public string nextLevelSceneName = "Level2";
    public string mainMenuSceneName = "MainMenu";

    private int enemiesKilled;
    private int score;
    private bool levelFinished;
    private bool isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged += OnPlayerHealthChanged;
            playerHealth.onDied += OnPlayerDied;
        }

        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = volume;
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
            OnVolumeChanged(volume);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        enemiesKilled = 0;
        score = 0;
        levelFinished = false;
        isPaused = false;

        UpdateUI();
        LockCursor();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged -= OnPlayerHealthChanged;
            playerHealth.onDied -= OnPlayerDied;
        }
    }

    private void Update()
    {
        if (levelFinished) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused && optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptions();
                return;
            }

            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    private void UpdateUI()
    {
        if (hpText != null && playerHealth != null)
            hpText.text = $"Health: {playerHealth.Cur}/{playerHealth.Max}";

        if (enemyCountText != null)
            enemyCountText.text = $"Enemy: {enemiesKilled}/{totalEnemiesInLevel}";

        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    private void OnPlayerHealthChanged(Health h)
    {
        UpdateUI();
    }


    public void OnPlayerDied(Health h)
    {
        if (levelFinished) return;
        levelFinished = true;

        ShowLevelComplete(false);
    }

    public void OnEnemyKilled(Health enemy)
    {
        if (levelFinished) return;

        enemiesKilled++;
        score += 50;
        UpdateUI();

        if (enemiesKilled >= totalEnemiesInLevel)
        {
            if (playerHealth != null && playerHealth.Cur == playerHealth.Max)
                score += 500;

            ShowLevelComplete(true);
        }
    }

    private void ShowLevelComplete(bool win)
    {
        levelFinished = true;
        isPaused = false;

        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);

        if (levelCompleteTitle != null)
            levelCompleteTitle.text = win ? "Level Complete" : "Game Over";

        UpdateUI();
        UnlockCursor();
    }

    public void OnClick_NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void OnClick_BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnClick_RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        UnlockCursor();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        LockCursor();
    }

    public void Pause_RestartLevel() => OnClick_RestartLevel();
    public void Pause_BackToMenu() => OnClick_BackToMenu();

    public void Pause_QuitGame()
    {
        Time.timeScale = 1f;
        UnlockCursor();
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void OpenOptions()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null && isPaused) pausePanel.SetActive(true);
    }

    public void OnVolumeChanged(float value)
    {
        float v = Mathf.Clamp01(value);
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("MasterVolume", v);
    }

    private void LockCursor()
    {
        if (CursorManger.Instance != null)
            CursorManger.Instance.LockCursor();
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void UnlockCursor()
    {
        if (CursorManger.Instance != null)
            CursorManger.Instance.UnlockCursor();
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}