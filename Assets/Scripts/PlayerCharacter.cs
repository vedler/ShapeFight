using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

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

    private Coroutine jetsFiring = null;

    private float jetpackFuel = 2000;
    private float maxFuel = 2000;
    private float maxHealth = 100;

    public static class FuelUsage
    {
        public static readonly float liftoff = 5f;
        public static readonly float normal = 3.5f;
        public static readonly float enhanced = 12f;
    }

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
    public PMovementStateHandler movementStateHandler { get; private set; }

    public bool wasMine { get; private set; }

    // -------- Networking ----------
    private PlayerSynchronizer synchronizer;
    public long numberOfTicks { get; private set; }
    private Text healthText;
    private Text fuelText;


    void Awake()
    {
        if (photonView.isMine)
        {
            print("ismine");
            wasMine = true;
        }

        // Subscribe to local input if this is our network view object
        if (wasMine)
        {
            // Send RPC with new random color
            SetCharacterColors(UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1));
            photonView.RPC("RemoteSetCharacterColors", PhotonTargets.AllBuffered, clr.r, clr.g, clr.b, clr.a);

            inputManager = GameManager.getInstance().getInputManager();

            // Subscribe to all movement input events
            inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

            // Register the camera to follow us
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamMovement>().setTarget(gameObject);
            healthText = GameObject.FindGameObjectWithTag("PlayerHealthTag").GetComponent<Text>();
            fuelText = GameObject.FindGameObjectWithTag("PlayerFuelTag").GetComponent<Text>();
        }
        //else
        //    SetCharacterColors();

        jetpack = transform.GetChild(1).gameObject;
        transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().Stop();
        stopJets();

        // --------

        numberOfTicks = 0;

        rigidBody = GetComponent<Rigidbody2D>();
        defaultGravityScale = rigidBody.gravityScale;
        
        movementStateHandler = new PMovementStateHandler(this);

        synchronizer = GetComponent<PlayerSynchronizer>();
    }

    // Use this for initialization
    void Start() { 
    
        
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
        
        // Delegate to movementStateHandler
        movementStateHandler.FixedUpdate();
        
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

    // Juta's jetpack code

    public float getJetpackFuel()
    {
        return jetpackFuel;
    }

    public bool hasEnoughFuelFor(PMovementStateHandler.EJetpackUsage usage)
    {
        switch (usage)
        {
            case PMovementStateHandler.EJetpackUsage.Liftoff:
                if (jetpackFuel >= FuelUsage.liftoff)
                {
                    return true;
                }
                return false;
            case PMovementStateHandler.EJetpackUsage.Normal:
                if (jetpackFuel >= FuelUsage.normal)
                {
                    return true;
                }
                return false;
            case PMovementStateHandler.EJetpackUsage.Enhanced:
                if (jetpackFuel >= FuelUsage.enhanced)
                {
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    public void reduceFuel()
    {
        jetpackFuel -= 5f;
        if (wasMine)
        {
            updateFuelText();
        }
    }

    public void reduceFuel(float f)
    {
        jetpackFuel -= f;
        if (wasMine)
        {
            updateFuelText();
        }
    }

    public void reduceFuel(PMovementStateHandler.EJetpackUsage usage)
    {
        switch (usage)
        {
            case PMovementStateHandler.EJetpackUsage.Liftoff:
                reduceFuel(FuelUsage.liftoff);
                break;
            case PMovementStateHandler.EJetpackUsage.Normal:
                reduceFuel(FuelUsage.normal);
                break;
            case PMovementStateHandler.EJetpackUsage.Enhanced:
                reduceFuel(FuelUsage.enhanced);
                break;
            default:
                return;
        }
    }

    public void restoreFuelABit()
    {
        if (jetpackFuel < maxFuel)
        {
            jetpackFuel = Mathf.Min(jetpackFuel + 7f, maxFuel);

            if (wasMine)
            {
                updateFuelText();
            }
        }
    }

    private void updateFuelText()
    {
        fuelText.text = string.Format("Fuel:    {0:0.0%}", Mathf.Max(jetpackFuel/maxFuel, 0));
    }

    private IEnumerator jetLifeCycle()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(.3f);
        }
        jetpack.GetComponent<ParticleSystem>().Stop();
        jetsFiring = null;
    }

    public void fireJets()
    {
        if (jetsFiring != null)
        {
            return;
        }
        //PlayerGroundState.nullifySoundCount();
        jetpack.GetComponent<ParticleSystem>().Play();
        
        jetsFiring = StartCoroutine(jetLifeCycle());
    }

    public void stopJets()
    {
        if (jetsFiring != null)
        {
            StopCoroutine(jetsFiring);
            jetsFiring = null;
        }
        jetpack.GetComponent<ParticleSystem>().Stop();
    }

    public void rotateJetpack()
    {
        Quaternion rotateTo = Quaternion.Euler(0, 0, 180);
        jetpack.transform.rotation = Quaternion.Slerp(jetpack.transform.rotation, rotateTo, Time.deltaTime * 1.2f);
    }

    public void rotateJetpack(float deg)
    {
        //reduceFuel(3.5f);
        Quaternion rotateTo = Quaternion.Euler(0, 0, deg);
        jetpack.transform.rotation = Quaternion.Slerp(jetpack.transform.rotation, rotateTo, Time.deltaTime * .9f);
    }

    public void burst()
    {
        //reduceFuel(12);
        Quaternion rotateTo = Quaternion.Euler(0, 0, 180);
        jetpack.transform.rotation = Quaternion.Slerp(jetpack.transform.rotation, rotateTo, Time.deltaTime * 1.9f);
    }

    public void getHit(float dam)
    {
        GetComponent<AudioSource>().Play();
        health -= dam;
        StartCoroutine(damageAnim());
        float ratio = health / maxHealth;
        healthText.text = string.Format("Health: {0:0.0%}", ratio);
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

    [PunRPC]
    public void RemoteSetCharacterColors(float r, float g, float b, float a, PhotonMessageInfo info)
    {
        SetCharacterColors(new Color(r, g, b, a));
    }

    [PunRPC]
    public void RequestCharacterColors(float r, float g, float b, float a, PhotonMessageInfo info)
    {
        SetCharacterColors(new Color(r, g, b, a));
        //photonView.RPC("RemoteSetCharacterColors", PhotonTargets., clr.r, clr.g, clr.b, clr.a);
    }

    public void SetCharacterColors(Color color)
    {
        clr = color;
        GetComponent<SpriteRenderer>().color = clr;
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = clr;
        transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().startColor = clr;
    }

    private void SetCharacterColors()
    {
        GetComponent<SpriteRenderer>().color = clr;
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = clr;
        transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().startColor = clr;
    }
}


