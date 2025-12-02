using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Main Buttons Group")]
    public GameObject mainButtonsGroup;

    [Header("UI")]
    public GameObject loadPanel;

    [Header("How To Play")]
    public GameObject howToPlayPanel;

    public Button btnLoadLevel1;
    public Button btnLoadLevel2;
    public Button btnLoadLevel3;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (loadPanel)
            loadPanel.SetActive(false);

        if (howToPlayPanel)
            howToPlayPanel.SetActive(false);

        if (mainButtonsGroup)
            mainButtonsGroup.SetActive(true);

        if (btnLoadLevel1)
            btnLoadLevel1.interactable = SceneExistsInBuild("Level1");

        if (btnLoadLevel2)
            btnLoadLevel2.interactable = SceneExistsInBuild("Level2");

        if (btnLoadLevel3)
            btnLoadLevel3.interactable = SceneExistsInBuild("Level3");
    }


    public void OnStartGame()
    {
        LoadSceneByNameAsync("Level1");
    }

    public void OnOpenLoadPanel()
    {
        if (loadPanel)
            loadPanel.SetActive(true);

        if (howToPlayPanel)
            howToPlayPanel.SetActive(false);

        if (mainButtonsGroup)
            mainButtonsGroup.SetActive(false);
    }

    public void OnCloseLoadPanel()
    {
        if (loadPanel)
            loadPanel.SetActive(false);

        if (howToPlayPanel)
            howToPlayPanel.SetActive(false);

        if (mainButtonsGroup)
            mainButtonsGroup.SetActive(true);
    }

    public void OnOpenHowToPlay()
    {
        if (howToPlayPanel)
            howToPlayPanel.SetActive(true);

        if (loadPanel)
            loadPanel.SetActive(false);

        if (mainButtonsGroup)
            mainButtonsGroup.SetActive(false);
    }

    public void OnCloseHowToPlay()
    {
        if (howToPlayPanel)
            howToPlayPanel.SetActive(false);

        if (loadPanel)
            loadPanel.SetActive(false);

        if (mainButtonsGroup)
            mainButtonsGroup.SetActive(true);
    }


    public void OnLoadLevel1()
    {
        LoadSceneByNameAsync("Level1");
    }

    public void OnLoadLevel2()
    {
        LoadSceneByNameAsync("Level2");
    }

    public void OnLoadLevel3()
    {
        LoadSceneByNameAsync("Level3");
    }

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
        if (!SceneExistsInBuild(sceneName))
        {
            Debug.LogWarning($"Scene {sceneName} not in Build Settings.");
            return;
        }

        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    bool SceneExistsInBuild(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (fileName == name)
                return true;
        }

        return false;
    }
}