using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TrueSync;

public class TSPlayerCharacter : TrueSyncBehaviour, IUserInputListener
{
    // Movement settings
    [SerializeField]
    public FP jumpPower;

    [SerializeField]
    public FP verticalDeltaGravity;

    [SerializeField]
    public FP wallDeltaGravity;

    [SerializeField]
    public FP leftAndRightPower;

    [SerializeField]
    private FP maxMoveSpeed;

    [SerializeField]
    public FP jetPackPower;


    // -----------------------

    private InputManager inputManager;

    public TSRigidBody2D rigidBody { get; private set; }
    public FP defaultGravityScale { get; private set; }

    // ------ Ground checking ----------

    [SerializeField]
    public string platformTag;

    [AddTracking]
    [SerializeField]
    public FP gravityScale;

    // -------- State handling ---------
    private TSPMovementStateHandler movementStateHandler;


    /*void Awake()
    {
       
    }*/

    // Use this for initialization
    public override void OnSyncedStart()
    {
        rigidBody = GetComponent<TSRigidBody2D>();
        defaultGravityScale = gravityScale;

        movementStateHandler = new TSPMovementStateHandler(this);

        // Subscribe to local input if this is our network view object
        if (owner.Id == localOwner.Id)
        {
            inputManager = GameManager.getInstance().getInputManager();

            // Subscribe to all movement input events
            inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

            // Register the camera to follow us
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamMovement>().setTarget(gameObject);
        }


    }

    // Check for collision (called before update)
    //public void OnCollisionStay(GameObject other)
    public void OnSyncedCollisionStay(GameObject other)
    {
        //contact.
        print("COLLISION");

        //collision.GetComponent<TSCollider2D>().
        // TODO: Events
        // TODO: If we unlock Z rotation of the sprite, we need to make sure we rotate the player object to be "parallel" to the wall
        // Dont try to raycast unless we are dealing with the right object here
        /*if (collision.collider.gameObject.tag == platformTag)
        {
            print("COLLISION2");

            /*FP x1 = new FP();
            FP x2 = new FP();
            FP y1 = new FP();
            FP y2 = new FP();

            x1 = this.gameObject.transform.position.x;
            y1 = this.gameObject.transform.position.y;

            x2 = other.transform.position.x;
            y2 = other.transform.position.y;

            // Get the direction towards the object
            TSVector dir = new TSVector(x2 - x1, y2 - y1, 0);
            dir.Normalize();

            TSRay ray = new TSRay(new TSVector(x1, y1, 0), dir);

            IBody body;
            TSVector hitNormal;
            FP fraction;

            //(new TrueSync.RigidBody(null)).BoundingBox.

            TrueSync.PhysicsManager.instance.Raycast(new TSVector(x1, y1, 0), dir, null, out body, out hitNormal, out fraction);
            
            TSRigidBody2D rBody = (TSRigidBody2D)body;// ** /
            //TrueSync.Physics2D.Collision coll = new TrueSync.Physics2D.Collision();

            //other.gameObject.GetComponent<TSCollider2D>().Shape.


            //other.GetComponent<BoxCollider2D>().bounds.ClosestPoint()


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
                    movementStateHandler.setOnWall(TSPMovementStateHandler.EWallDirection.Left);
                }
                else
                {
                    movementStateHandler.setOnWall(TSPMovementStateHandler.EWallDirection.Right);
                }
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LocalLateUpdate()
    {
        // Delegate to movementStateHandler
        movementStateHandler.LateUpdate();
    }

    public override void OnSyncedUpdate()
    {
        // Delegate to movementStateHandler
        movementStateHandler.FixedUpdate();

        if (rigidBody.velocity.magnitude > maxMoveSpeed)
            rigidBody.velocity = rigidBody.velocity.normalized * maxMoveSpeed;

        this.LocalLateUpdate();
    }

    public void OnUserInputKeyHold(EInputGroup group, ICommand command)
    {
        if (command is MoveCommand)
        {
            movementStateHandler.addCommand(TSPMovementStateHandler.ECommandType.Hold, command);
        }
    }


    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (command is MoveCommand)
        {
            movementStateHandler.addCommand(TSPMovementStateHandler.ECommandType.Down, command);
        }

    }

    public void OnUserInputKeyUp(EInputGroup group, ICommand command)
    {
        if (command is MoveCommand)
        {
            movementStateHandler.addCommand(TSPMovementStateHandler.ECommandType.Up, command);
        }
    }
}