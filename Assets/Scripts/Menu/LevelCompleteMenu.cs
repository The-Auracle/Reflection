using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelCompleteMenu : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private LevelTimer levelTimerScript;
    
    [Header("Components")]
    [SerializeField] private GameObject levelCompleteMenu;

    [Header("Menu First Selected Option")]
    [SerializeField] private GameObject levelCompleteMenuFirst;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI levelCompleteTimeText;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    // This is called when the script instance is loaded
    private void Awake()
    {
        levelCompleteMenu.SetActive(false);
    }

    /**
     * This finishes the level when the goal is reached.
     */
    public void LevelComplete()
    {

        levelTimerScript.LevelComplete();
        LevelCompleteTime();
        playerInput.enabled = false;
    }

    /**
     * This opens the level complete menu.
     */
    public void OpenLevelCompleteMenu()
    {
        levelCompleteMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(levelCompleteMenuFirst);
    }


    /**
     * This restarts the current level.
     */
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /**
     * This progresses the game to the next level.
     */
    public void NextLevel()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /**
     * This returns the game to the main menu.
     */
    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    /**
     * This sets the LevelCompleteTimeText to the appropriate level complete time and best time.
     */
    private void LevelCompleteTime()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Level Time: ").AppendLine(levelTimerScript.GetElapsedTimeText());
        sb.Append("Best Time: ").AppendLine(levelTimerScript.GetBestTimeText());

        if (levelTimerScript.IsNewRecord())
        {
            sb.AppendLine("New Record!!!");
        }

        levelCompleteTimeText.text = sb.ToString();
    }
}
