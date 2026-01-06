using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenu : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] PauseMenu PauseMenuScript;
    [SerializeField] private LevelCompleteMenu levelCompleteMenuScript;

    private bool menuOpenInput = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        // Perform Player Menu logic
        Pause();

        // Reset inputs
        menuOpenInput = false;
    }

    /**
     * This pauses the game based on player controls.
     */
    private void Pause()
    {
        if (menuOpenInput)
        {
            PauseMenuScript.Pause();
        }
    }

    /**
     * This recieves player's menu open input.
     * 
     * @param context is the context in which we're recieving player inputs.
     */
    public void ReceiveMenuOpenInput(InputAction.CallbackContext context)
    {
        menuOpenInput = menuOpenInput || context.performed;
    }
}
