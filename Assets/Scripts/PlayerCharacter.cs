using UnityEngine;
using System.Collections;
using System;

public class PlayerCharacter : MonoBehaviour, IUserInputListener {

    // Movement settings
    [SerializeField]
    public float jumpPower;

    [SerializeField]
    public float verticalDeltaGravity;

    [SerializeField]
    public float wallDeltaGravity;

    [SerializeField]
    public float leftAndRightPower;

    // -----------------------

    private InputManager inputManager;
    
    public Rigidbody2D rigidBody { get; private set; }
    public float defaultGravityScale { get; private set; }
    
    // ------ Ground checking ----------
    
    [SerializeField]
    public string groundTag;

    [SerializeField]
    public string wallTag;

    // -------- State handling ---------
    private PMovementStateHandler movementStateHandler;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        defaultGravityScale = rigidBody.gravityScale;
        
        movementStateHandler = new PMovementStateHandler(this);
    }

    // Use this for initialization
    void Start ()
    {
        inputManager = GameManager.getInstance().getInputManager();

        // Subscribe to all movement input events
        inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

        // TODO: Register this object as a player in inputManager
    }

    // Check for collision (called before update)
    void OnCollisionStay2D(Collision2D collision)
    {
        // TODO: Events
        // TODO: If we unlock Z rotation of the sprite, we need to make sure we rotate the player object to be "parallel" to the wall
        
        if (collision.collider.gameObject.tag == groundTag)
        {
            movementStateHandler.setOnGround();
        }
        else if (collision.collider.gameObject.tag == wallTag)
        {
            // neg left, 0 not touching, pos right

            // Get the vector of the contact normal
            // PS: If we are touched on both sides by the same collider, 
            //      this might make the object go "Garry's Mod" (first contact point's side might alternate every frame)
            Vector3 hit = collision.contacts[0].normal;

            // TODO: Log (most verbose mode)

            float angle = Vector3.Angle(hit, Vector3.up);

            if (Mathf.Approximately(angle, 0))
            {
                // TODO: If jetpacking, disable?
                // The wall is "below" us, meaning we are on top of a wall and can treat it as ground
                movementStateHandler.setOnGround();
            }
            else if (Mathf.Approximately(angle, 90))
            {
                // TODO: Possibly instead of layer flags, we can check if we are actually on on ground with these flags (or just on top of the wall, standing - below)
                // Find which side we are touching
                Vector3 cross = Vector3.Cross(Vector3.forward, hit);
                if (cross.y > 0)
                {
                    movementStateHandler.setOnWall(PMovementStateHandler.EWallDirection.Left);
                }
                else
                {
                    movementStateHandler.setOnWall(PMovementStateHandler.EWallDirection.Right);
                }
            }
        }
    }

    // TODO: Max velocity (normalize velocity * maxSpeed)
    // TODO: Jetpack (when in air, holding jump will use jetpack)

    // Update is called once per frame
    void Update () {
        
	}

    void LateUpdate()
    {
        // Delegate to movementStateHandler
        movementStateHandler.LateUpdate();
    }

    void FixedUpdate()
    {
        // Delegate to movementStateHandler
        movementStateHandler.FixedUpdate();
    }

    public void OnUserInputKeyHold(EInputGroup group, ICommand command)
    {   
        if (command is MoveCommand)
        {
            movementStateHandler.addCommand(PMovementStateHandler.ECommandType.Hold, command);
        }
    }


    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (command is MoveCommand)
        {
            movementStateHandler.addCommand(PMovementStateHandler.ECommandType.Down, command);
        }
    }

    public void OnUserInputKeyUp(EInputGroup group, ICommand command)
    {
        if (command is MoveCommand)
        {
            movementStateHandler.addCommand(PMovementStateHandler.ECommandType.Up, command);
        }
    }
}
