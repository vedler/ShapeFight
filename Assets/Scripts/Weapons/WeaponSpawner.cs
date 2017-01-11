using UnityEngine;
using System.Collections;
using System;

public class WeaponSpawner : Photon.MonoBehaviour, IUserInputListener {

    private InputManager inputManager;

    private WeaponManager weaponManager;

    private WSelectionManager weaponSelectionManager;

    // -----------------------

    Hand hand;
    private float timeStampMain;
    private float timeStampAlt;

    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (command is ShootingCommand)
        {
            ShootingCommand shootingCommand = (ShootingCommand)command;
            //Parse target data
            Vector2 targetPos = shootingCommand.targetPos;
            //Hand[] allHands = FindObjectsOfType<Hand>();
            //Hand hand = allHands[allHands.Length - 1];
            //PlayerCharacter player = hand.GetComponentInParent<PlayerCharacter>();

            Hand hand = GetComponentInChildren<Hand>();
            PlayerCharacter player = GetComponent<PlayerCharacter>();

            Vector2 playerVelocity = player.GetComponent<Rigidbody2D>().velocity;
            Vector2 handPos = new Vector2(hand.transform.position.x, hand.transform.position.y);
            Vector2 direction = targetPos - handPos;

            //Normalize the vector for the adding of force
            direction.Normalize();

            float sin = direction.y / direction.magnitude, cos = Mathf.Abs(direction.x / direction.magnitude);

            print(playerVelocity);

            Vector2 myPos = new Vector2(
                        hand.transform.position.x + Mathf.Sign(direction.x) * ( 2f * Mathf.Abs(cos) + 0.5f * (Math.Min(Math.Abs(playerVelocity.x), 2))), //( 1.5f + .5f * Math.Abs(playerVelocity.x)),
                        hand.transform.position.y + Mathf.Sign(direction.y) * ( 2f * Mathf.Abs(sin) + 0.5f * (Math.Min(Math.Abs(playerVelocity.y), 2)))
                        );

            Quaternion rotation = Quaternion.Euler(0, 0, -90 + (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));

            switch (shootingCommand.control)
            {
                case EInputControls.ShootMain:
                    // Instead of using the newRocket field, create a manager where you can get main and alt weapons, that have already been set up with the correct components
                    // I.e. an empty projectile prefab, that a manager will attach a mover and an effect component to

                    //weaponSelectionManager.getMainWeapon().GetComponent<AbsWeaponMover>().SetStartPosition(myPos);
                    
                                        //Check for cooldown
                    if (timeStampMain > Time.time)
                    {
                        return;
                    }

                    if(weaponSelectionManager.getMainWeaponName() == "Weapons/networkPellet")
                    {
                        shootPellets(myPos, rotation, targetPos, handPos, direction);
                    }
                    else
                    {
                        weaponManager.ReuseNetworkObject(weaponSelectionManager.mainWeaponName, myPos, rotation, direction);
                    }

                    //Set cooldown
                    timeStampMain = Time.time + FindConfig(weaponSelectionManager.altWeaponName).cooldownPeriod;
                    break;
                    
                case EInputControls.ShootAlt:

                    //Check for cooldown
                    if (timeStampAlt > Time.time)
                    {
                        return;
                    }

                    //weaponSelectionManager.getAltWeapon().GetComponent<AbsWeaponMover>().SetStartPosition(myPos);
                    if (weaponSelectionManager.getAltWeaponName() == "Weapons/networkPellet")
                    {
                        shootPellets(myPos, rotation, targetPos, handPos, direction);
                    }
                    else
                    {
                        weaponManager.ReuseNetworkObject(weaponSelectionManager.altWeaponName, myPos, rotation, direction);
                    }

                    //Set cooldown
                    timeStampAlt = Time.time + FindConfig(weaponSelectionManager.altWeaponName).cooldownPeriod;
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
        hand = gameObject.GetComponentInChildren<Hand>();
    }

    // Use this for initialization
    void Start()
    {
        inputManager = GameManager.getInstance().getInputManager();
        weaponManager = GameManager.getInstance().getWeaponManager();
        weaponSelectionManager = GameManager.getInstance().getWSelectionManager();

        //Creates a pool for weapons
        //weaponManager.CreatePool(weaponSelectionManager.getMainWeapon(), 20);
        //weaponManager.CreatePool(weaponSelectionManager.getAltWeapon(), 20);

        weaponManager.CreateNetworkPool(weaponSelectionManager.mainWeaponName, 20);
        weaponManager.CreateNetworkPool(weaponSelectionManager.altWeaponName, 20);

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
            //weaponManager.ReuseObject(weaponSelectionManager.getMainWeapon(), myPos, rotation, direction);
            weaponManager.ReuseNetworkObject(weaponSelectionManager.mainWeaponName, myPos, rotation, direction);
        }
    }

    public WeaponConfig FindConfig(string name)
    {
        switch (name)
        {
            case "Weapons/networkRocket":
                return weaponManager.rocketConfig;
            case "Weapons/networkBullet":
                return weaponManager.bulletConfig;
            case "Weapons/networkPellet":
                return weaponManager.pelletConfig;
        }

        return null;
    }
}
