using UnityEngine;

public interface IDataPersistence
{
    /**
     * This loads data to the script from a GameData object.
     * 
     * @param data refers to the GameData object data is being loaded from.
     */
    public void LoadData(GameData data);

    /**
     * This saves data to a GameData object from the script.
     * 
     * @param data refers to the GameData object data is being saved to.
     */
    public void SaveData(ref GameData data);
}
