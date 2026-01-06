using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDerPath;
    private string dataFileName;

    /**
     * This constructor creates a new FIleDataHandler object with the specified directory path and file name for the data.
     *
     * @param dataDirPath refers to the directory path the data is being stored.
     * @param dataFileName refers to the name of the file the data is being stored.
     */
    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDerPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    /**
     * This returns the game data from this objects designated file.
     * 
     * @return the gane data from this objects designated file.
     */
    public GameData Load()
    {
        // Use Path.Combine to account for different OS's having different path separators.
        string fullPath = Path.Combine(dataDerPath, dataFileName);

        GameData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                // Load the serialized data from the file.
                string dataToLoad = "";
                
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize the data from JSON back into the C# object.
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " +  fullPath + "\n" + e);
            }
        }

        return loadedData;
    }

    /**
     * This saves the specified game data to this objects designated file.
     * 
     * @param data refers to the game data being saved to file.
     */
    public void Save(GameData data)
    {
        // Use Path.Combine to account for different OS's having different path separators.
        string fullPath = Path.Combine(dataDerPath, dataFileName);

        try
        {
            // Create the directory the file will be written to if it doesn't already exist.
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize the C# game data object into JSON.
            string dataToStore = JsonUtility.ToJson(data, true);

            // Write the serialized data to the file.
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}
