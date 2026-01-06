using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour, IDataPersistence
{
    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectMenu;

    [Header("Menu First Selected Option")]
    [SerializeField] private GameObject mainMenuFirst;

    [Header("Buttons")]
    [SerializeField] private Button[] buttons;

    [Header("Level Preview")]
    [SerializeField] private GameObject levelText;
    [SerializeField] private GameObject levelImage;

    private int levelImageMaxHeight = 600;
    private int levelImageMaxWidth = 600;

    private SerializableSortedDictionary<int, float> bestTimes;

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        levelSelectMenu.SetActive(false);
        levelText.SetActive(false);
        levelImage.SetActive(false);
    }

    /**
     * This loads the data from the best times into this Object.
     * 
     * @param data refers to the data for the curent scene.
     */
    public void LoadData(GameData data)
    {
        bestTimes = data.bestTimes;

        // Enable/disable level select buttons depending on if they're unlocked
        int unlockedLevels = bestTimes.Count + 1;

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        for (int i = 0; i < unlockedLevels && i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }

    }

    /**
     * This saves the data from the curent scene into this Object.
     *
     * @param data refers to the data for the curent scene.
     */
    public void SaveData(ref GameData data)
    {
        // Does Nothing
    }

    /**
     * This loads the specified level.
     * 
     * @param levelNumber refers to the number of the level to load.
     */
    public void loadLevel(int levelNumber)
    {
        SceneManager.LoadScene("Level " + levelNumber);
    }

    /**
     * This goes back to the main menu.
     */
    public void Back()
    {
        mainMenu.SetActive(true);
        levelSelectMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
    }

    /**
     * This loads the preivew for a specified level.
     * 
     * @param levelNumber refers to the number of the level to load the preview of; 0 refers to no level.
     */
    public void LoadPreview(int levelNumber)
    {
        if (levelNumber == 0)
        {
            levelText.SetActive(false);
            levelImage.SetActive(false);
        }
        else
        {
            StringBuilder sb = new StringBuilder();

            // Set Level Text
            sb.Append("Level ").AppendLine(levelNumber.ToString());
            sb.Append("Best Time - ").AppendLine(GetBestTimeText(levelNumber));

            levelText.GetComponent<TextMeshProUGUI>().text = sb.ToString();

            // Set Level Image Sprite
            levelImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("Level Previews/Level " + levelNumber + " Preview");
            ResizeLevelImage();

            levelText.SetActive(true);
            levelImage.SetActive(true);
        }
    }

    /**
     * This resizes the level image preview to be the right proportions
     */
    private void ResizeLevelImage()
    {
        Image image = levelImage.GetComponent<Image>();
        RectTransform rectTransform = levelImage.GetComponent<RectTransform>();

        if (image.sprite == null)
        {
            rectTransform.sizeDelta = new Vector2(0, 0);
        }
        else
        {
            int width;
            int height;

            if (image.sprite.bounds.extents.x > image.sprite.bounds.extents.y)
            {
                width = levelImageMaxWidth;
                height = (int)(levelImageMaxHeight * image.sprite.bounds.extents.y / image.sprite.bounds.extents.x);

            }
            else
            {
                width = (int)(levelImageMaxHeight * image.sprite.bounds.extents.x / image.sprite.bounds.extents.y);
                height = levelImageMaxHeight;
            }

            rectTransform.sizeDelta = new Vector2(width, height);
        }
    }

    /**
     * This returns the specified best time as a string.
     * 
     * @param levelNumber refers to the level Number the best of is gotten. If there
     *        is no best time of the specified level number, return "Not Completed".
     * 
     * @return the specified best time as a string.
     */
    public string GetBestTimeText(int levelNumber)
    {
        if (bestTimes.ContainsKey(levelNumber))
        {
            float bestTime;
            bestTimes.TryGetValue(levelNumber, out bestTime);

            int minutes = Mathf.FloorToInt(bestTime / 60);
            int seconds = Mathf.FloorToInt(bestTime % 60);
            int centiseconds = Mathf.FloorToInt(((bestTime % 1) * 100) % 100);

            return string.Format("<mspace=0.6em>{0:00}:{1:00}:{2:00}</mspace>", minutes, seconds, centiseconds);
        }
        else
        {
            return "Not Completed";
        }


    }

}
