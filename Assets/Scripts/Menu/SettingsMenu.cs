using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour, IDataPersistence
{
    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    [Header("Menu First Selected Option")]
    [SerializeField] private GameObject mainMenuFirst;

    [Header("Sliders")]
    [SerializeField] private Slider[] sliders;

    private SerializableList<float> volumeSettings;

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        settingsMenu.SetActive(false);
    }

    /**
     * This loads the data from the best times into this Object.
     * 
     * @param data refers to the data for the curent scene.
     */
    public void LoadData(GameData data)
    {
        // Load data
        volumeSettings = data.volumeSettings;

        // Update sliders
        sliders[0].value = volumeSettings[0];
        sliders[1].value = volumeSettings[1];
        sliders[2].value = volumeSettings[2];

        // Update audio
        AudioManager.instance.UpdateMasterVolume(volumeSettings[0]);
        AudioManager.instance.UpdateSoundEffectsVolume(volumeSettings[1]);
        AudioManager.instance.UpdateMusicVolume(volumeSettings[2]);
    }

    /**
     * This saves the data from the curent scene into this Object.
     *
     * @param data refers to the data for the curent scene.
     */
    public void SaveData(ref GameData data)
    {
        // save data
        data.volumeSettings = volumeSettings;
    }

    /**
     * This goes back to the main menu.
     */
    public void Back()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
    }

    /**
     * This updates master volume to a specified level.
     *
     * @param level refers to the new volume level from -80 to 0.
     */
    public void UpdateMasterVolume(float level)
    {
        // Update data
        volumeSettings[0] = level;

        // Update audio
        AudioManager.instance.UpdateMasterVolume(level);
    }

    /**
     * This updates sound effects volume to a specified level.
     *
     * @param level refers to the new volume level from -80 to 0.
     */
    public void UpdateSoundEffectsVolume(float level)
    {
        // Update data
        volumeSettings[1] = level;

        // Update audio
        AudioManager.instance.UpdateSoundEffectsVolume(level);
    }

    /**
     * This updates music volume to a specified level.
     *
     * @param level refers to the new volume level from -80 to 0.
     */
    public void UpdateMusicVolume(float level)
    {
        // Update data
        volumeSettings[2] = level;

        // Update audio
        AudioManager.instance.UpdateMusicVolume(level);
    }
}
