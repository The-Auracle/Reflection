using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Inputs")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Menu First Selected Option")]
    [SerializeField] private GameObject pauseMenuFirst;

    // This is called when the script instance is loaded
    private void Awake()
    {
        pauseMenu.SetActive(false);
    }

    /**
     * This pauses the game and opens the pause menu.
     */
    public void Pause()
    {
        OpenPauseMenu();
    }

    /**
     * This resumes the game and closes the pause menu.
     */
    public void Resume()
    {
        ClosePauseMenu();
    }

    /**
     * This restarts the current level.
     */
    public void Restart()
    {
        ClosePauseMenu();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /**
     * This returns the game to the main menu.
     */
    public void MainMenu()
    {
        ClosePauseMenu();
        SceneManager.LoadScene("Main Menu");
    }

    /**
     * This opens the pause menu.
     */
    private void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(pauseMenuFirst);
        Time.timeScale = 0.0f;
        playerInput.enabled = false;
    }

    /**
    * This closes the pause menu.
    */
    private void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1.0f;
        playerInput.enabled = true;
    }
}
