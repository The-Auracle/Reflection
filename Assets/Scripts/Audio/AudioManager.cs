using UnityEngine;
using UnityEngine.Audio;
using static Unity.VisualScripting.Member;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioMixer audioMixer;

    /**
     * This starts/updates the MusicManager instance.
     */
    public void StartManager()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /**
     * This updates master volume to a specified level.
     *
     * @param level refers to the new volume level from -80 to 0.
     */
    public void UpdateMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20.0f);
    }

    /**
     * This updates sound effects volume to a specified level.
     *
     * @param level refers to the new volume level from -80 to 0.
     */
    public void UpdateSoundEffectsVolume(float level)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(level) * 20.0f);
    }

    /**
     * This updates music volume to a specified level.
     *
     * @param level refers to the new volume level from -80 to 0.
     */
    public void UpdateMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20.0f);
    }
}
