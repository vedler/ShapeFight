using UnityEngine;
using System.Collections;
using System;

public class WeaponSpawner : MonoBehaviour, IUserInputListener {

    public GameObject newRocket;
    public Rigidbody2D rigidBody;
    // -----------------------

    private InputManager inputManager;

    private WeaponManager weaponManager;

    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (command is ShootingCommand)
        {
            ShootingCommand shootingCommand = (ShootingCommand)command;
            switch (shootingCommand.control)
            {
                case EInputControls.ShootMain:

                    //Parse target data
                    Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    Hand hand = FindObjectOfType<Hand>();
                    PlayerCharacter player = FindObjectOfType<PlayerCharacter>();

                    Vector2 playerVelocity = player.GetComponent<Rigidbody2D>().velocity;
                    playerVelocity.Normalize();

                    Vector2 direction = targetPos - new Vector2(hand.transform.position.x, hand.transform.position.y);

                    //Normalize the vector for the adding of force
                    direction.Normalize();

                    Vector2 myPos = new Vector2(
                        hand.transform.position.x + Mathf.Sign(direction.x) * (Mathf.Abs(direction.x) + 1) + (-.5f) * Math.Abs(playerVelocity.x),
                        hand.transform.position.y + direction.y + playerVelocity.y
                        );

                    Quaternion rotation = Quaternion.Euler(0, 0, -90 + (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
                    newRocket.GetComponent<RocketMover>().SetStartPosition(myPos);
                    weaponManager.ReuseObject(newRocket, myPos, rotation, direction);
                    break;
                    /*case EInputControls.ShootAlt:
                        siia vajalik kood
                        break;*/
            }
        }
    }

    public void OnUserInputKeyHold(EInputGroup group, ICommand command)
    {
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

    public void OnUserInputKeyUp(EInputGroup group, ICommand command)
    {
    }

    void Awake()
    {
        rigidBody = newRocket.GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start()
    {
        inputManager = GameManager.getInstance().getInputManager();
        weaponManager = GameManager.getInstance().getWeaponManager();

        //Creates a rocket pool with a size of rockets
        weaponManager.CreatePool(newRocket, 20);

        // Subscribe to all shooting input events
        inputManager.subscribeToInputGroup(EInputGroup.ShootingInput, this);

    }

    // Update is called once per frame
    void Update () {
	
	}
}
