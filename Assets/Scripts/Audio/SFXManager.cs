using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    [SerializeField] private AudioSource SFXObject;

    /**
     * This starts/updates the SFXManager instance.
     */
    public void StartManager()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /**
     * This plays a specified sound effect at a position at a certain volume.
     * 
     * @param audioClip is the sound effect being played.
     * @param spawnTransform is the position the sound effect is spawned.
     * @param volume is the volume the sound effect is played.
     */
    public void PlayAudioClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        // Spawn in gameObject
        AudioSource audioSource = Instantiate(SFXObject, spawnTransform.position, Quaternion.identity);

        // Assign the audioClip
        audioSource.clip = audioClip;

        // Assign Volume
        audioSource.volume = volume;

        // Play Sound
        audioSource.Play();

        // Get length of audio clip
        float clipLength = audioSource.clip.length;

        // Destroy the clip after it is done playing
        Destroy(audioSource.gameObject, clipLength);

    }

    /**
     * This plays a random sound effect from a specified list at a position at a certain volume.
     * 
     * @param audioClips is the list of audio clips randomly being played.
     * @param spawnTransform is the position the sound effect is spawned.
     * @param volume is the volume the sound effect is played.
     */
    public void PlayRandomAudioClip(AudioClip[] audioClips, Transform spawnTransform, float volume)
    {
        // Assign a random index
        int rand = Random.Range(0, audioClips.Length);

        // Spawn in gameObject
        AudioSource audioSource = Instantiate(SFXObject, spawnTransform.position, Quaternion.identity);

        // Assign the audioClip
        audioSource.clip = audioClips[rand];

        // Assign Volume
        audioSource.volume = volume;

        // Play Sound
        audioSource.Play();

        // Get length of audio clip
        float clipLength = audioSource.clip.length;

        // Destroy the clip after it is done playing
        Destroy(audioSource.gameObject, clipLength);

    }
}
