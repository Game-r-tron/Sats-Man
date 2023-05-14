using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GamePlayControl : MonoBehaviour
{

    public GameObject pauseMenu;
    private bool isPaused;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPause()
    {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
    }  

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Pacman");
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        // Replace "MainMenu" with the name of your main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}
