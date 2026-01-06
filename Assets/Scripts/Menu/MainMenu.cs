using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectMenu;
    [SerializeField] private GameObject settingsMenu;

    [Header("Menu First Selected Option")]
    [SerializeField] private GameObject mainMenuFirst;
    [SerializeField] private GameObject levelSelectFirst;
    [SerializeField] private GameObject settingsFirst;

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        mainMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
    }

    /**
     * This opens the level select menu.
     */
    public void LevelSelect()
    {
        levelSelectMenu.SetActive(true);
        mainMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(levelSelectFirst);
    }

    /**
     * This opens the settings menu.
     */
    public void Settings()
    {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(settingsFirst);
    }

    /**
     * This closes the game.
     */
    public void Quit()
    {
        Application.Quit();
    }
}
