using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMirror : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private PlayerMovement playerMovementScript;

    [Header("Camera")]
    [SerializeField] private new CinemachineCamera camera;
    [SerializeField] private CinemachineFollow cameraFollow;
    [SerializeField] private CinemachineConfiner2D cameraConfiner;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Mirror Image Components")]
    [SerializeField] private GameObject mirrorImageObject;
    [SerializeField] private Transform mirrorImageTransform;

    [Header("Mirror Collider")]
    [SerializeField] private CompositeCollider2D mirrorCollider;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip mirrorSoundClip;

    private bool isMirroring = false;
    private bool isCameraRotating = false;

    private float cameraConfinerSlowingDistance;
    private Vector3 cameraFollowDamping;

    private float mirrorCounter;
    private float mirrorTime = 1f;

    private float cameraOriginalRotation;
    private float cameraMirrorRotation;

    private float playerOriginalPosition;
    private float playerMirrorPosition;

    private bool mirrorInput;

    private Direction gravityDirection = Direction.down;

    private enum Direction
    {
        up, down
    }

    // FixedUpdate is called once per fixed time interval.
    private void FixedUpdate()
    {
        // Perform flip logic
        if (!isMirroring)
        {
            Mirror();
        }

        // Reset inputs
        mirrorInput = false;
    }

    // Update is called once every frame.
    private void Update()
    {
        // Perform mirror image model logic
        MirrorImageTransform();

        // Perform mirror player movement logic
        MirrorMovement();
        MirrorCamera();
    }

    /**
     * This starts the action of mirroring the player from one side of the mirror to the other.
     */
    private void Mirror()
    {
        // Flips collisions
        if (IsMirrorable() && mirrorInput)
        {
            // Set variable flags
            isMirroring = true;
            isCameraRotating = false;

            // Camera conditions
            cameraConfinerSlowingDistance = cameraConfiner.SlowingDistance;
            cameraFollowDamping = cameraFollow.TrackerSettings.PositionDamping;

            cameraConfiner.SlowingDistance = 0f;
            cameraFollow.TrackerSettings.PositionDamping = Vector3.zero;

            cameraOriginalRotation = camera.transform.rotation.eulerAngles.z;
            cameraMirrorRotation = cameraOriginalRotation + 180;

            // Player conditions
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = 0f;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 200f / 255f);

            playerOriginalPosition = transform.position.y;
            playerMirrorPosition = MirrorTransform().y;

            // Play Audio
            SFXManager.instance.PlayAudioClip(mirrorSoundClip, transform, 1.0f);

            // Set timer for mirror transformation
            mirrorCounter = 0f;

        }
    }

    /**
     * This moves the player from one side of the mirror to the other.
     */
    private void MirrorMovement()
    {
        if (isMirroring && !isCameraRotating)
        {
            // Set Mirror Timer
            mirrorCounter += Time.deltaTime;

            // Transform Player Position
            transform.position = new Vector3(transform.position.x, Mathf.SmoothStep(playerOriginalPosition, playerMirrorPosition, mirrorCounter / mirrorTime));

            // Set conditions for player after full mirror movement
            if (transform.position.y == playerMirrorPosition)
            {
                // Set variable flags
                isCameraRotating = true;

                // Camera Conditions
                cameraConfiner.SlowingDistance = cameraConfinerSlowingDistance;
                cameraFollow.TrackerSettings.PositionDamping = cameraFollowDamping;

                // Player Conditions
                if (gravityDirection == Direction.down)
                {
                    rigidBody.gravityScale = -1;
                    gravityDirection = Direction.up;
                }
                else
                {
                    rigidBody.gravityScale = 1;
                    gravityDirection = Direction.down;
                }

                rigidBody.SetRotation(rigidBody.rotation + 180);

                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);

                // Reset Mirror Counter
                mirrorCounter = 0f;
            }
        }
    }

    /**
     * This rotates the camera 180 degrees smoothly.
     */
    private void MirrorCamera()
    {
        if (isCameraRotating)
        {
            // Set Mirror Timer
            mirrorCounter += Time.deltaTime;

            camera.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.SmoothStep(cameraOriginalRotation, cameraMirrorRotation, mirrorCounter / mirrorTime));

            if ((mirrorCounter / mirrorTime) >= 1 && Mathf.Abs(camera.transform.rotation.eulerAngles.z % 180) < 0.1)
            {
                isMirroring = false;
                isCameraRotating = false;
            }
        }
        
    }

    /**
     * This sets the mirror image model's transform to the other side of the mirror tile.
     */
    private void MirrorImageTransform()
    {
        Vector3 transform = MirrorTransform();

        if (!isMirroring && transform != Vector3.one)
        { 
            mirrorImageObject.SetActive(true);

            mirrorImageTransform.position = transform;
        }
        else
        {
            mirrorImageObject.SetActive(false);
        }
    }

    /**
     * This returns the transform the player would be if moved to the other side of the mirror.
     * 
     * @return the transform the player would be if moved to the other side of the mirror.
     *         If player cannot be mirrored, return Vector3.one.
     */
    private Vector3 MirrorTransform()
    {   
        if (IsMirrorable())
        {
            // This gets the location of where the player will be on the other side of the mirror.
            List<Bounds> shapeBounds = new List<Bounds>();
            mirrorCollider.GetShapeBounds(shapeBounds, true, true);

            for (int i = 0; i < shapeBounds.Count; i++)
            {
                Bounds shape = shapeBounds[i];
                float positionLeniancy = 1.0f;

                float playerX = transform.position.x;
                float playerY = transform.position.y;

                if (shape.center.x - (shape.extents.x + positionLeniancy) <= playerX && playerX <= shape.center.x + (shape.extents.x + positionLeniancy))
                {
                    if (shape.center.y - (shape.extents.y + positionLeniancy) <= playerY && playerY <= shape.center.y + (shape.extents.y + positionLeniancy))
                    {
                        float flipTileHeight = shape.extents.y * 2 + 1;

                        return transform.position + flipTileHeight * GetGravityDirection();
                    }
                }
            }
        }

        return Vector3.one;

    }
    /**
     * This checks if the player is on a mirror tile.
     * 
     * @returns true if the player is on a mirror tile; else false.
     */
    private bool IsMirrorable()
    {
        // How much off the flip tile counts as still being flippable.
        float boxSizeX = boxCollider.size.x / 4;
        Vector2 boxSize = new Vector2(boxSizeX, playerMovementScript.GetCollisionBoxDepth());

        RaycastHit2D boxCastLeft = Physics2D.BoxCast(boxCollider.bounds.center - new Vector3((boxCollider.bounds.extents.x + boxSizeX) / 2, 0), boxSize, 0.0f, GetGravityDirection(), boxCollider.bounds.extents.y, playerMovementScript.GetGroundCollisionMask());
        RaycastHit2D boxCastRight = Physics2D.BoxCast(boxCollider.bounds.center + new Vector3((boxCollider.bounds.extents.x + boxSizeX) / 2, 0), boxSize, 0.0f, GetGravityDirection(), boxCollider.bounds.extents.y, playerMovementScript.GetGroundCollisionMask());

        // Check if standing within a certain distance of the flip tile
        bool isCollided = false;

        if (boxCastLeft.collider != null && boxCastRight.collider != null)
        {
            isCollided = boxCastLeft.collider.CompareTag("Flip Tile") && boxCastRight.collider.CompareTag("Flip Tile");
        }

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (isCollided)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, GetGravityDirection().y * boxCollider.bounds.extents.y + playerMovementScript.GetCollisionBoxDepth()), new Vector3(2 * boxCollider.bounds.extents.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, GetGravityDirection().y * boxCollider.bounds.extents.y - playerMovementScript.GetCollisionBoxDepth()), new Vector3(2 * boxCollider.bounds.extents.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, GetGravityDirection().y * boxCollider.bounds.extents.y + playerMovementScript.GetCollisionBoxDepth()), new Vector3(0, -2 * playerMovementScript.GetCollisionBoxDepth()), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x, GetGravityDirection().y * boxCollider.bounds.extents.y + playerMovementScript.GetCollisionBoxDepth()), new Vector3(0, -2 * playerMovementScript.GetCollisionBoxDepth()), rayColor);

        return isCollided;
    }

    /**
     * This returns if the player is mirroring or not.
     * 
     * @return true if player is mirroring; else false.
     */
    public bool IsMirroring()
    {
        return isMirroring; 
    }

    /**
     * This returns the vector of gravity's current direction.
     * 
     * @return the vector of gravity's current direction.
     */
    public Vector3 GetGravityDirection()
    {
        Vector3 direction;

        if (gravityDirection == Direction.down)
        {
            direction = Vector3.down;
        }
        else
        {
            direction = Vector3.up;
        }

        return direction;
    }

    /**
     * This recieves player's mirror input
     * 
     * @param context is the context in which we're recieving player inputs.
     */
    public void ReceiveMirrorInput(InputAction.CallbackContext context)
    {
        mirrorInput = mirrorInput || context.performed;
    }
}
