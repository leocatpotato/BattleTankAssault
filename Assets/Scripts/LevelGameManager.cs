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

    [Header("Level Info")]
    public int totalEnemiesInLevel = 6;
    public string nextLevelSceneName = "Level2";
    public string mainMenuSceneName = "MainMenu";

    private int enemiesKilled;
    private int score;
    private bool levelFinished;

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
        }

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        enemiesKilled = 0;
        score = 0;
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged -= OnPlayerHealthChanged;
        }
    }


    private void UpdateUI()
    {
        if (hpText != null && playerHealth != null)
            hpText.text = $"{playerHealth.Cur}/{playerHealth.Max}";

        if (enemyCountText != null)
            enemyCountText.text = $"{enemiesKilled}/{totalEnemiesInLevel}";

        if (scoreText != null)
            scoreText.text = score.ToString();
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
            {
                score += 500;
            }

            ShowLevelComplete(true);
        }
    }


    private void ShowLevelComplete(bool win)
    {
        levelFinished = true;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (levelCompleteTitle != null)
            levelCompleteTitle.text = win ? "Level Complete" : "Game Over";

        UpdateUI();
    }



    public void OnClick_NextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelSceneName))
            SceneManager.LoadScene(nextLevelSceneName);
    }

    public void OnClick_BackToMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }
}