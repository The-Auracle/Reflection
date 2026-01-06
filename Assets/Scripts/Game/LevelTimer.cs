using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour, IDataPersistence
{
    [Header("Level ID")]
    [SerializeField] int id;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI levelTimerText;

    private float elapsedTime;
    private float bestTime;

    private bool isTimerRunning = true;
    private bool isNewRecord = false;

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();

        levelTimerText.text = GetElapsedTimeText();
    }

    /**
     * This loads the data from the curent level into this Object.
     * 
     * @param data refers to the data for the curent level.
     */
    public void LoadData(GameData data)
    {
        if (data.bestTimes.ContainsKey(id))
        {
            data.bestTimes.TryGetValue(id, out bestTime);
        }
        else
        {
            bestTime = float.MaxValue;
        }
    }

    /**
     * This saves the data from the curent level into this Object.
     *
     * @param data refers to the data for the curent level.
     */
    public void SaveData(ref GameData data)
    {
        if (data.bestTimes.ContainsKey(id))
        {
            data.bestTimes.Remove(id);
        }

        if (bestTime != float.MaxValue)
        {
            data.bestTimes.Add(id, bestTime);
        }
    }

    /**
     * This updates the timer to the current time if the timer is running.
     */
    private void UpdateTimer()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    /**
     * This stops the timer and updates the best time for the curent level.
     */
    public void LevelComplete()
    {
        isTimerRunning = false;

        // Round elapsed time and best time to 2 decimal places.
        float roundedElapsedTime = (Mathf.Round(elapsedTime * 100)) / 100.0f;
        float roundedBestTime = (Mathf.Round(bestTime * 100)) / 100.0f;

        if (roundedElapsedTime < roundedBestTime)
        {
            isNewRecord = true;
            bestTime = elapsedTime;
        }
    }

    /**
     * This returns the level timer's elapsed time as a string.
     * 
     * @return the level timer's elapsed time as a string.
     */
    public string GetElapsedTimeText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        int centiseconds = Mathf.FloorToInt(((elapsedTime % 1) * 100) % 100);

        return string.Format("<mspace=0.6em>{0:00}:{1:00}:{2:00}</mspace>", minutes, seconds, centiseconds);
    }

    /**
     * This returns the level timer's best time as a string.
     * 
     * @return the level timer's best time as a string.
     */
    public string GetBestTimeText()
    {
        int minutes = Mathf.FloorToInt(bestTime / 60);
        int seconds = Mathf.FloorToInt(bestTime % 60);
        int centiseconds = Mathf.FloorToInt(((bestTime % 1) * 100) % 100);

        return string.Format("<mspace=0.6em>{0:00}:{1:00}:{2:00}</mspace>", minutes, seconds, centiseconds);
    }

    /**
     * This returns if a new record has been achieved.
     */
    public bool IsNewRecord()
    {
        return isNewRecord;
    }


}
