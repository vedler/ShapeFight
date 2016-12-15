using UnityEngine;
using System.Collections;
using System;

public class WeaponSpawner : Photon.MonoBehaviour, IUserInputListener {

    private InputManager inputManager;

    private WeaponManager weaponManager;

    private WSelectionManager weaponSelectionManager;

    // -----------------------

    private float timeStampMain;
    private float timeStampAlt;

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
            Vector2 handPos = new Vector2(hand.transform.position.x, hand.transform.position.y);
            Vector2 direction = targetPos - handPos;

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

                    //Check for cooldown
                    if (timeStampMain > Time.time)
                    {
                        return;
                    }

                    weaponSelectionManager.getMainWeapon().GetComponent<AbsWeaponMover>().SetStartPosition(myPos);
                    if(weaponSelectionManager.getMainWeaponName() == "newPellet")
                    {
                        shootPellets(myPos, rotation, targetPos, handPos, direction);
                    }
                    else
                    {
                        weaponManager.ReuseObject(weaponSelectionManager.getMainWeapon(), myPos, rotation, direction);
                    }

                    //Set cooldown
                    timeStampMain = Time.time + weaponSelectionManager.getMainWeapon().GetComponent<AbsWeaponMover>().cooldownPeriod;
                    break;
                    
                case EInputControls.ShootAlt:

                    //Check for cooldown
                    if (timeStampAlt > Time.time)
                    {
                        return;
                    }

                    weaponSelectionManager.getAltWeapon().GetComponent<AbsWeaponMover>().SetStartPosition(myPos);
                    if (weaponSelectionManager.getAltWeaponName() == "newPellet")
                    {
                        shootPellets(myPos, rotation, targetPos, handPos, direction);
                    }
                    else
                    {
                        weaponManager.ReuseObject(weaponSelectionManager.getAltWeapon(), myPos, rotation, direction);
                    }

                    //Set cooldown
                    timeStampAlt = Time.time + weaponSelectionManager.getAltWeapon().GetComponent<AbsWeaponMover>().cooldownPeriod;
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
        weaponManager.CreatePool(weaponSelectionManager.getMainWeapon(), 100);
        weaponManager.CreatePool(weaponSelectionManager.getAltWeapon(), 10);

        // If view is ours, attach us to the input manager
        if (photonView.isMine)
        {
            // Subscribe to all shooting input events
            inputManager.subscribeToInputGroup(EInputGroup.ShootingInput, this);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}

    //For the shotgun
    private void shootPellets(Vector2 myPos, Quaternion rotation, Vector2 targetPos, Vector2 handPos, Vector2 direction)
    {
        for (int i = 0; i < 10; ++i)
        {
            targetPos += new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f));
            direction = targetPos - handPos;
            direction.Normalize();
            weaponManager.ReuseObject(weaponSelectionManager.getMainWeapon(), myPos, rotation, direction);
        }
    }
}
