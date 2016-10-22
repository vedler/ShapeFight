using UnityEngine;
using System.Collections;
using System;

public class PlayerCharacter : MonoBehaviour, IUserInputListener {

    // Movement settings
    [SerializeField]
    private float jumpPower;

    [SerializeField]
    private float verticalDeltaGravity;

    [SerializeField]
    private float leftAndRightPower;


    // -----------------------

    private InputManager inputManager;

    public bool ready { get; private set; }

    private Rigidbody2D rigidBody;
    private float gravityScale;

    
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        gravityScale = rigidBody.gravityScale;
    }

    // Use this for initialization
    void Start ()
    {
        inputManager = GameManager.getInstance().getInputManager();

        // Subscribe to all movement input events
        inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

        // Subscribe to all shooting input events
        inputManager.subscribeToInputGroup(EInputGroup.ShootingInput, this);
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void OnUserInputKeyHold(EInputGroup group, ICommand command)
    {
        // TODO: Max velocity
        // TODO: Jetpack (when in air, holding jump will use jetpack)
        
        if (command is MoveCommand)
        {

            switch (((MoveCommand)command).control)
            {
                case EInputControls.MoveUp:
                    rigidBody.gravityScale = gravityScale - verticalDeltaGravity;
                    break;
                case EInputControls.MoveDown:
                    rigidBody.gravityScale = gravityScale + verticalDeltaGravity;
                    break;
                case EInputControls.MoveRight:
                    rigidBody.AddForce(new Vector2(leftAndRightPower, 0), ForceMode2D.Impulse);
                    break;
                case EInputControls.MoveLeft:
                    rigidBody.AddForce(new Vector2(-leftAndRightPower, 0), ForceMode2D.Impulse);
                    break;
            }
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
            switch (((MoveCommand)command).control)
            {
                case EInputControls.Jump:
                    rigidBody.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
                    break;
            }
        }

    }

    public void OnUserInputKeyUp(EInputGroup group, ICommand command)
    {
        if (command is MoveCommand)
        {

            switch (((MoveCommand)command).control)
            {
                case EInputControls.MoveUp:
                    rigidBody.gravityScale = gravityScale;
                    break;
                case EInputControls.MoveDown:
                    rigidBody.gravityScale = gravityScale;
                    break;
            }
        }
    }
}
