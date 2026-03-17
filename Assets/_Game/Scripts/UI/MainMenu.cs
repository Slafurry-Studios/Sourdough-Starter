using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string aboutMenuSceneName = "AboutMenu";
    [SerializeField] private string settingsMenuSceneName = "SettingsMenu";


    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }


    public void About()
    {
        SceneManager.LoadScene(aboutMenuSceneName);
    }

    public void Settings()
    {
        SceneManager.LoadScene(settingsMenuSceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }
}