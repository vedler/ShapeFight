using UnityEngine;
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

    [SerializeField]
    public GameObject jetpack;

    private Coroutine jetsFiring;

    private float jetpackFuel = 2000;

    [SerializeField]
    private float health = 100;

    private Color clr;


    // -----------------------

    private InputManager inputManager;
    
    public Rigidbody2D rigidBody { get; private set; }
    public float defaultGravityScale { get; private set; }
    
    // ------ Ground checking ----------
    
    [SerializeField]
    public string platformTag;

    // -------- State handling ---------
    private PMovementStateHandler movementStateHandler;

    
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        defaultGravityScale = rigidBody.gravityScale;
        
        movementStateHandler = new PMovementStateHandler(this);
    }

    // Use this for initialization
    void Start() { 
    
        // Subscribe to local input if this is our network view object
        if (photonView.isMine)
        {
            clr = UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1);
            GetComponent<SpriteRenderer>().color = clr;
            jetpack = transform.GetChild(1).gameObject;
            inputManager = GameManager.getInstance().getInputManager();

            // Subscribe to all movement input events
            inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

            // Register the camera to follow us
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamMovement>().setTarget(gameObject);
        }
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = clr;
        transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().startColor = clr;
        transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().Stop();
        stopJets();
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

        if (rigidBody.velocity.magnitude > maxMoveSpeed)
            rigidBody.velocity = rigidBody.velocity.normalized * maxMoveSpeed;
    }

    public void OnUserInputKeyHold(EInputGroup group, ICommand command)
    {   
        if (command is MoveCommand)
        {
            movementStateHandler.addCommand(PMovementStateHandler.ECommandType.Hold, command);
        }
        /*  Tulistamiseks vajalik
        if (command is ShootingCommand)
        {

            switch (((ShootingCommand)command).control)
            {

                case EInputControls.ShootMain:
                    vajalik kood siia
                    break;
                case EInputControls.ShootAlt:
                    vajalik kood siia
                    break;
            }
        } */
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

    // Juta's jetpack code

    public float getJetpackFuel()
    {
        return jetpackFuel;
    }

    public void reduceFuel()
    {
        jetpackFuel -= 5f;
    }

    public void reduceFuel(float f)
    {
        jetpackFuel -= f;
    }

    public void restoreFuelABit()
    {
        jetpackFuel += 7f;
    }

    private IEnumerator jetLifeCycle()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(.3f);
        }
        jetpack.GetComponent<ParticleSystem>().Stop();
    }

    public void fireJets()
    {
        //PlayerGroundState.nullifySoundCount();
        jetpack.GetComponent<ParticleSystem>().Play();

        reduceFuel();
        jetsFiring = StartCoroutine(jetLifeCycle());
    }

    public void stopJets()
    {
        if (jetsFiring != null)
            StopCoroutine(jetsFiring);
        jetpack.GetComponent<ParticleSystem>().Stop();
    }

    public void rotateJetpack()
    {
        Quaternion rotateTo = Quaternion.Euler(0, 0, 180);
        jetpack.transform.rotation = Quaternion.Slerp(jetpack.transform.rotation, rotateTo, Time.deltaTime * 1.2f);
    }

    public void rotateJetpack(float deg)
    {
        reduceFuel(3.5f);
        Quaternion rotateTo = Quaternion.Euler(0, 0, deg);
        jetpack.transform.rotation = Quaternion.Slerp(jetpack.transform.rotation, rotateTo, Time.deltaTime * .9f);
    }

    public void burst()
    {
        reduceFuel(12);
        Quaternion rotateTo = Quaternion.Euler(0, 0, 180);
        jetpack.transform.rotation = Quaternion.Slerp(jetpack.transform.rotation, rotateTo, Time.deltaTime * 1.9f);
    }

    public void getHit(float dam)
    {
        GetComponent<AudioSource>().Play();
        health -= dam;
        StartCoroutine(damageAnim());
    }

    public IEnumerator damageAnim()
    {
        for (int i = 0; i < 6; i++)
        {
            if (i % 6 == 0)
                GetComponent<SpriteRenderer>().color = Color.red;
            else if (i % 6 == 3)
                GetComponent<SpriteRenderer>().color = Color.white;
            else
                GetComponent<SpriteRenderer>().color = clr;
            yield return new WaitForSeconds(.1f);
        }
    }
}
