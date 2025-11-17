using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        SceneManager.LoadScene("baris");
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void SettingsButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }
}
