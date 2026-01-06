using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Animator animator;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip deathSoundClip;

    // Calls itself on collision with a Collision2D object.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Death"))
        {
            Death();
        }
    }

    /**
     * This triggers the player death logic.
     */
    private void Death()
    {
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.gravityScale = 0.0f;
        playerInput.enabled = false;
        animator.SetTrigger("Death");

        SFXManager.instance.PlayAudioClip(deathSoundClip, transform, 1.0f);
    }

    /**
     * This restart the current level.
     */
    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
