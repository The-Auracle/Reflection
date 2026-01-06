using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [SerializeField] private AudioSource source;

    /**
     * This starts/updates the MusicManager instance.
     */
    public void StartManager()
    {
        if (instance == null)
        {
            instance = this;

            // Start action
            instance.PlayMusic();
        }
        else
        {
            if (instance.source.clip != source.clip)
            {
                instance.source.clip = source.clip;

                // Start action
                instance.PlayMusic();
            }
        }
    }

    /**
     * This plays the MusicManager's music.
     */
    public void PlayMusic()
    {
        source.Play();
    }

    /**
     * This pauses the MusicManager's music.
     */
    public void PauseMusic()
    {
        source.Pause();
    }
}
