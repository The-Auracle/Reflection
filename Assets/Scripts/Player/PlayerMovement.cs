using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private PlayerMirror playerMirrorScript;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Collisions")]
    [SerializeField] private LayerMask groundCollisionMask;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] jumpSoundClips;
    [SerializeField] private AudioClip landSoundClip;
    [SerializeField] private AudioClip slideSoundClip;

    private float horizontalInput;
    private bool jumpStartInput;
    private bool jumpEndInput;

    private bool isGrounded = true;

    private float collisionBoxDepth = 0.05f;
    private float collisionCornerBoxDepth = 0.3f;

    private float verticalSpeed;
    private float maxSpeed = 6.0f;
    private float speedAcceleration = 25.0f;
    private float speedDecelFactor = 0.35f;

    private float jumpVelocity = 7.0f;
    private float shortHopFactor = 0.5f;
    private float maxFallSpeed = 10.0f;

    private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    private float wallJumpBufferTime = 0.1f;
    private float wallJumpBufferCounter;

    private Vector2 wallJumpVelocity = new Vector2(6f, 7f);
    private float wallSlideSpeed = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMirrorScript = GetComponent<PlayerMirror>();
    }

    // FixedUpdate is called every FixedUpdate defined interval.
    void FixedUpdate()
    {
        // Perform player logic
        if (!playerMirrorScript.IsMirroring())
        {
            Move();
            Jump();
            Grounded();
            WallSlide();
            WallJump();

            ClampFallSpeed();
            CatchCeilingCorner();
            CatchWallCorner();
        }

        // Reset inputs
        jumpStartInput = false;
        jumpEndInput = false;
    }

    /**
     * This updates the player's horizontal movement.
     */
    private void Move()
    {
        // Accelerate movement with player input
        if (horizontalInput != 0.0f)
        {
            rigidBody.AddForceX(horizontalInput * speedAcceleration * -playerMirrorScript.GetGravityDirection().y);

            rigidBody.linearVelocityX = Mathf.Clamp(rigidBody.linearVelocityX, -maxSpeed, maxSpeed);
        }
        // Decelerate movement without player input
        else
        {
            if (Mathf.Abs(rigidBody.linearVelocityX) < 0.1f)
            {
                rigidBody.linearVelocityX = 0.0f;
            }
            else
            {
                float decelAmount = -rigidBody.linearVelocityX * speedAcceleration * speedDecelFactor;

                rigidBody.AddForceX(decelAmount);
            }
        }
    }

    /**
     * This handles the player's jump movement
     */
    private void Jump()
    {
        // Coyote Time setting
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump Buffer setting
        if (jumpStartInput)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Full hop
        if (coyoteTimeCounter > 0.0f && jumpStartInput)
        {
            rigidBody.linearVelocityY = -jumpVelocity * playerMirrorScript.GetGravityDirection().y;

            coyoteTimeCounter = 0.0f;
            jumpBufferCounter = 0.0f;

            jumpStartInput = false;

            SFXManager.instance.PlayRandomAudioClip(jumpSoundClips, transform, 1.0f);
        }

        // Short hop
        if ((-rigidBody.linearVelocityY * playerMirrorScript.GetGravityDirection().y > 0) && jumpEndInput)
        {
            rigidBody.linearVelocityY = shortHopFactor * rigidBody.linearVelocityY;
        }
    }
    
    /**
     * This does any action needed when the player is grounded for the first time after being in the air.
     */
    private void Grounded()
    {
        if (!isGrounded && IsGrounded())
        {
            isGrounded = true;

            float volume = Mathf.Abs(verticalSpeed / maxSpeed);

            if (volume > 0.8f)
            {
                SFXManager.instance.PlayAudioClip(landSoundClip, transform, volume);
            }
            
        }
        else if (!IsGrounded())
        {
            isGrounded = false;

            verticalSpeed = rigidBody.linearVelocityY;
        }
    }

    /**
     * This handles the wall jump movement.
     */
    private void WallJump()
    {
        if (jumpStartInput)
        {
            wallJumpBufferCounter = wallJumpBufferTime;
        }
        else
        {
            wallJumpBufferCounter -= Time.deltaTime;
        }

        if (!IsGrounded() && wallJumpBufferCounter > 0.0f)
        {
            if (IsWalledLeft())
            {
                rigidBody.linearVelocityX = wallJumpVelocity.x;
                rigidBody.linearVelocityY = -wallJumpVelocity.y * playerMirrorScript.GetGravityDirection().y;

                SFXManager.instance.PlayRandomAudioClip(jumpSoundClips, transform, 1.0f);
            }
            else if (IsWalledRight())
            {
                rigidBody.linearVelocityX = -wallJumpVelocity.x;
                rigidBody.linearVelocityY = -wallJumpVelocity.y * playerMirrorScript.GetGravityDirection().y;

                SFXManager.instance.PlayRandomAudioClip(jumpSoundClips, transform, 1.0f);
            }
        }
    }

    /**
     * This handles the wall slide movement.
     */
    private void WallSlide()
    {
        if ((IsWalledLeft() || IsWalledRight()) && !IsGrounded() && horizontalInput != 0.0f)
        {
            if (playerMirrorScript.GetGravityDirection() == Vector3.down)
            {
                rigidBody.linearVelocityY = Mathf.Clamp(rigidBody.linearVelocityY, -wallSlideSpeed, rigidBody.linearVelocityY);
            }
            else
            {
                rigidBody.linearVelocityY = Mathf.Clamp(rigidBody.linearVelocityY, rigidBody.linearVelocityY, wallSlideSpeed);
            }

            //SFXManager.instance.PlayAudioClip(slideSoundClip, transform, 1.0f);
        }
    }

    /**
     * This clamps the fall speed.
     */
    private void ClampFallSpeed()
    {
        if (playerMirrorScript.GetGravityDirection() == Vector3.down)
        {
            rigidBody.linearVelocityY = Mathf.Clamp(rigidBody.linearVelocityY, -maxFallSpeed, rigidBody.linearVelocityY);
        }
        else
        {
            rigidBody.linearVelocityY = Mathf.Clamp(rigidBody.linearVelocityY, rigidBody.linearVelocityY, maxFallSpeed);
        }

    }

    /**
     * This catches and moves the player off a ceiling corner if they hit a corner in mid-air.
     */
    private void CatchCeilingCorner()
    {
        while (!IsGrounded() && !IsWalledRight() && IsBumpedCeilingCornerLeft())
        {
            transform.position = transform.position + new Vector3(0.01f, 0f);
            Physics2D.SyncTransforms();
        }

        while (!IsGrounded() && !IsWalledLeft() && IsBumpedCeilingCornerRight())
        {
            transform.position = transform.position - new Vector3(0.01f, 0f);
            Physics2D.SyncTransforms();
        }
    }

    /**
     * This catches and moves the player up a wall corner if they hit one while falling.
     */
    private void CatchWallCorner()
    {
        while (horizontalInput < 0.0f && -rigidBody.linearVelocityY * playerMirrorScript.GetGravityDirection().y <= 0.0f && IsBumpedWallCornerLeft())
        {
            transform.position = transform.position - new Vector3(0f, 0.01f) * playerMirrorScript.GetGravityDirection().y;
            Physics2D.SyncTransforms();
        }

        while (horizontalInput > 0.0f && -rigidBody.linearVelocityY * playerMirrorScript.GetGravityDirection().y <= 0.0f && IsBumpedWallCornerRight())
        {
            transform.position = transform.position - new Vector3(0f, 0.01f) * playerMirrorScript.GetGravityDirection().y;
            Physics2D.SyncTransforms();
        }
    }

    /**
     * This checks if the player is grounded.
     * 
     * @return true when the player is on the ground; else false.
     */
    private bool IsGrounded()
    {
        // How much off the ground counts as still being grounded
        Vector2 boxSize = new Vector2(boxCollider.size.x, collisionBoxDepth);

        RaycastHit2D boxCastGround = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0.0f, playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y, groundCollisionMask);

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (boxCastGround.collider != null)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y + collisionBoxDepth), new Vector3(2 * boxCollider.bounds.extents.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y - collisionBoxDepth), new Vector3(2 * boxCollider.bounds.extents.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y + collisionBoxDepth), new Vector3(0, -2 * collisionBoxDepth), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y + collisionBoxDepth), new Vector3(0, -2 * collisionBoxDepth), rayColor

        return boxCastGround.collider != null;
    }

    /***
     * This checks if the player is touching a wall to their left.
     * 
     * @return true when the player is touching a wall to their left; else false.
     */
    private bool IsWalledLeft()
    {
        // How much off the wall counts as still being walled
        Vector2 boxSize = new Vector2(collisionBoxDepth, boxCollider.size.y);

        RaycastHit2D boxCastWall = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0.0f, Vector2.left, boxCollider.bounds.extents.x, groundCollisionMask);

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (boxCastWall.collider != null)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-(boxCollider.bounds.extents.x - collisionBoxDepth), boxCollider.bounds.extents.y), new Vector3(2 * collisionBoxDepth, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-(boxCollider.bounds.extents.x - collisionBoxDepth), -boxCollider.bounds.extents.y), new Vector3(2 * collisionBoxDepth, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-(boxCollider.bounds.extents.x - collisionBoxDepth), boxCollider.bounds.extents.y), new Vector3(0, -2 * boxCollider.bounds.extents.y), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-(boxCollider.bounds.extents.x + collisionBoxDepth), boxCollider.bounds.extents.y), new Vector3(0, -2 * boxCollider.bounds.extents.y), rayColor);

        return boxCastWall.collider != null;
    }

    /***
     * This checks if the player is touching a wall to their left.
     * 
     * @return true when the player is touching a wall to their left; else false.
     */
    private bool IsWalledRight()
    {
        // How much off the wall counts as still being walled
        Vector2 boxSize = new Vector2(collisionBoxDepth, boxCollider.size.y);

        RaycastHit2D boxCastWall = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0.0f, Vector2.right, boxCollider.bounds.extents.x, groundCollisionMask);

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (boxCastWall.collider != null)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - collisionBoxDepth, boxCollider.bounds.extents.y), new Vector3(2 * collisionBoxDepth, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - collisionBoxDepth, -boxCollider.bounds.extents.y), new Vector3(2 * collisionBoxDepth, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - collisionBoxDepth, boxCollider.bounds.extents.y), new Vector3(0, -2 * boxCollider.bounds.extents.y), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x + collisionBoxDepth, boxCollider.bounds.extents.y), new Vector3(0, -2 * boxCollider.bounds.extents.y), rayColor);

        return boxCastWall.collider != null;
    }

    /**
     * This checks if the player is bumping a ceiling corner to their left.
     * 
     * @return true when the player is bumping a ceiling corner to their left; else false.
     */
    private bool IsBumpedCeilingCornerLeft()
    {
        Vector2 boxSize = new Vector2(collisionCornerBoxDepth, 2 * collisionBoxDepth);

        RaycastHit2D boxCastCorner = Physics2D.BoxCast(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - collisionCornerBoxDepth / 2, 0), boxSize, 0.0f, -playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y, groundCollisionMask);
        RaycastHit2D boxCastNotCorner = Physics2D.BoxCast(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x - collisionCornerBoxDepth * 3 / 2, 0), boxSize, 0.0f, -playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y, groundCollisionMask);

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (boxCastCorner.collider != null && boxCastNotCorner.collider == null)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y + boxSize.y)), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y - boxSize.y)), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y + boxSize.y)), new Vector3(0, -boxSize.y), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x + boxSize.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y + boxSize.y)), new Vector3(0, -boxSize.y), rayColor);

        return boxCastCorner.collider != null && boxCastNotCorner.collider == null;
    }

    /**
     * This checks if the player is bumping a ceiling corner to their right.
     * 
     * @return true when the player is bumping a ceiling corner to their right; else false.
     */
    private bool IsBumpedCeilingCornerRight()
    {
        Vector2 boxSize = new Vector2(collisionCornerBoxDepth, 2 * collisionBoxDepth);

        RaycastHit2D boxCastCorner = Physics2D.BoxCast(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x / 2, 0), boxSize, 0.0f, -playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y, groundCollisionMask);
        RaycastHit2D boxCastNotCorner = Physics2D.BoxCast(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x * 3 / 2, 0), boxSize, 0.0f, -playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y, groundCollisionMask);

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (boxCastCorner.collider != null && boxCastNotCorner.collider == null)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y + boxSize.y)), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y - boxSize.y)), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y + boxSize.y)), new Vector3(0, -boxSize.y), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x, -playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y + boxSize.y)), new Vector3(0, -boxSize.y), rayColor);

        return boxCastCorner.collider != null && boxCastNotCorner.collider == null;
    }

    /**
     * This checks if the player is bumping a wall corner to their left.
     * 
     * @return true when the player is bumping a wall corner to their left; else false.
     */
    private bool IsBumpedWallCornerLeft()
    {
        Vector2 boxSize = new Vector2(2 * collisionBoxDepth, collisionCornerBoxDepth);

        RaycastHit2D boxCastCorner = Physics2D.BoxCast(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, 0), boxSize, 0.0f, playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y - boxSize.y / 2, groundCollisionMask);
        RaycastHit2D boxCastNotCorner = Physics2D.BoxCast(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x, 0), boxSize, 0.0f, playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y - boxSize.y * 3 / 2, groundCollisionMask);

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (boxCastCorner.collider != null && boxCastNotCorner.collider == null)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x - boxSize.x / 2, playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y - boxSize.y)), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x - boxSize.x / 2, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x - boxSize.x / 2, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y), new Vector3(0, boxSize.y), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(-boxCollider.bounds.extents.x + boxSize.x / 2, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y), new Vector3(0, boxSize.y), rayColor);

        return boxCastCorner.collider != null && boxCastNotCorner.collider == null;
    }

    /**
     * This checks if the player is bumping a wall corner to their left.
     * 
     * @return true when the player is bumping a wall corner to their left; else false.
     */
    private bool IsBumpedWallCornerRight()
    {
        Vector2 boxSize = new Vector2(2 * collisionBoxDepth, collisionCornerBoxDepth);

        RaycastHit2D boxCastCorner = Physics2D.BoxCast(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x, 0), boxSize, 0.0f, playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y - boxSize.y / 2, groundCollisionMask);
        RaycastHit2D boxCastNotCorner = Physics2D.BoxCast(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x, 0), boxSize, 0.0f, playerMirrorScript.GetGravityDirection(), boxCollider.bounds.extents.y - boxSize.y * 3 / 2, groundCollisionMask);

        ////Debug raycasts
        //Color rayColor;

        //// Green when touching ground
        //if (boxCastCorner.collider != null && boxCastNotCorner.collider == null)
        //{
        //    rayColor = Color.green;
        //}
        //// Red when not touching ground
        //else
        //{
        //    rayColor = Color.red;
        //}

        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x / 2, playerFlipScript.GetGravityDirection().y * (boxCollider.bounds.extents.y - boxSize.y)), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x / 2, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y), new Vector3(boxSize.x, 0), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x - boxSize.x / 2, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y), new Vector3(0, boxSize.y), rayColor);
        //Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x + boxSize.x / 2, playerFlipScript.GetGravityDirection().y * boxCollider.bounds.extents.y), new Vector3(0, boxSize.y), rayColor);

        return boxCastCorner.collider != null && boxCastNotCorner.collider == null;
    }

    /**
     * This returns the ground collision mask.
     * 
     * @return ground collision mask.
     */
    public LayerMask GetGroundCollisionMask()
    {
        return groundCollisionMask;
    }

    /**
     * This returns the collision box depth of the player.
     * 
     * @return collision box depth.
     */
    public float GetCollisionBoxDepth()
    {
        return collisionBoxDepth; 
    }

    /**
     * This recieves player's movement input.
     * 
     * @param context is the context in which we're recieving player inputs.
     */
    public void ReceiveMovementInput(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
    }

    /**
     * This recieves player's jump input.
     * 
     * @param context is the context in which we're recieving player inputs.
     */
    public void ReceiveJumpInput(InputAction.CallbackContext context)
    {
        jumpStartInput = jumpStartInput || context.performed;

        jumpEndInput = jumpEndInput || context.canceled;
    }
        
}