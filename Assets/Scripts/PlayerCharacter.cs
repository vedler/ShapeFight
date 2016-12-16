﻿using UnityEngine;
using System.Collections;
using System;

public class PlayerCharacter : Photon.MonoBehaviour, IUserInputListener {

    // Movement settings
    [SerializeField]
    public float jumpPower;

    [SerializeField]
    public float verticalDeltaGravity;

    [SerializeField]
    public float wallDeltaGravity;

    [SerializeField]
    public float leftAndRightPower;

    [SerializeField]
    private float maxMoveSpeed;

    [SerializeField]
    public float jetPackPower;


    // -----------------------

    private InputManager inputManager;
    
    public Rigidbody2D rigidBody { get; private set; }
    public float defaultGravityScale { get; private set; }
    
    // ------ Ground checking ----------
    
    [SerializeField]
    public string platformTag;

    // -------- State handling ---------
    public PMovementStateHandler movementStateHandler { get; private set; }

    public bool wasMine { get; private set; }

    // -------- Networking ----------
    private PlayerSynchronizer synchronizer;
    public long numberOfTicks { get; private set; }

    void Awake()
    {
        numberOfTicks = 0;

        rigidBody = GetComponent<Rigidbody2D>();
        defaultGravityScale = rigidBody.gravityScale;
        
        movementStateHandler = new PMovementStateHandler(this);

        if (photonView.isMine)
        {
            print("ismine");
            wasMine = true;
        }

        synchronizer = GetComponent<PlayerSynchronizer>();
    }

    // Use this for initialization
    void Start ()
    {
        // Subscribe to local input if this is our network view object
        if (wasMine)
        {
            print("wasmine");

            inputManager = GameManager.getInstance().getInputManager();

            // Subscribe to all movement input events
            inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

            // Register the camera to follow us
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamMovement>().setTarget(gameObject);
        }
    }

    // Check for collision (called before update)
    void OnCollisionStay2D(Collision2D collision)
    {
        // TODO: Events
        // TODO: If we unlock Z rotation of the sprite, we need to make sure we rotate the player object to be "parallel" to the wall
        
        /*if (collision.collider.gameObject.tag == groundTag)
        {
            movementStateHandler.setOnGround();
        }
        else */
        
        if (collision.collider.gameObject.tag == platformTag)
        {
            // neg left, 0 not touching, pos right

            // Get the vector of the contact normal
            // PS: If we are touched on both sides by the same collider, 
            //      this might make the object go "Garry's Mod" (first contact point's side might alternate every frame)
            Vector3 hit = collision.contacts[0].normal;

            // TODO: Log (most verbose mode)

            float angle = Vector3.Angle(hit, Vector3.up);

            if (angle < 80.0f)
            {
                // TODO: If jetpacking, disable?
                // The wall is "below" us, meaning we are on top of a wall and can treat it as ground
                movementStateHandler.setOnGround();
            }
            else if (angle >= 80.0f)
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
        numberOfTicks++;

        if (wasMine && !(PhotonNetwork.isMasterClient))
        {
            //synchronizer.sendInputData();
        }

        // Delegate to movementStateHandler
        movementStateHandler.FixedUpdate();

        if (wasMine && !(PhotonNetwork.isMasterClient))
        {
            //synchronizer.sendInputData();
        }

        if (rigidBody.velocity.magnitude > maxMoveSpeed)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * maxMoveSpeed;
        }
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

    public float getMaxMoveSpeed()
    {
        return maxMoveSpeed;
    }
}
