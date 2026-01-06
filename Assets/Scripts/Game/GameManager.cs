using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Managers")]
    [SerializeField] DataPersistenceManager dataPersistenceManager;
    [SerializeField] AudioManager audioManager;
    [SerializeField] SFXManager sfxManager;
    [SerializeField] MusicManager musicManager;

    // Start is called just before any of the Update methods is called for the first time.
    private void Start()
    {
        if (instance == null)
        {
            instance = this;

            StartManagers();

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            StartManagers();

            Destroy(gameObject);
        }
    }

    /**
     * This starts/updates all managers within this GameManager.
     */
    private void StartManagers()
    {
        audioManager.StartManager();
        sfxManager.StartManager();
        musicManager.StartManager();

        // Load data system after so that data can flow properly to other systems
        dataPersistenceManager.StartManager();
    }
}
