using UnityEngine;
using System.Collections;
using System;

public class WeaponSpawner : MonoBehaviour, IUserInputListener {

    private InputManager inputManager;

    private WeaponManager weaponManager;

    private WSelectionManager weaponSelectionManager;

    // -----------------------

    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (command is ShootingCommand)
        {
            ShootingCommand shootingCommand = (ShootingCommand)command;
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

            switch (shootingCommand.control)
            {
                case EInputControls.ShootMain:

                    // Instead of using the newRocket field, create a manager where you can get main and alt weapons, that have already been set up with the correct components
                    // I.e. an empty projectile prefab, that a manager will attach a mover and an effect component to

                    weaponSelectionManager.getMainWeapon().GetComponent<AbsWeaponMover>().SetStartPosition(myPos);
                    weaponManager.ReuseObject(weaponSelectionManager.getMainWeapon(), myPos, rotation, direction);
                    break;
                    
                    

                case EInputControls.ShootAlt:
                    weaponSelectionManager.getAltWeapon().GetComponent<AbsWeaponMover>().SetStartPosition(myPos);
                    weaponManager.ReuseObject(weaponSelectionManager.getAltWeapon(), myPos, rotation, direction);
                    break;
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
    }

    // Use this for initialization
    void Start()
    {
        inputManager = GameManager.getInstance().getInputManager();
        weaponManager = GameManager.getInstance().getWeaponManager();
        weaponSelectionManager = GameManager.getInstance().getWSelectionManager();

        //Creates a pool for weapons
        weaponManager.CreatePool(weaponSelectionManager.getMainWeapon(), 20);
        weaponManager.CreatePool(weaponSelectionManager.getAltWeapon(), 20);

        // Subscribe to all shooting input events
        inputManager.subscribeToInputGroup(EInputGroup.ShootingInput, this);

    }

    // Update is called once per frame
    void Update () {
	
	}
}
