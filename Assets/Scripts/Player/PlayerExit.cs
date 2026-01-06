using UnityEngine;

public class PlayerExit : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private LevelCompleteMenu levelCompleteMenuScript;

    [Header("Components")]
    [SerializeField] private new Transform transform;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;

    [Header("Exit")]
    [SerializeField] private Transform exitTransform;

    private bool levelComplete = false;

    private float pullForce = 50.0f;
    private float stopDistance = 0.05f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (levelComplete)
        {
            moveTowardsExit();
        }
    }

    // This gets called when a Collider2D enters the trigger zone.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!levelComplete && collision.CompareTag("Exit"))
        {
            LevelComplete();
        }
    }

    /**
     * This completes the level.
     */
    private void LevelComplete()
    {
        if (!levelComplete)
        {
            levelCompleteMenuScript.LevelComplete();
            animator.SetTrigger("Exit");
            rigidBody.gravityScale = 0.0f;

            levelComplete = true;
        }
    }

    /**
     * This opens the level complete menu.
     */
    private void OpenLevelCompleteMenu()
    {
        levelCompleteMenuScript.OpenLevelCompleteMenu();

    }

    /**
     * This moves the player towards the center of the level exit.
     */
    private void moveTowardsExit()
    {
        float distance = Vector3.Distance(exitTransform.position, transform.position);
        Vector3 direction = (exitTransform.position - transform.position).normalized;
        Vector3 perpDirection = Vector3.Cross(direction, new Vector3(0, 0, -1));

        if (distance > stopDistance)
        {
            rigidBody.AddForce(direction * pullForce);
        }
        else
        {
            rigidBody.linearVelocity = Vector3.zero;
        }
    }
}
