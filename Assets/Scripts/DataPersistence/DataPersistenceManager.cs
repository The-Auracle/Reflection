using UnityEngine;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    public static DataPersistenceManager instance;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    /**
     * This starts/updates the DataPersistenceManager instance.
     */
    public void StartManager()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            instance.SaveGame();
            instance.fileName = fileName;
        }

        instance.UpdateManager();
    }

    /**
     * This updates the manager with current data.
     */
    private void UpdateManager()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();

        LoadGame();
    }

    // This function is called when the MonoBehavior will be destroyed.
    private void OnDestroy()
    {
        instance.SaveGame();
    }

    /**
     * This creates new game data for the system to manage.
     */
    public void NewGame()
    {
        this.gameData = new GameData();
    }

    /**
     * This loads new game data from a designated file for the system to manage.
     */
    public void LoadGame()
    {
        // Load any saved data from a file using data handler
        this.gameData = dataHandler.Load();

        // Create a new game if no data exists.
        if (gameData == null)
        {
            NewGame();

            Debug.Log("No SaveData found. Creating New Game.");
        }

        // Push the loaded data to all other scripts that need it.

        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
        {
            dataPersistence.LoadData(gameData);
        }
    }

    /**
     * This saves game data from the system to a designated file.
     */
    public void SaveGame()
    {
        // Pass the data to other scripts so they can update it.
        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
        {
            dataPersistence.SaveData(ref gameData);
        }

        // Save data to a file using the data handler
        dataHandler.Save(gameData);
    }

    /**
     * This method finds and returns all objects that have the IDataPersistence interface in this scene.
     * 
     * @return a list of all objects that have the IDataPersistence interface in this scene.
     */
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();
    
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
