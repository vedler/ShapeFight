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

    // Coroutine to reiterate until finished with loading
    IEnumerator initPlayerCharacter()
    {
        if (GameManager.getInstance() == null || !GameManager.getInstance().areManagersReady())
        {
            yield return new WaitForSeconds(0.1f);    
        }

        rigidBody = GetComponent<Rigidbody2D>();
        gravityScale = rigidBody.gravityScale;

        inputManager = GameManager.getInstance().getInputManager();

        // Subscribe to all movement input events
        inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

        ready = true;
        yield break;
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(initPlayerCharacter());
	}
	
	// Update is called once per frame
	void Update () {
        if (!ready) return;
	}

    public void OnUserInputKeyHold(EInputGroup group, ICommand command)
    {
        // TODO: Max velocity
        // TODO: Jetpack (when in air, holding jump will use jetpack)

        if (!ready) return;
        
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
    }


    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (!ready) return;

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
        if (!ready) return;
        
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
