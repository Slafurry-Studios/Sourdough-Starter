using UnityEngine;
using UnityEngine.SceneManagement;

public class AboutMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}