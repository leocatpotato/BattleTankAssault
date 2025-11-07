using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject loadPanel;
    public Button btnLoadLevel2;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (btnLoadLevel2)
        {
            bool hasLevel2 = SceneExistsInBuild("Level2") || SceneExistsInBuildIndex(2);
            btnLoadLevel2.interactable = hasLevel2;
        }

        if (loadPanel) loadPanel.SetActive(false);
    }

    public void OnStartGame()
    {
        LoadSceneByNameAsync("Level1");
    }

    public void OnOpenLoadPanel()
    {
        if (loadPanel) loadPanel.SetActive(true);
    }

    public void OnCloseLoadPanel()
    {
        if (loadPanel) loadPanel.SetActive(false);
    }

    public void OnLoadLevel1() => LoadSceneByNameAsync("Level1");
    public void OnLoadLevel2() => LoadSceneByNameAsync("Level2");

    public void OnQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void LoadSceneByNameAsync(string sceneName)
    {
        if (!SceneExistsInBuild(sceneName)) { Debug.LogWarning($"Scene {sceneName} not in Build Settings."); return; }
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    bool SceneExistsInBuild(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var filename = System.IO.Path.GetFileNameWithoutExtension(path);
            if (filename == name) return true;
        }
        return false;
    }

    bool SceneExistsInBuildIndex(int index)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        return index >= 0 && index < count;
    }
}