using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public SerializableSortedDictionary<int, float> bestTimes;
    public SerializableList<float> volumeSettings;

    /**
     * This constructor creates a new GameData object.
     */
    public GameData()
    {
        bestTimes = new SerializableSortedDictionary<int, float>();

        volumeSettings = new SerializableList<float>();
        volumeSettings.Add(0.5f);
        volumeSettings.Add(0.5f);
        volumeSettings.Add(0.5f);
    }
}
